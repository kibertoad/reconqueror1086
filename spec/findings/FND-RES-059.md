---
id: FND-RES-059
title: An MVG entry holds the pointer shapes' hot spots; 0x00018850 loads it and the CSF of the same name
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018850..0x00018B0C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063EE0..0x00063F47
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063F9C..0x0006409D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006A5F0..0x0006A835
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00084784..0x0008494F
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x13CEDE8..0x13CEE32
tool: Python 3.14 LE object, page and fixup mapping of CONQUER.EXE with capstone 5.0.7 bounded 32-bit disassembly; Python container reader per FMT-RES-001 to FMT-RES-003
environment: null
---

## Observation

`0x00018850(name, from_archive)` frees the buffers of an earlier load when
`0x0009A68C` is 1, then builds `name` with the extension `MVG`. With
`from_archive` 0 it checks the file exists (error text `MVG files does not
exist`) and reads the whole file. Otherwise it finds the archive entry by name
through `0x0004957C` (error `MVG files does not exist in gob`), allocates the
entry's size (`unable to alloc memory for MVG loading`) and reads it through
`0x000495F4`. It then reads the bytes:

- dword `+0x00` must be `0xDEAD1234`, or the load returns 0;
- dword `+0x04` is a count, stored at `0x000B0BD0`;
- dword `+0x08` is a size: three buffers of that many bytes are allocated, at
  `0x000B07F0`, `0x000B07F4` and `0x000B07E0`;
- `count * 16` bytes from `+0x0C` are copied into a buffer allocated at
  `0x000B07E4`.

It then builds the same name with the extension `CSF`, loads that sprite set
through `0x00018430`, stores it at `0x0009E038`, sets `0x0009A68C` to 1, frees
the MVG bytes and returns 1. Any failed allocation returns 0.

The one call, from `0x00072180` at `0x00072200`, passes `from_archive` 1;
startup calls `0x00072180` with `FFMOUSE` and 1 (FND-UI-008). `0x0006A830`
returns the count. `0x00064030(n)` and its twin `0x00063F9C(n)` keep the
address of 16-byte record `n` at `0x000B07AC`, take the frame's width and
height from the frame's own first two words, and pass the frame and the
buffers at `0x000B07E0` and `0x000B07F0` to `0x0007CE40`. `0x00063EE0` draws
the pointer at the position at `0x000B07E8` and `0x000B07EC` less the
record's dwords `+0x08` and `+0x0C`. No located code reads the record's
dwords `+0x00` and `+0x04`.

`0x0006A5F0` reads the same header from a file, field by field, and is called
only from `0x00084784`. A search of object 1 for E8 calls and E9 jumps whose
target is `0x00084784`, and of every internal fixup for that target, finds
none; the same search finds the call from `0x0002A4E6` to `0x00072180`.
Computed calls through registers or tables built at run time were not
searched.

`ffmouse.mvg` decodes to 108 bytes: the magic, count 6, size 484, and six
records whose first two dwords are 20 and 20 and whose last two are
(0, 0), (10, 10), (1, 19), (10, 10), (10, 10) and (1, 0). `ffmouse.CSF` holds
six frames of 20 by 20 (FND-UI-008).

## Interpretation

An MVG entry is the companion of a sprite set: one 16-byte record per frame,
whose last two dwords are the frame's hot spot, and a size for the work
buffers the pointer drawing uses (484 bytes for 20-by-20 frames with room to
spare). The first two dwords match the frame size, but the game takes the
size from the frame.

## Alternatives

- The record's first two dwords give the drawn size: ruled out for the paths
  read, which take the size from the CSF frame.
- `0x00084784` is a live second pointer setup: no direct call or fixup reaches
  it.

## How to reproduce

Map CONQUER.EXE's LE objects through the object, page and fixup tables (the
data object loads at `0x00090000`; control: `FFMOUSE` at `0x00093A70`, as
FND-UI-008 gives). Disassemble the ranges above. Scan object 1 for E8 and E9
whose target is `0x00084784` and `0x00072180`, and list fixups to the same
targets. Decode `ffmouse.mvg` from C1086.GOB.
