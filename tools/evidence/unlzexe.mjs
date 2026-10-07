#!/usr/bin/env node
// Unpacks a DOS executable packed by LZEXE 0.91 into a plain MZ file, so its code can be read at
// the addresses the unpacked program runs at. Usage:
//   node tools/evidence/unlzexe.mjs <packed.exe> <unpacked.exe>
// It prints the unpacked file's size and xxh3, the identity a build's `unpacked` item records.
//
// The packed file holds, before the decompressor's CS:0, the compressed load module, and at CS:0
// a header of eight words: the original IP, CS, SP and SS, the compressed module's size in
// paragraphs, and three words the unpacker does not need. The relocation table is at CS:0x158.
// The compressed module is a bit stream of 16-bit little-endian control words, each consumed from
// bit 0, with byte operands read from the stream between them; a new control word is read as soon
// as the previous one's sixteenth bit is taken:
//   1                 copy one byte
//   0 0 b1 b0, byte   copy (b1 b0) + 2 bytes from 0x100 - byte back
//   0 1, lo, hi       distance 0x2000 - (lo | (hi & 0xF8) << 5); count (hi & 7) + 2, or when that
//                     is 0, a count byte: 0 ends the module, 1 marks a segment change and copies
//                     nothing, anything else copies count + 1 bytes
// The relocation table is a list of bytes giving the distance from the previous relocation; 0 is
// followed by a word: 0 adds 0xFFF0 to the position, 1 ends the table, and any other value is the
// distance. Each position is written as segment:offset with an offset of 0 to 15.
//
// The unpacked file's header is this tool's own: 28 bytes of fields, then the relocations, padded
// with zeros to a multiple of 16 bytes. Minimum allocation keeps the packed program's total
// memory (packed load module plus its minimum allocation) when the unpacked module is smaller,
// and is 0 otherwise; maximum allocation, SS, SP, CS and IP come from the packed file and its
// header; the checksum and overlay number are 0. LZEXE 0.90 files (`LZ09`) are rejected.
import { readFileSync, writeFileSync, statSync } from "node:fs";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { sourceXxh3 } from "@scientific-method/executable-reader";

const MAX_INPUT = 1024 * 1024;
const MAX_OUTPUT = 1024 * 1024;
const MAX_RELOCATIONS = 16384;
const RELOCATION_TABLE = 0x158;

function fail(message) {
  throw new Error(`unlzexe: ${message}`);
}

class Reader {
  constructor(bytes, start, end) {
    this.bytes = bytes;
    this.pos = start;
    this.end = end;
  }
  byte() {
    if (this.pos >= this.end) fail(`compressed data runs past offset 0x${this.end.toString(16)}`);
    return this.bytes[this.pos++];
  }
  word() {
    const lo = this.byte();
    return lo | (this.byte() << 8);
  }
}

class Bits {
  constructor(reader) {
    this.reader = reader;
    this.buf = reader.word();
    this.count = 16;
  }
  bit() {
    const b = this.buf & 1;
    this.count -= 1;
    if (this.count === 0) {
      this.buf = this.reader.word();
      this.count = 16;
    } else {
      this.buf >>= 1;
    }
    return b;
  }
}

function expand(reader) {
  const out = [];
  const bits = new Bits(reader);
  for (;;) {
    if (out.length > MAX_OUTPUT) fail(`load module exceeds ${MAX_OUTPUT} bytes`);
    if (bits.bit()) {
      out.push(reader.byte());
      continue;
    }
    let count;
    let distance;
    if (!bits.bit()) {
      count = ((bits.bit() << 1) | bits.bit()) + 2;
      distance = 0x100 - reader.byte();
    } else {
      const lo = reader.byte();
      const hi = reader.byte();
      distance = 0x2000 - (lo | ((hi & 0xf8) << 5));
      count = (hi & 7) + 2;
      if (count === 2) {
        const n = reader.byte();
        if (n === 0) break;
        if (n === 1) continue;
        count = n + 1;
      }
    }
    if (distance > out.length) fail(`copy reaches ${distance} bytes back from output byte ${out.length}`);
    for (let k = 0; k < count; k += 1) out.push(out[out.length - distance]);
  }
  return Buffer.from(out);
}

function relocations(bytes, start, end) {
  const reader = new Reader(bytes, start, end);
  const list = [];
  let segment = 0;
  let offset = 0;
  for (;;) {
    let span = reader.byte();
    if (span === 0) {
      span = reader.word();
      if (span === 0) {
        segment += 0x0fff;
        continue;
      }
      if (span === 1) break;
    }
    offset += span;
    segment += offset >> 4;
    offset &= 0x0f;
    if (segment > 0xffff) fail("relocation segment exceeds 0xFFFF");
    list.push([offset, segment]);
    if (list.length > MAX_RELOCATIONS) fail(`more than ${MAX_RELOCATIONS} relocations`);
  }
  return list;
}

export function unpackLzexe(bytes) {
  if (bytes.length < 0x20 || bytes.length > MAX_INPUT) fail("not a file of 32 bytes to 1 MiB");
  const w = (o) => bytes.readUInt16LE(o);
  if (w(0) !== 0x5a4d) fail("no MZ signature");
  const tag = bytes.toString("latin1", 0x1c, 0x20);
  if (tag === "LZ09") fail("LZEXE 0.90 is not supported");
  if (tag !== "LZ91") fail(`no LZ91 tag at 0x1C (found ${JSON.stringify(tag)})`);
  const headerBytes = w(8) * 16;
  const lastPage = w(2);
  const pages = w(4);
  const fileImageEnd = pages * 512 - (lastPage ? 512 - lastPage : 0);
  if (pages === 0 || fileImageEnd > bytes.length || headerBytes >= fileImageEnd) fail("MZ size fields are outside the file");
  const packedImage = fileImageEnd - headerBytes;
  const csStart = headerBytes + w(0x16) * 16;
  if (csStart + 16 > fileImageEnd || csStart + RELOCATION_TABLE > fileImageEnd) fail("decompressor header is outside the load module");
  const info = {
    ip: w(csStart), cs: w(csStart + 2), sp: w(csStart + 4), ss: w(csStart + 6),
    compressedParagraphs: w(csStart + 8),
  };
  const dataStart = csStart - info.compressedParagraphs * 16;
  if (dataStart < headerBytes) fail("compressed module starts before the load module");
  const reader = new Reader(bytes, dataStart, csStart);
  const image = expand(reader);
  const relocs = relocations(bytes, csStart + RELOCATION_TABLE, fileImageEnd);
  for (const [off, seg] of relocs) {
    if (seg * 16 + off + 2 > image.length) fail(`relocation ${seg.toString(16)}:${off.toString(16)} is outside the load module`);
  }
  const fields = 0x1c;
  const header = Math.ceil((fields + relocs.length * 4) / 16) * 16;
  const total = header + image.length;
  const out = Buffer.alloc(total);
  const paragraphs = (n) => Math.ceil(n / 16);
  const packedMemory = paragraphs(packedImage) + w(0x0a);
  const minAlloc = Math.max(0, packedMemory - paragraphs(image.length));
  out.writeUInt16LE(0x5a4d, 0);
  out.writeUInt16LE(total % 512, 2);
  out.writeUInt16LE(Math.ceil(total / 512), 4);
  out.writeUInt16LE(relocs.length, 6);
  out.writeUInt16LE(header / 16, 8);
  out.writeUInt16LE(Math.min(minAlloc, 0xffff), 0x0a);
  out.writeUInt16LE(w(0x0c), 0x0c);
  out.writeUInt16LE(info.ss, 0x0e);
  out.writeUInt16LE(info.sp, 0x10);
  out.writeUInt16LE(0, 0x12);
  out.writeUInt16LE(info.ip, 0x14);
  out.writeUInt16LE(info.cs, 0x16);
  out.writeUInt16LE(fields, 0x18);
  out.writeUInt16LE(0, 0x1a);
  relocs.forEach(([off, seg], i) => {
    out.writeUInt16LE(off, fields + i * 4);
    out.writeUInt16LE(seg, fields + i * 4 + 2);
  });
  image.copy(out, header);
  return {
    bytes: out,
    info: { ...info, relocations: relocs.length, loadModule: image.length, compressedEnd: reader.pos, decompressorStart: csStart },
  };
}

if (import.meta.url === pathToFileURL(resolve(process.argv[1] ?? "")).href) {
  const [input, output] = process.argv.slice(2);
  if (!input || !output) {
    console.error("Usage: node tools/evidence/unlzexe.mjs <packed.exe> <unpacked.exe>");
    process.exit(2);
  }
  try {
    if (statSync(input).size > MAX_INPUT) fail("input exceeds 1 MiB");
    const { bytes, info } = unpackLzexe(readFileSync(input));
    writeFileSync(output, bytes, { flag: "wx" });
    console.log(JSON.stringify({ size: bytes.length, xxh3: sourceXxh3(bytes), ...info }));
  } catch (error) {
    console.error(error.message);
    process.exit(1);
  }
}
