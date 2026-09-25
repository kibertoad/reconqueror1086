---
id: FND-PERSON-001
title: Character attributes are read by 0x00015EF0 and written by 0x00015F0C, which limits the first 15 to 0..20
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015EAC..0x00015EBA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015EF0..0x00015F09
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015F0C..0x00015F86
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00015EF0(row, field)` follows the pointer at `0x0009A650` to a list of row pointers, takes entry
`row`, follows the pointer at offset `0x50` of that record, and returns the dword at `field * 4`.
`0x00015F0C(row, field, value)` reaches the same dword. For a field below 15 it stores 0 when the
value is negative, 20 when it is above 20, and the value otherwise. For field 15 or above it stores
0 when the value is negative and the value otherwise. `0x00015EAC(row)` returns the dword at offset
0 of the row record, the character's name.

## Interpretation

The first 15 attributes, strength to right-leg health, are held to 0 to 20 whenever the game writes
them through `0x00015F0C`; the others, experience, wealth, age, colour and the tallies, are held
only at 0.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00015EAC` to `0x00015F86`.
