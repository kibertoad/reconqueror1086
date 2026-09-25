---
id: FND-BATTLE-006
title: Formation code 3 splits the player's first category, and the foe is placed on a coin toss
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029209..0x00029448
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029449..0x00029643
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00028C28..0x00028C37
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Code 3 takes `h = c0 / 2` of the count `c0` of the player's first category, and with
`rows = height / 60`, `k = n - c0` and `b = 60 * (h / 4 + 1) + 60 * (k / rows)`, places the first `h`
units of that category at `x = b - 60 * (i / 4) + 120`, `y = 60 * (i % 4) + 30`, the other `c0 - h` at
`x = b - 60 * (j / 4) + 120`, `y = 60 * (j % 4) + 480` with `j` counted from 0 again, and the player's
other units at `x = b - 60 * (j / rows)`, `y = 60 * (j % rows) + 30`, again from 0. Every code, and a
code above 3, then reaches `0x00029449`, which draws `0x0006B3F1` once, takes the signed remainder by
2, and jumps through the four-entry table at `0x00028C28`. For 0 the foe's unit `i`, counted from 0,
gets `x = width - (60 * (i / rows) + 60)`; for 1 it gets
`x = width - (60 * (m / rows) + 60) + 60 * (i / rows)`, with `m` the foe's unit count and the width
the dword at `0x000A9CB4`; both use `y = 60 * (i % rows) + 30`. Entry 2 is a mirrored wedge and entry 3
returns; a non-negative draw reaches neither.

## Interpretation

The foe stands in columns from the right edge, in one of two orders, whatever formation the player
picked.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00029209..0x00029448`, `0x00029449..0x00029643`, `0x00028C28..0x00028C37`.
