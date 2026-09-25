---
id: FND-UI-016
title: The four fief tables share row, terrain, full-screen, OK and Cancel routines, and the village table sets the tax in steps of 5
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000234C8..0x0002371B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002371C..0x000238E4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000239B4..0x00023EBF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002323C..0x000232FF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00023300..0x00023360
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00023364..0x000234A5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024334..0x000243CB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00033D3C..0x00033E5D
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The setup routines bind, for screens 19 to 22 (`FCASTLE`, `FVILLAGE`, `FFARM`, `FFOREST`): the rows
(18, 14, 9 and 8 of them) to one slot-2 and one slot-3 routine per screen; the region at (30, 435) to
an OK routine; the region at (70, 435) to a Cancel routine; the region at (383, 20, 232, 380) to a
slot-2 and a slot-3 terrain routine; and the region at (532, 0, 86, 16) to a full-screen routine.
For the castle these are `0x0001DD44`, `0x0001E054`, `0x0001D884`, `0x0001DB54`, `0x0001E414`,
`0x0001EF0C` and `0x0001DBB8`; for the village `0x00033E98`, `0x000340CC`, `0x00033A98`,
`0x00033B94`, `0x000342E4`, `0x00034E50` and `0x00033BF8`; for the farm `0x00020548`, `0x000207A4`,
`0x00020294`, `0x0002036C`, `0x00020A7C`, `0x000213AC` and `0x000203D0`; for the forest `0x000234C8`,
`0x0002371C`, `0x0002323C`, `0x00023300`, `0x000239B4`, `0x00024334` and `0x00023364`.

In the forest, OK (`0x0002323C`) passes nine 32-byte records from `0x000A9B30` to `0x0002FBD4`,
stores `0x000A99F4` as field 17 of row 0 and passes `0x000A99F8`, `0x000A99FC` and `0x000A9A00` to
`0x0002D6C0`, `0x0002D73C` and `0x0002D6A0`, then calls `0x0006E980`, `0x0002F450(0, 1)` and
`0x00059760(1, 1)`. Cancel (`0x00023300`) calls `0x0002F438(0)`, `0x0006E980` and `0x00059760(1,
1)`. The full-screen routine (`0x00023364`) flips the dword at `0x000A99EC`: going full screen it
shows picture `0x141`, gives the terrain region the rectangle (20, 22, 600, 442) and disables regions
0 to 9; going back it redraws the table with the words at `0x2DEC` and `0x2DF0`, restores (383, 20,
232, 380) and enables regions 0 to 11. The slot-3 terrain routine (`0x00024334`) steps the dword at
`0x000A9A14` through 16 to 26, wrapping, when it lies in that range and `0x000A9C58` is not 0. The row
and slot-2 terrain routines show refusals through `0x00025004` and were not traced further.

The village table's region 16 at (250, 435) has two routines: slot 2 (`0x00033D3C`) adds 5 to the
dword at `0x0009AD10` while it is below 100, slot 3 (`0x00033DC8`) subtracts 5 but not below 0; both
draw it with the format at `0x4608` at (265, 435) and call `0x00034F20`.

## Interpretation

Each table is a staged editor: OK commits the staged values and Cancel discards them. The village's
region 16 is the tax rate, 0 to 100 in steps of 5.

## Alternatives

What the row routines change, and what `0x00034F20` recomputes from the tax, were not traced.

## How to reproduce

Disassemble the listed routines and the other screens' routines named, and read the strings at the
object-2 offsets pushed.
