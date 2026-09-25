---
id: FND-PERSON-008
title: The game ends the campaign when row 0 AGE is 30
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002AD62..0x0002ADB3
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The routine at `0x0002AD62` returns 0 when the dword at `0x0009ADC0` is not 0. When the table
exists (`0x00015FF8`) and field 18 (AGE) of row 0 is 30, it stores 0 in the dword at `0x0009AABC`,
calls `0x00024CA0` and, when the dword at `0x0009ADB8` is not 0, plays `avg_end.smk` from the
path in the `CD_PATH` setting.

## Interpretation

At 30 the campaign ends with the retirement movie.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0002AD62` to `0x0002ADE5`.
