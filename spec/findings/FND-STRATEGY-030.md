---
id: FND-STRATEGY-030
title: The hostile markers use a frame from a per-property table and brigands frame 3
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A63C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009AE88
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003B904..0x0003B940
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003A63C` visits the five hostile records and, for each active one, draws frame `+0x38` of the
image `+0x110` at the truncated `+0x5C`, `+0x60` through `0x0003F0A0`. The constructors store the
word at `0x0009AE88 + 2 * origin` shifted left 3 in `+0x38`. The brigand pass draws frame 3 of the
image handle at `0x0009AE80` at each active brigand's truncated position.

## Interpretation

Each property's forces carry that property's colour block.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003A63C`, `0x0009AE88`, `0x0003B904..0x0003B940` in `CD:CONQUER.EXE`.
