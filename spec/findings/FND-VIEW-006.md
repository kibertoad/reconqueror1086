---
id: FND-VIEW-006
title: The raycaster forms its ray from a fixed forward basis and a lateral basis from the view column, and returns the first opaque candidate
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000470A8..0x00047589
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00046739..0x000467F7
tool: Ghidra 12.1.3
environment: null
---

## Observation

Raycaster `0x000470A8`..`0x00047589` takes an origin in 8.8 map units, a heading and a view
column `x`. It forms the local ray `(0x4000, ((0x400000 / width) * (x - width / 2)) >> 8)`,
with `width` the view width, and rotates it by the heading through `0x000447C4`. It calls
traversal `0x00045158` to collect candidates, then visits them in the order they were added. For
each it selects a surface from block fields `+0x2C` to `+0x38` by the face that was hit (north,
east, south and west in that order, as the selector at `0x00046739`..`0x000467F7` maps face
masks `0x100`, `0x200`, `0x400` and `0x800`), reverses the texture coordinate on north and east
faces, and projects the block's vertical extent as
`top = horizon - (upper - elevation) * width / depth` and
`bottom = horizon + (elevation - lower) * width / depth`, where `lower` and `upper` are block
fields `+0x1C` and `+0x20`. It samples the texture through `0x000444E8` and skips a candidate
whose sampled pixel is palette index 0. It returns the contact x and y, the vertical contact,
the depth, the block and the pixel coordinates of the first candidate that passes.

## Interpretation

A pick through the view returns the first thing actually drawn at that pixel. Transparent parts
of sprites and textures let the ray through to what lies behind them.

## Alternatives

The routine was once taken for a projection onto the floor plane that returns an empty floor
cell. It returns only contacts with projected block surfaces; the floor-plane reading is ruled
out.

## How to reproduce

Open `0x000470A8`; the constants `0x4000` and `0x400000` appear in the basis computation before
the call to `0x00045158`.
