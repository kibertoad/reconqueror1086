---
id: FND-BATTLE-015
title: The pointer dispatcher scrolls, shows hover status, and routes codes 2, 3, 6 and 7
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000264F8..0x000265CC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000265CC..0x00026787
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000267A4..0x00026B67
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00025F08..0x00025FB5
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

First it scrolls by the pointer position in `0x000B07E8` and `0x000B07EC`: `x <= 5` subtracts 10 from
`0x000A9CAC` when that is positive; `x >= w - 5` adds 10 when `W - w - 10 > 0x000A9CAC`, with `w` the
dword at `0x000B0854` and `W` the one at `0x000A9CB4`; the same for y with `0x000A9CA8`, the dwords at
`0x000B0868` and `0x000A9C88`, and a reserve of 50 at the bottom. Each move sets `0x000A9CC0` to 1. It then hit-tests the unit rectangles
at the scrolled point: a player unit shows `OUR %3d%%` with its `+0x20`, a foe shows `FOE`, and either
sets `0x0009AA88`. On the first miss after that it shows `WINNING` when `0x000A9C94 <= 0x000A9C70` and
`LOSING` otherwise, and clears `0x0009AA88`. The status panel is 83 by 20 at `(0x000A9C74 + 160, view height - 25)`. Code 2 selects a unit; code 3 tests the three control
rectangles `(m + 400, H - 34, 84, 31)`, `(m + 524, H - 34, 41, 31)` and `(m + 566, H - 34, 41, 31)`
at the raw point, with `m` at `0x000A9C74` (160 for a width of 1024, 80 for 800, 0 otherwise); a
miss falls through to selection. The first control, when `0x000A9C8C` is 0, sets `0x000A9C7C` to 0 and
`0x000A9C8C` to 1; later it asks `Do you want to retreat?` and returns 1 on yes. The second writes
`+0x28 = 1` to every selected unit, the third to every unit with `+0x20 > 0`. Codes 6 and 7 write the
destination for every selected unit: y is limited to 45 at least and then to `height - 85` at most,
and each gets `+0x28 = 0`, `+0x0C = x + 0x000A9CAC` and `+0x10 = y + 0x000A9CA8`. Selection at the
scrolled point accepts only lane 0 with `+0x20 > 0`, and appends its index to the byte list at
`0x000A9C84` (count in byte 0) unless it is already there.

## Interpretation

Selection never removes a unit; the control strip and scrolling use different coordinates.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000264F8..0x000265CC`, `0x000265CC..0x00026787`, `0x000267A4..0x00026B67`, `0x00025F08..0x00025FB5`.
