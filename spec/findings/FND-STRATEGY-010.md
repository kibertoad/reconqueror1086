---
id: FND-STRATEGY-010
title: Mode 3 at 0x0003908C chases the live position of a player force
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003908C..0x00039426
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003908C(s, &done)` stores 0 in `done`. When player record `t = +0x14` has `+0x00` 0 it stores
`0xFFFF` in `done` and returns. Otherwise it stores the truncated floats of the player record in
`+0x3C` and `+0x40`, takes the differences to the truncation of `+0x5C + +0x64` and `+0x60 + +0x68`,
and stores them divided by `0x0002FC70` of their squared length in `+0x64` and `+0x68`, or 1.0 in
both when that length is 0 or less. It projects `(INT32(x) + INT32(dx), INT32(y) + INT32(dy))`
into `+0x44` and `+0x48`. For tile kind 9 it stores the origin's words `+0x05` and `+0x07` in `+0x3C`
and `+0x40`, divides the differences to the truncated position by the truncation of their floating
square root (or uses 1.0 when that is 0 or less), stores 1 in `+0x34`, adds the quotients to
`+0x5C` and `+0x60` and stores them in `+0x64` and `+0x68`. Otherwise it adds `dx * w * k` and
`dy * w * k` to the position and projects the truncated position into `+0x44` and `+0x48`.

## Interpretation

A pursuit never arrives on its own: it signals only when its target disappears. Meeting impassable
terrain turns it into a direct movement back to its castle after one unscaled step.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003908C..0x00039426` in `CD:CONQUER.EXE`.
