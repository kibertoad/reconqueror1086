---
id: FND-STRATEGY-027
title: Focusing puts a record near the upper left of the view, and edge panning moves the camera one cell on one axis
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000130E4..0x00013165
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003C6E0..0x0003C769
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003D83C..0x0003D8B5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E500..0x0006E518
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E524..0x0006E544
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E558..0x0006E570
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E57C..0x0006E59C
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000130E4(row, col)` calls `0x0003C6E0(row, col)`, which stores `row - 2`, plus 200 when that is
negative, in the camera row at `0x000AB104` and `col - 11`, or 0 when that is negative, in the
column at `0x000AB108`. The caller then stores 192 in the row when it is above 192 and 10 when it is
below 10, and 2 in the column when it is below 2 and 300 when it is above 300, and calls `0x0003C770`,
`0x00012F28`, `0x0003A63C` and `0x00063270(1, 1)`.

`0x0003D83C` stores 1 in `0x0009AEF8` and reads the pointer globals `x` at `0x000B07E8` and `y` at
`0x000B07EC`. When `x > 628` and the row is below 192 it jumps to `0x0006E524`, which adds 1 to the
row modulo 200 and returns to `0x0003D83C`'s caller. Otherwise, when `x < 8` and the row is above 10
it jumps to `0x0006E558`, which subtracts 1 and stores 199 when the result is negative. Otherwise,
when `y > 474` and the column is below 300 it jumps to `0x0006E57C`, which adds 1 modulo 400, and
otherwise, when `y < 8` and the column is above 2, to `0x0006E500`, which subtracts 1 and stores 399
when the result is negative. Only when none of these applies does it store 0 in `0x0009AEF8`.

## Interpretation

Focusing puts the record two rows and eleven columns from the view's upper left corner. Panning
moves one cell per call along the first edge that applies, so a corner moves the camera along one
axis only, and a call that moves the camera leaves `0x0009AEF8` set, which holds back the hostile
pass (FND-STRATEGY-001) until a later call finds the pointer off the edges.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000130E4..0x00013165`, `0x0003C6E0..0x0003C769`, `0x0003D83C..0x0003D8B5`, `0x0006E500..0x0006E518`, `0x0006E524..0x0006E544`, `0x0006E558..0x0006E570`, `0x0006E57C..0x0006E59C` in `CD:CONQUER.EXE`.
