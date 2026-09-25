---
id: FND-BATTLE-014
title: State 0 acquires a target, walks to a destination, or picks the nearest foe
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000270B4..0x00027474
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00027479..0x00027C59
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00027CE7..0x00027E16
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

State 0 first probes the four corners like `0x00028408`, with no offset. A living unit of the other
lane is stored in `+0x30` and ends the probe; once a corner has hit any other unit, a miss at a later
corner ends it, and misses before the first hit go on. When `+0x30` is then not -1, it calls
`0x000296B4` with the unit and the value left in EAX, and ends the unit's turn. That value is the index
just stored, or, when nothing was stored in this pass, the index of the last unit hit or 0 after a
final miss. With both `+0x0C` and `+0x10` at -1 it takes the automatic
branch. Otherwise it computes the octant to `(+0x0C, +0x10)` and turns one step when it differs from
`+0x14`. Facing it, it handles y and then x in the same pass. For y with `+0x10` not -1: equal
coordinates set `+0x10` to -1 and `+0x28` to 1. A remaining distance below 10 in absolute value probes
`0x00028408` with that offset; a miss moves to `+0x10` and sets `+0x10` to -1, a living foe (2) moves
there too, keeps `+0x10`, stores the hit in `+0x30` and calls `0x000296B4`, and a friend (1) sets
`+0x28` to 1 and `+0x10` to -1. A longer distance probes 10 for a knight and 5 otherwise; a miss moves
that far, a foe moves that far, stores the hit and calls `0x000296B4`, and a friend sets `+0x28` to 1
and moves `+0x10` 5 back toward the unit: moving down, a result below 0 becomes height minus 90;
moving up, a result at or above height minus 90 becomes 90. Every move adds 1 to `+0x18` modulo 5.
The x axis follows with the width. The automatic branch runs only for a unit with `+0x28` of 1,
`+0x20` above 0, and both lane totals above 0. It takes the living unit of the other lane with the
smallest `sqrt(dx * dx + dy * dy)`, computed with `fsqrt` and stored through `fistp` after a call to
`0x00063EC0`, below `0x7FFF`, the first on a tie. When that unit's `+0x0C` is -1 or equals the unit's
x, the unit's `+0x0C` becomes the other's x, and otherwise the other's `+0x0C`; the unit's `+0x10`
becomes the other's y when the other's `+0x10` is -1, and otherwise the other's `+0x10`.

## Interpretation

`0x00063EC0` likely sets truncation for the store, as the old notes assumed. A unit on automatic
heads for its nearest foe, or for where that foe is heading. A unit that hits a foe keeps moving on
the other axis in the same pass. A unit that kept a target from an earlier pass, because it was still
turning toward it, turns toward another unit in the next pass.

## Alternatives

Whether `0x00063EC0` truncates was not checked.

## How to reproduce

Disassemble `0x000270B4..0x00027474`, `0x00027479..0x00027C59`, `0x00027CE7..0x00027E16`.
