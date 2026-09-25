---
id: FND-PERSON-006
title: The pre-generated handler randomises row 0 for the first shield and stores constants for the other five
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000426A0..0x000426B7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000426B8..0x00042A30
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The handler reads the clicked region through `0x00059D50` and jumps through the six-entry table
at `0x000426A0`. Region 0 sets row 0's name, then for fields 0 to 16 draws `0x00024C38(1)` and
`0x00024C38(8)`, negates the second when the first is not 0, and writes the field plus it through
`0x00015F0C`; inside the same loop it writes `0x00024C38(10) * 100` to field 17 (WEALTH), so the
last of the 17 draws is kept. Regions 1 to 5 each set row 0's name and write constants to fields
0, 1, 2, 3, 5, 6, 15 and 16 and to field 17 through `0x00015F0C`. Every region then calls
`0x0005B2C0`, stores -1 in the dword at `0x0009AAC4` and calls `0x000596C0(6, 1, 1)`.

## Interpretation

Sir Chaunce Norman is region 0: his attributes start from row 0 as loaded and move by -8 to 8,
and his wealth before dubbing is 0 to 1,000 in hundreds. The other five profiles are fixed values
in the executable, a designer's content.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000426B8` to `0x00042A30` and read the six dwords at `0x000426A0`.
