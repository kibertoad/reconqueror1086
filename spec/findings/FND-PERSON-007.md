---
id: FND-PERSON-007
title: Dubbing sets every AGE to 18, gives five items and adds 240 to WEALTH
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019A8C..0x00019B0E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043100..0x0004310B
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The callback at `0x00019A8C` writes 18 to field 18 (AGE) of every row through `0x00015F0C`, calls
`0x00043100` with 9, 23, 32, 28 and 16 in that order, then writes field 17 (WEALTH) of row 0 plus
240 through `0x00015F0C`. `0x00043100(item)` adds 1 to the dword at `0x0009C6C8 + 8 * item`.

## Interpretation

Both routes to knighthood end here, so every new knight starts with the same five items and 240
shillings on top of the wealth the route left.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00019A8C` to `0x00019B0E` and `0x00043100`.
