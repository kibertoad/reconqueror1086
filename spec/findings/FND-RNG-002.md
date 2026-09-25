---
id: FND-RNG-002
title: Three helpers reduce a draw to a range: scaled, remainder and dice
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000445B4..0x000445C1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041110..0x0004111D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024C38..0x00024C4B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CE4C..0x0004CE73
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000445B4` calls `0x0006B3F1`, multiplies the result by its argument with `imul`, and shifts the
low 32 bits right by 15 with `shr`. `0x00041110` is a second copy of the same three instructions.
`0x00024C38` calls `0x0006B3F1`, adds 1 to its argument and returns the remainder of a signed
`idiv` of the draw by it. `0x0004CE4C` takes a count and a number of sides, and adds up, once for
each count while the count is above 0, the result of `0x000445B4` with the sides, plus 1. There are
21 direct calls to `0x000445B4` and 61 to `0x00024C38`. The first-person damage code at
`0x0004F2CA` calls `0x000445B4` with 200.

## Interpretation

`0x000445B4` gives 0 to `n - 1`, `0x00024C38` gives 0 to `n` inclusive, and `0x0004CE4C` rolls dice.
The practice joust uses the copy at `0x00041110`, so it draws from the same generator.

## Alternatives

None known.

## How to reproduce

Disassemble the four addresses and scan object 1 for `E8` calls to each.
