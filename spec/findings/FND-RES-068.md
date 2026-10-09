---
id: FND-RES-068
title: Every use of the name FFONTA2.FNT ends in a property call that stores nothing, so the game never reads the .FNT entry
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00067980..0x000679A6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00067DBD..0x00067DC6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001A118..0x0001A13F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001AA16..0x0001AA3F
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B105B4..0x1B105E4
tool: Python 3.14 LE object, page and fixup mapping of CONQUER.EXE with capstone 5.0.7 bounded 32-bit disassembly; Python container reader per FMT-RES-001 to FMT-RES-003
environment: null
---

## Observation

The data object holds the string `FFONTA2.FNT` seven times. Internal fixups
reach them from 13 instructions. Twelve push the string as the third argument
of `0x00067980(object, 3, name)`. The thirteenth, at `0x00021F2C`, passes it
to `0x0001A118`, which copies it to `0x0009A71C`; the only readers of
`0x0009A71C` are `0x0001AA29` and `0x0001AB52`, which pass it to
`0x00067980` with property 3 as well.

`0x00067980` jumps through a 24-entry table at `0x00067920` for properties 0
to 23 (any other returns 0). The table's entries, read from the fixups of the
build's bytes, send property 3 to `0x00067DBD`, the same target as the
out-of-range case, which returns 0 and stores nothing. Entry 4, for
comparison, goes to `0x000679C9`, which stores its argument.

The entry `ffonta2.fnt` is index `0x183` of C1086.GOB, stored, 49 bytes: a
dword 1, the words 11, 14 and 19, the names `FFONTA` and `SVG` in fixed
fields, and 19 punctuation characters. No `push 0x183` occurs in object 1.
The executable names no other `.FNT` file; `FNT%d.PCX` and `FNT6.PCX` name
pictures. The game draws text with sprite sets such as `ffonta.CSF`
(index `0x182`).

## Interpretation

The `.FNT` descriptor is left over from an earlier text system: the code that
names it hands the name to a property the shipped object type ignores, and
nothing reads the entry. It needs no format entry; FMT-RES-001's Coverage
lists it.

## Alternatives

This replaces FND-RES-061. Its partial-range endpoint was written on the last
body byte identified by the verified metadata in FND-RES-063, while its text
did not settle the intended endpoint. The replacement explicitly includes that
byte and writes the exclusive boundary. Every behavioral observation and its
qualification is retained; no complete reading is claimed.


- The entry is read by index: no constant push of `0x183` was found; an index
  computed at run time was not searched.
- `0x00067980` is not the only property setter: every one of the 13
  references was read and each ends at `0x00067980`.

## How to reproduce

Map CONQUER.EXE as FND-RES-067 gives. List the fixups to each copy of the
string and disassemble each source. Read the table at `0x00067920` through
the fixups of its 24 dwords. Search object 1 for the bytes of `push 0x183`.
Read the `ffonta2.fnt` entry of C1086.GOB.

Verify the body boundaries against FND-RES-063 using its read-only metadata
procedure. Those extents bound this recorded scope and do not establish the
behavior or completeness of a reading.
