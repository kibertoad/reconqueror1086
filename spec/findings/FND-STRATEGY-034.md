---
id: FND-STRATEGY-034
title: The brigand step 0x0004A61C loops its route and has no terrain or step-size stop
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004A61C..0x0004A955
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000973AD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000973B1
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0004A61C(r)` handles waypoints as `0x0004A300` does (FND-STRATEGY-011), except that reaching the
last point stores 0 in `+0x30` and goes on from the first. For a length of 0 or less it stores 0 in
`+0x30` and `+0x18`, 1 in `+0x0C` and returns 1. It projects the position plus the truncated previous
direction into `+0x44` and `+0x48`, reads the speed `w` of that tile's kind without testing for kind
9, and stores `ex / n * w * k` and `ey / n * w * k` in `+0x64` and `+0x68`. When `INT32(x) + INT32(dx)` or `INT32(y) + INT32(dy)` is
negative it stores 1.0 in both and adds 1 to both coordinates and returns 0. When both components lie
strictly between the floats at `0x000973B1` and `0x000973AD` it adds them to the position;
otherwise it stores 1 in `+0x0C`, 0 in `+0x30` and `+0x18`, and shows a message that a brigand is in
trouble. Both of these paths return 1.

## Interpretation

A brigand force patrols its route until its order ends. The limits are the largest finite floats, so
the move is refused only for an infinite or undefined step.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0004A61C..0x0004A955`, `0x000973AD`, `0x000973B1` in `CD:CONQUER.EXE`.
