---
id: FND-TOURNEY-002
title: The tournament tent picks five distinct opponents from rows 1 to 14, never row 8
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C99C..0x0005C9CB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C930..0x0005C998
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005D318..0x0005D334
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005D168..0x0005D304
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

On entry, `0x0005C99C` calls `0x0005C930` when `0x0009DBCC` is 0, then sets `0x0009DBCC` to 1.
`0x0005C930` sets the five dwords at `0x000AFF48` to 0, then for each in turn draws
`0x00024C38(14)` again and again until the result is neither 0 nor 8 nor equal to any of the five,
and stores it. The menu callback `0x0005D318` reads the selected menu row, indexes the dwords at
`0x0009DC48`, which hold 0, 0, 1, 2, 3, 4, and stores the entry of `0x000AFF48` it names at
`0x0009DC74`; for row 0 it shows the Exit label in place of a name. `0x0005D168` shows a line
with the opponent's name and, when `0x000AFF44` is 0, his fields 20 and 21, or when it is 1 his
fields 22 and 23, read through `0x00015EF0`.

## Interpretation

The five opponents are character rows, drawn once per tournament, and row 8, the king, and row 0,
the player, never fight. The line shows the opponent's joust or melee record.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0005C930` to `0x0005C9CB` and `0x0005D168` to `0x0005D334`, and read the six dwords
at object-2 offset `0xDC48`.
