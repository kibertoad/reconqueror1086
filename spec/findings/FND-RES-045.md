---
id: FND-RES-045
title: INST.EXE takes the switches M, Z, F and D from its command line and loads install.scr whole before it reads RESOURCE.CFG
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:06F7..20F3:0842
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2CBD:00EC..2CBD:010A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:0842..20F3:0891
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:000F..1C17:01AD
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:0D83..3583:0E29
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; a Python recursive-descent walk of direct calls over its load image
environment: null
---

## Observation

The unpacked file, its notation, the object at DS:55D8 and its vtables are as
FND-RES-066 gives them. 20F3:029C calls vtable entries +0x44, +0x50, +0x54,
+0x2C and +0x08 and 263F:0504 before its read of `resource.cfg` at
20F3:03D6.

Command line. Entry +0x44 is 20F3:06F7 (this, argc, argv). It copies
`argv[0]` over the string `install.exe` at DS:142E. For each later argument
whose first byte is `-` or `/`, it takes every following byte up to the NUL,
upper-cased through 2CBD:00EC (which maps `a` to `z` and nothing else), and:

- `M` sets the doubleword at +0x1E7 to 0, then adds the decimal digits that
  follow, each step multiplying by 10;
- `Z` sets the word at +0x198 to 1;
- `F` sets the word at +0x1F3 to 1;
- `D` sets the word at +0x243 to 1;
- any other byte is skipped.

An argument that starts with neither `-` nor `/` is skipped. The routine
returns after the last argument; it calls nothing that ends the run.

Entry +0x2C is 20F3:0842. When the doubleword at +0x1E7 is 0 it returns at
once; otherwise it calls vtable entry +0x08 of each object in the list at
object+2 (count at +0x192) with that number, and 263F:0009 with 1 after each.

Script load. Entry +0x54 is 1C17:000F. It clears the far pointer at
DS:534A, forms the string at DS:5557 followed by the name at +0x19A
(`install.scr`), and opens it through 1000:53CF with the mode word 0x4001.

- When the open succeeds it takes the length through 1000:463F into DS:534E,
  allocates that many bytes through 1000:0D4B into DS:534A, and when the
  allocation fails calls 1000:2557 with `Assertion failed: %s, file %s, line
  %d` (DS:0D83), `scriptFileBuf`, `code\command.cpp` and 0x7D. It then reads
  through 1000:5D1D, keeps the count it returns in DS:534E, closes the file
  through 1000:4271, and stores 0 at DS:534A plus that count.
- When the open fails and the archive handle at +0x23A is 0, it returns with
  DS:534A null.
- When the open fails and the handle is not 0, it opens the same path in
  the archive through 30C9:0394 (returning when that gives null), takes the
  length through 3216:0CD4, allocates and asserts as above (line 0x8A), reads
  through 3216:05B6, calls 263F:0133 with `Can't read script file!`
  (DS:0E11) when that returns 0, closes the entry and stores the closing 0.

Exits. The startup calls 1000:085B with main's return value, and the
message routine 20F3:2DBD calls 1000:027A after printing; neither routine was
read, and both are taken here as the ways the run ends. A search of the relocation
table for far calls to them finds nine callers, among them 263F:0133 and
20F3:1734 (vtable entry +0x14). A recursive descent from the six entries
20F3:06F7, 20F3:2975, 1C17:000F, 20F3:0842, 2821:00BB and 263F:0504,
following direct near and far calls and jumps, reaches 217 functions: of the
nine callers it reaches only 263F:0133, from 1C17:000F, and it reaches
1000:027A through 1000:2557. It also reaches the stack check 1000:3F57 and
the routines behind it. It meets eleven indirect calls in functions at load-image offsets
0x10000 and above, which it does not follow: in 20F3:0842 (+0x08 of the
listed objects), in 2821:00BB, and in the functions at 0x1D0FC, 0x1E08E,
0x1F75B, 0x21ADC, 0x22167 (four) and 0x22560. It meets nine more in
functions below 0x6200.

## Interpretation

`inst.exe -f`, as `INSTALL.BAT` runs it, sets one flag (+0x1F3) and nothing
else on the command line. Before it reads `resource.cfg`, the installer loads
`install.scr` whole into memory as text; a failed allocation stops it with an
assertion message. Along direct calls, the run ends before the
`resource.cfg` read only on such failures; the indirect calls listed above
are not resolved, so this does not show that the read is always reached.

## Alternatives

- `-f` names a file: ruled out; each byte after the `-` is a switch letter,
  and `F` only sets a word.
- `install.scr` is read line by line when it is interpreted: ruled out for
  this load, which reads the whole file into one buffer first.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-066 gives. Disassemble the ranges in
Locations as 16-bit code with the load image at segment 0x1000 and read the
strings at DS:0D83 to DS:0E29 and DS:142E. List the relocation entries whose
target word follows a 0x9A byte and whose far pointer is 0000:085B or
0000:027A. Walk the six entries named above by recursive descent, starting a
function at every direct call target, stopping each path at a return, an
unconditional jump or an invalid byte, and list the functions reached and the
indirect calls met. Keep listings in ignored local storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.
