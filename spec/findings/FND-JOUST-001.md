---
id: FND-JOUST-001
title: The practice joust plays jousprac.SMK and paints frames of lance1.csf over it at y + 90, clipped to the movie area
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041164
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041227
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041CEC..0x00041D69
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041120
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000416D1..0x0004174F
tool: Ghidra 12.1.3
environment: null
---

## Observation

Worker `0x00041164` pushes the string `jousprac.SMK` at `0x000961DC` at `0x00041227` and opens
that movie. Caller `0x00041CEC` passes 1 to `0x00041120` at `0x00041D17`, which formats
`lance%1d.csf` (the string at `0x000961B0`) with it, loads the sprite file and keeps its handle;
the caller then runs the worker at `0x00041D46` and releases the handle at `0x00041D69`. At
`0x000416D1`..`0x0004174F` the worker takes the frame's width and height, shortens the width to
`0x27F - x` when `x + width` reaches `0x27F` and the height to `0x12B - y` when `y + height`
reaches `0x12B` (with `x` and `y` first raised to 0 when negative), sets that clip rectangle at
`(x, y + 90)` through `0x00064320`, and paints the frame at `(x, y + 90)` through
`0x00065130`.

## Interpretation

The movie is shown 90 pixels down the screen and the lance sprite is drawn over it in movie
coordinates. No part of a lance frame is drawn outside the movie area.

## Alternatives

None known.

## How to reproduce

Open `0x00041164`; the push of `0x000961DC` is at `0x00041227`, and the clip and paint calls
follow the frame selection at `0x0004169E`.
