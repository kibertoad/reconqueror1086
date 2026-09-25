---
id: FND-BATTLE-005
title: Formation codes 0, 1 and 2 place the player's units in columns, reversed columns or a wedge
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002904B..0x00029208
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The code is the dword at `0x000A9CBC`. With `rows = height / 60`, where the height is the dword at
`0x000A9C88`, code 0 at `0x0002904B` places player unit `i` at `x = 60 * (i / rows) + 60`,
`y = 60 * (i % rows) + 30`. Code 1 at `0x000290BA` keeps that y and uses
`x = 60 * (n / rows) + 60 - 60 * (i / rows)`, where `n` is the number of player units. Code 2 at
`0x00029132` counts `k` up from 0 until `k * (k + 1) / 2 >= n`, and takes `r = k` when the two are
equal and `r = k + 1` otherwise. It places the units in rows of 1, 2, 3 and more: the first row starts
at `(45 * r, height / 2)`, each unit is 60 below the one before, and each next row starts 45 left and 30
up from the start of the row before. All divisions are signed. Every code then goes to the foe
placement at `0x00029449` (FND-BATTLE-006).

## Interpretation

When `n` is not a triangular number the wedge starts one column further right than it needs to.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0002904B..0x00029208`.
