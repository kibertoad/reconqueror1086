---
id: FND-TOURNEY-007
title: The practice melee loads MELEE0 to MELEE2 with a draw of 0 to 2
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00042014..0x000420A4
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00042014` draws `0x00024C38(2)`, converts it to decimal and appends it to `MELEE` and `.RES`. It
then draws `0x00024C38(6)` and `0x00024C38(4)`, and calls `0x0005921C` with the name, the first of
the two, the sum of both, and 0.

## Interpretation

The practice melee picks one of the three base scenes at random. The two later numbers likely set
the size of the two sides.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00042014` to `0x000420A4`.
