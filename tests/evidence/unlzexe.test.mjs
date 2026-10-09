import { test } from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, writeFileSync, readFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { spawnSync } from "node:child_process";
import { unpackLzexe } from "../../tools/evidence/unlzexe.mjs";
import { readerTool } from "@scientific-method/executable-reader";

// Writes the LZEXE 0.91 bit stream: a control word is reserved where the reader will read it,
// which is at the start and right after the byte operand that follows its predecessor's last bit.
class Writer {
  constructor() {
    this.out = [0, 0];
    this.word = 0;
    this.slot = 0;
    this.count = 0;
  }
  bit(b) {
    this.word |= b << this.count;
    this.count += 1;
    if (this.count === 16) {
      this.out[this.slot] = this.word & 0xff;
      this.out[this.slot + 1] = this.word >> 8;
      this.slot = this.out.length;
      this.out.push(0, 0);
      this.word = 0;
      this.count = 0;
    }
  }
  byte(b) {
    this.out.push(b);
  }
  literal(b) {
    this.bit(1);
    this.byte(b);
  }
  short(count, distance) {
    this.bit(0); this.bit(0);
    this.bit((count - 2) >> 1); this.bit((count - 2) & 1);
    this.byte((0x100 - distance) & 0xff);
  }
  long(count, distance) {
    const v = 0x2000 - distance;
    this.bit(0); this.bit(1);
    const lo = v & 0xff;
    const hiBits = (v >> 5) & 0xf8;
    if (count <= 9 && count >= 3) {
      this.byte(lo); this.byte(hiBits | (count - 2));
    } else {
      this.byte(lo); this.byte(hiBits); this.byte(count - 1);
    }
  }
  marker(n) {
    this.bit(0); this.bit(1);
    this.byte(0); this.byte(0); this.byte(n);
  }
  finish() {
    this.marker(0);
    this.out[this.slot] = this.word & 0xff;
    this.out[this.slot + 1] = this.word >> 8;
    return Buffer.from(this.out);
  }
}

function packed({ tag = "LZ91", stream, relocs, minAlloc = 0x40 }) {
  const paras = Math.ceil(stream.length / 16);
  const decompressor = Buffer.alloc(0x158 + relocs.length);
  [0x0005, 0x0001, 0x0200, 0x0010, paras, 0, 0, 0].forEach((v, i) => decompressor.writeUInt16LE(v, i * 2));
  Buffer.from(relocs).copy(decompressor, 0x158);
  const image = Buffer.concat([stream, Buffer.alloc(paras * 16 - stream.length), decompressor]);
  const total = 0x20 + image.length;
  const header = Buffer.alloc(0x20);
  header.writeUInt16LE(0x5a4d, 0);
  header.writeUInt16LE(total % 512, 2);
  header.writeUInt16LE(Math.ceil(total / 512), 4);
  header.writeUInt16LE(2, 8);
  header.writeUInt16LE(minAlloc, 0x0a);
  header.writeUInt16LE(0xffff, 0x0c);
  header.writeUInt16LE(paras, 0x16);
  header.writeUInt16LE(0x1c, 0x18);
  header.write(tag, 0x1c, "latin1");
  return Buffer.concat([header, image]);
}

function sample() {
  const original = [];
  const w = new Writer();
  const lit = (b) => { w.literal(b); original.push(b); };
  const copy = (count, distance, form) => {
    w[form](count, distance);
    for (let k = 0; k < count; k += 1) original.push(original[original.length - distance]);
  };
  for (let i = 0; i < 300; i += 1) lit((i * 7 + 3) & 0xff);
  copy(2, 1, "short");
  copy(5, 256, "short");
  copy(3, 300, "long");
  copy(9, 0x2000 > original.length ? original.length : 0x2000, "long");
  w.marker(1);
  copy(40, 17, "long");
  copy(256, 299, "long");
  lit(0x42);
  return { stream: w.finish(), original: Buffer.from(original) };
}

// relocations at 0x012, 0x025 and 0x148 (distance 0x123 written as a word)
const RELOCS = [0x12, 0x13, 0x00, 0x23, 0x01, 0x00, 0x01, 0x00];

test("unlzexe expands every code form and rebuilds the header and relocations", (t) => {
  const { stream, original } = sample();
  const { bytes, info } = unpackLzexe(packed({ stream, relocs: RELOCS }));
  const u16 = (o) => bytes.readUInt16LE(o);
  assert.equal(u16(0), 0x5a4d);
  assert.equal(u16(6), 3);
  assert.equal(u16(8), 3); // 0x1C + 12 bytes of relocations, padded to 0x30
  assert.deepEqual([u16(0x1c), u16(0x1e), u16(0x20), u16(0x22), u16(0x24), u16(0x26)], [2, 1, 5, 2, 8, 0x14]);
  assert.deepEqual([u16(0x14), u16(0x16), u16(0x10), u16(0x0e), u16(0x18), u16(0x0c)], [5, 1, 0x200, 0x10, 0x1c, 0xffff]);
  assert.deepEqual(bytes.subarray(0x30), original);
  assert.equal(bytes.length, 0x30 + original.length);
  assert.deepEqual([u16(2), u16(4)], [bytes.length % 512, Math.ceil(bytes.length / 512)]);
  assert.equal(info.relocations, 3);
  assert.ok(info.compressedEnd <= info.decompressorStart);
  const packedParas = Math.ceil((Math.ceil(stream.length / 16) * 16 + 0x158 + RELOCS.length) / 16);
  assert.equal(u16(0x0a), Math.max(0, packedParas + 0x40 - Math.ceil(original.length / 16)));

  const dir = mkdtempSync(join(tmpdir(), "unlzexe-")); t.after(() => rmSync(dir, { recursive: true, force: true }));
  const input = join(dir, "in.exe"), output = join(dir, "out.exe");
  writeFileSync(input, packed({ stream, relocs: RELOCS }));
  const cli = spawnSync(process.execPath, [resolve(import.meta.dirname, "../../tools/evidence/unlzexe.mjs"), input, output], { encoding: "utf8" });
  assert.equal(cli.status, 0, cli.stderr);
  assert.equal(JSON.parse(cli.stdout).size, bytes.length);
  assert.equal(JSON.parse(cli.stdout).tool, readerTool());
  assert.equal(JSON.parse(cli.stdout).layout, 1);
  assert.equal(JSON.parse(cli.stdout).packer, "LZEXE 0.91");
  assert.deepEqual(readFileSync(output), bytes);
  const again = spawnSync(process.execPath, [resolve(import.meta.dirname, "../../tools/evidence/unlzexe.mjs"), input, output], { encoding: "utf8" });
  assert.notEqual(again.status, 0, "an existing output file is not overwritten");
});

test("unlzexe rejects unknown packing, bad copies, truncated streams and stray relocations", () => {
  const { stream } = sample();
  assert.throws(() => unpackLzexe(packed({ tag: "PKLI", stream, relocs: RELOCS })), /No packer the reader unpacks/);
  const early = new Writer(); early.literal(1); early.short(2, 5);
  assert.throws(() => unpackLzexe(packed({ stream: early.finish(), relocs: RELOCS })), /reaches 4 bytes before the start/);
  // one paragraph: a control word of sixteen literal bits and only fourteen literal bytes
  const noEnd = Buffer.from([0xff, 0xff, ...Array.from({ length: 14 }, (_, i) => i)]);
  assert.throws(() => unpackLzexe(packed({ stream: noEnd, relocs: RELOCS })), /before its end mark/);
  assert.throws(() => unpackLzexe(packed({ stream, relocs: [0x00, 0x00, 0x00, 0x05, 0x00, 0x01, 0x00] })), /past the unpacked load module/);
  assert.throws(() => unpackLzexe(Buffer.from("MZ")), /expected MZ/);
});
