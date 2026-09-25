---
id: FND-VIEW-005
title: The first-person view starts with elevation 0x80, horizon at half the view height, and the view width as ray width
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005421F..0x000542E4
tool: Ghidra 12.1.3
environment: null
---

## Observation

Viewer setup `0x0005421F`..`0x000542E4` stores a camera elevation of `0x80`, a horizon row of
`height / 2` (integer division) and the view's width, which the raycaster uses to spread its
rays.

## Interpretation

The eye is half a block above the floor, and the horizon is the middle row of the view.

## Alternatives

None known.

## How to reproduce

Open `0x0005421F`; the three stores follow the view size computation.
