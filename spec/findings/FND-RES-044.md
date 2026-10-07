---
id: FND-RES-044
title: INST.EXE reads RESOURCE.CFG from the current directory through a line reader that splits each line at its first equals sign, trims both sides and compares keys without case
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:0138..1000:0151
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:2D61..20F3:2DA5
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2487:000D..2487:015E
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:000E..20F3:01B3
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:029C..20F3:03D9
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:1093..20F3:1733
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 27C0:01BB..27C0:0316
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 27C0:0510..27C0:0533
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2FD0:0492..2FD0:0593
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:63B6..1000:6471
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:1846..3583:18C1
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:1A04..3583:1A8F
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; Python byte searches of its load image
environment: null
---

## Observation

`CD:INST.EXE` is packed with LZEXE 0.91 and is read here unpacked, as the
build's manifest gives it. The unpacked file is an MZ file with a 0x39F0-byte
header. Addresses are `segment:offset` with the segment of the load image
plus 0x1000; the data segment is at load-image segment 0x2583 (3583 in the
notation, DS below). The routines in segments 1000 and 2FD0 named here are
run-time library routines.

Startup and main. The startup code at 1000:0138..1000:0151 pushes the
argument words and calls 20F3:2D61 (main). Before that call the startup
passes DS:5224 and DS:5272 in SI and DI to 1000:01ED (not read); the six-byte
entry at DS:5236 in that range holds type 1, priority 0x20 and the far
pointer 2487:000D. 2487:000D calls
2487:0047 with DS:55D8. 2487:0047 calls the base constructor 20F3:000E, sets
the object's vtable word to DS:1A14, passes eight handler objects (DS:1A90,
DS:1C40, DS:1DF0, DS:2150, DS:1FA0, DS:2300, DS:24B0 and DS:2660) to vtable
entry +0x00, stores DS:131C in the far pointer at +0x1EB and passes it to +0x00
too. 20F3:000E sets the vtable word to DS:1846, stores far pointers to
`install.scr` (DS:14D5) at +0x19A and `resource.cfg` (DS:14BD) at +0x1DA,
sets the word at +0x1F5 to 1 (20F3:00F8) and the byte at +0x213 to 0x3D,
`=` (20F3:017D). A search of the load image for the displacement bytes
0xDA 0x01 finds one store to +0x1DA, at 20F3:00B4, and reads at 20F3:03C7,
20F3:04FC and 20F3:27B1; the other two hits are a `push 0x1DA` and a store to
[bp-0x26].

DS:1A04 holds the far pointer 2583:55D8 in the file, with a relocation entry
on its segment word; the byte search for 0x04 0x1A finds `les` reads of it
and no store encoding. Main copies `argv[0]` into the string at DS:53AC and
calls vtable entry +0x1C of that object with `argc` and `argv`. The 31
entries of DS:1A14 equal those of DS:1846 except +0x18; +0x1C is 20F3:029C,
+0x48 is 20F3:1093 and +0x4C is 20F3:1102.

20F3:029C forms DS:5557 followed by `install.sip` (DS:14E8), and when DOS
find-first (1000:45DF) finds it opens it through 30C9:018F and keeps the
handle at +0x23A, otherwise stores 0 there. It then calls vtable entries
+0x44, +0x50, +0x54, +0x2C, +0x08 and 263F:0504 (not read), and at
20F3:03D6 calls +0x48 with the flag 0, the name at +0x1DA and the far pointer
object+0x20B. Every path from 20F3:0339 reaches that call when the callees
return; the one branch, on the word at +0x243, only skips two calls into
1E86. The disc root
has no `INSTALL.SIP`.

The call 20F3:1093 (flag, name, out). With the flag 0 it calls +0x4C with
the name, `out` and a null directory. With the flag nonzero it returns at
once unless the word at +0x1F5 is nonzero and the string at +0x194 is
nonempty, and then calls +0x4C with the name, a null `out` and the directory
string at +0x194.

The reader 20F3:1102 (name, out, directory):

- Path. With a directory it formats `%c:%s` (DS:15CE) from the byte at
  +0x1E6 and the directory, adds `\` (DS:15D4) when the last character is not
  one, and adds the name. With a null directory the path is the name alone.
- Open. It calls find-first on the path. When that returns 0 it opens the
  path with mode `rt` (DS:15D6) through 1000:48EF and returns when that
  fails. Otherwise it opens the name in the archive at +0x23A through
  30C9:0394 and returns when that gives null.
- Start. When `out` is not null it stores 0 at the first byte of the
  string `out` points to. It stores 0 in the first two bytes of the string
  at +0x18 of the object at +0x1EB.
- Lines. It reads a line into a buffer with the size 0x8D through
  1000:4543 (from the file) or 3216:04A4 (from the archive), ends the loop
  when that returns null, and appends `\n` when the line has none.
- Split. 27C0:01BB copies the line, trims it, and compares its first 7
  bytes with `default` (DS:2E4C) through 1000:643A, a byte comparison that
  keeps case. On a match it sets a flag word to 1 and deletes those 7 bytes;
  otherwise the flag is 0. It empties the key and value strings, looks for
  the byte at +0x213 (`=`) through 1000:6295, and returns with both empty
  when there is none. Otherwise the key is the text before the first `=` and
  the value the text after it, each trimmed. Trimming (27C0:0510, then
  2FD0:04B0 and 2FD0:052D) removes space, tab, LF and CR (DS:2E71) from the
  start, then from the end, never removing the first remaining byte from the
  end.
- Key case. 2FD0:0492 lower-cases the key through 1000:6416, which maps
  `A` to `Z` to `a` to `z`. Every key and value comparison below goes
  through 1000:63B6, which maps `a` to `z` to upper case on both sides before
  comparing.
- With a directory, a line whose flag is set is skipped.
- Keys, in the order tested:

  | Key | What the reader does with the value |
  |---|---|
  | `minems` | 1000:414A, low word to +0x209 |
  | `mincpu` | string copied to +0x203 |
  | `mode` | string copied to +0x1FF |
  | `directory` | string copied to +0x194 and to the string at +0x18 of the object at +0x1EB; the line buffer is emptied |
  | `mindos` | 1000:414A, low word to +0x207 |
  | `polyspace` | first `,`-separated token through 1000:65EA and 1000:414A to the doubleword at +0x1A2, the next token to +0x1A6 |
  | `space` | 1000:414A, doubleword to +0x19E |
  | `cmd` | string copied to +0x1F7 |
  | `cd` | when the value is `yes`, the word at +0x1FB is set to 1 |
  | `smartdrv` | when the value is `yes`, the word at +0x1FD is set to 1 |
  | `floppy` | with a null directory only: when the value is `yes`, the word at +0x1D6 is set to 1 |

- Any other key (and `floppy` read with a directory) goes to the handler
  objects: for each of the word count at +0x192, it calls vtable entry
  +0x2C of the far pointer at object+2+4i with the key, the value and the
  flag. When one returns nonzero the loop stops; the line buffer is emptied
  when the value is not `none` (DS:1636) and the flag is 0.
- After each line, when the line buffer is not empty and `out` is not null,
  the line is appended to the string `out` points to.
- At the end it closes the archive entry (30C9:041D) or the file (1000:43BE)
  and frees its strings.

## Interpretation

The installer `INST.EXE`, which `INSTALL.BAT` starts, reads `resource.cfg`
from the current directory, with no drive or directory added, after an
object built at startup reaches it from main. A line is split at its first
`=`; spaces and tabs around the key and the value, and around the line, are
dropped, so the column alignment of the shipped file is not needed. Keys
match without regard to case. `default` at the start of a line, in that case
only, marks the line. The shipped file's `directory`, `cd`, `minCPU`,
`minDOS`, `mode` and `smartDrv` lines are taken by the built-in keys; its
`videoDrv`, `joyDrv`, `memoryDrv` and `mouseDrv` lines go to the handler
objects.

## Alternatives

- The reader requires the key padded to column 10: ruled out; the key ends
  at the first `=` and is trimmed.
- Keys match with their stored case: ruled out; the key is lower-cased and
  compared without case.
- CONFIG.EXE or SETUP.EXE reads this file: not tested here; this finding
  locates the reader in INST.EXE and searched no other program.

## How to reproduce

Unpack `CD:INST.EXE` with `tools/evidence/unlzexe.mjs` at commit 0f1ac61,
check the unpacked xxh3 against the manifest, and disassemble it as 16-bit
code with the load image at segment 0x1000 and relocation targets marked, at
the ranges in Locations. Read the 31 far pointers at DS:1846 and at DS:1A14,
the far pointer at DS:1A04, the startup table from DS:5224 to DS:5272, and
the strings at DS:14BD, DS:14D5, DS:14E8, DS:15CE to DS:1636, DS:2E4C and
DS:2E71. Search the load image for the byte pairs 0xDA 0x01 and 0x04 0x1A and
decode each hit. Keep listings in ignored local storage.
