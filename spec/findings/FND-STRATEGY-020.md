---
id: FND-STRATEGY-020
title: Routine 0x00011554 moves one player record along its route or toward its target
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00011554..0x00011CAA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063EC0..0x00063EDC
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00011554(i)` returns when record `i`'s `+0x00` is 0 or its `+0x0C` is not 0. When `+0x14` is not
0 it takes the hostile record at index `+0x14 & 0xFF` when bit `0x1000` is set and otherwise the
brigand record at that index (`0x000AA150 + 0x118 * n`). When that record's `+0x00` is 0 it stores 0
in `+0x18`, `+0x30`, `+0x0C` and `+0x14` and returns. Otherwise it stores the target's truncated
position in `+0x3C` and `+0x40` and the differences to the truncated own position divided by
`0x0002FC70` of their squared length in `+0x64` and `+0x68`. When `+0x14` is 0 it stores the pair
at `+0x70 + 8 * +0x30` in `+0x3C` and `+0x40`.

When both differences between the destination and the truncated position are below 6 in absolute
value, or a difference is 0 or more while its direction float is below 0, or 0 or less while the
float is above 0, it adds 1 to `+0x30`. When
`+0x30` then reaches `+0x18`, or `+0x14` is not 0, it stores 1 in `+0x0C`, 0 in `+0x18`, `+0x30`,
`+0x14` and the dword at `0x0009AE58`, calls three pointer routines and returns 0. Otherwise it
takes the next pair and a new direction as above.

It reads the speed float of the kind in `+0x58` (FND-STRATEGY-013), and computes the probe
`(INT32(x + INT32(dx) * INT32(w) * k), INT32(y + INT32(dy) * INT32(w) * k))`. When `+0x64` or `+0x68`
is above the float at `0x00090351` (40.0) or below the one at `0x00090355` (-40.0), or not ordered,
it stops the record. Otherwise it projects the probe into `+0x44` and `+0x48` and reads the tile. For
kind 9 it probes again at `(INT32(x) + INT32(dx * 50.0), INT32(y) + INT32(dy * 50.0))` with the double
at `0x00090359`; a negative coordinate stops the record, kind 9 there shows a message box after
focusing the map and stops it, and any other kind lets it move. To stop, it stores 0 in the old
selected record's `+0x04` and in `+0x14` and `+0x18`, `i` in `0x0009AE64`, 1 in `+0x04`, 0 in
`0x0009AE58`, 1 in `+0x0C`, calls the pointer routines, stores the truncated position in `+0x3C`
and `+0x40` and returns 1. To move, it stores the first probe's kind in `+0x58`, adds `dx * w * k`
and `dy * w * k` to `+0x5C` and `+0x60` and projects the truncated position into `+0x44` and `+0x48`.

## Interpretation

A player record walks its own route of up to 20 points or chases a hostile or brigand force. The
speed used is the kind of the cell the record last moved onto, which can be 9 when the record
stepped into water whose far side was dry. A chase ends at the first waypoint arrival, so the record
stops when it reaches the target's position.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00011554..0x00011CAA`, `0x00063EC0..0x00063EDC` in `CD:CONQUER.EXE`.
