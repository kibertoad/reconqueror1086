---
id: FND-VIEW-008
title: The candidate helper keeps at most 31 contacts, and a switch table gives each block kind its shape
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00044F7C..0x00045061
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00044A28
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00044CC0..0x00044F61
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00034A0C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000AC1A0..0x000AC3A0
tool: Ghidra 12.1.3
environment: null
---

## Observation

Helper `0x00044F7C` stores each candidate's contact x, contact y, packed face and block in the
arrays at `0x000AC220`, `0x000AC1A0`, `0x000AC2A0` and `0x000AC320` (object 2 offsets `0x1C220`,
`0x1C1A0`, `0x1C2A0` and `0x1C320`). Each array has 32 slots, but at `0x00045061` the helper
stops when the count after incrementing reaches `0x1F`, so at most 31 are used. Intersection
routine `0x00044A28` jumps through the table at `0x00034A0C` on the block's kind: kind 0 gives
no candidate, kinds 1 and 4 intersect the cell as a box, kinds 2 and 3 a plane through the
middle of the cell (along x and along y), and kinds 5 and 6 the two diagonals.
`0x00044CC0`..`0x00044F61` records the face hit as mask `0x100` (north), `0x200` (east), `0x400`
(south) or `0x800` (west).

## Interpretation

Block kinds are shapes: empty, solid cell, thin walls through the middle of the cell, a
billboard sprite, and diagonal walls.

## Alternatives

None known.

## How to reproduce

Open `0x00044F7C` and note the four array bases; the comparison with `0x1F` is at `0x00045061`.
Follow the indirect jump in `0x00044A28` to the table at `0x00034A0C`.
