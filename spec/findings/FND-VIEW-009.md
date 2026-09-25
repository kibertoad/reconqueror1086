---
id: FND-VIEW-009
title: For a kind-4 block the raycaster uses the block centre's forward distance as depth and chooses a sprite angle from its heading
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00047308..0x00047419
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00047308..0x0004735A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004737B..0x000473C3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000473CE..0x00047419
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x00047308`..`0x0004735A` the raycaster forms a kind-4 block's centre as
`(cell << 8) + 0x80 + offset` on each axis (offsets at block `+0x24` and `+0x28`), subtracts the
origin, and rotates the difference by the negative of the view heading through `0x000447C4`. At
`0x0004737B`..`0x000473C3` it takes the rotated forward coordinate as the depth and computes the
texture column as `((ray_lateral * depth) >> 14) - centre_lateral + 0x80`, rejects the
candidate when that is outside 0 to `0xFF`, and shifts it right by `8 - shift` (block `+0x14`).
At `0x000473CE`..`0x00047419` it selects the sector
`((((surface3 - view_heading + 0x80 / divisions) & 0xFF) * divisions) >> 8)`, where
`surface3` is block `+0x38` and `divisions` is block `+0x34`. When behaviour bit 2 (`0x04`) is
set and the sector is above `divisions / 2`, it uses `divisions - sector` and mirrors the
texture column. The texture is block `+0x2C`'s low word plus the sector.

## Interpretation

Sprites face the viewer and their depth is the distance of their centre along the view
direction. A sprite shows one of several stored angles, and for symmetrical figures the far
half of the angles reuses the near half mirrored.

## Alternatives

None known.

## How to reproduce

Open `0x00047308`, reached from `0x000470A8` for a candidate whose block kind is 4.
