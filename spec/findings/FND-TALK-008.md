---
id: FND-TALK-008
title: After a conversation variable 6 becomes the debt and five variables set the marriage
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021EEC..0x00021F56
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021F68..0x0002218C
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Before the conversation `0x00021EEC` calls `0x0001A0E8` four times, `0x0001A118` and
`0x0006F795(0x40)`; after it, `0x0006F795(0x7F)`. It then reads variable 6 and calls
`0x00015F0C(0, 26, value)`, and stores 0 in `0x0009A934`. For variables 22, 49, 88, 126 and 167 in
that order, with codes 2, 3, 6, 4 and 5, it reads the variable and, when it is not 0, reads field 28
of row 0, calls `0x0002C250(0, code)` when that is 0, and calls `0x00015F0C(0, 28, code)`.

## Interpretation

The conversation scripts set variable 6 to the money borrowed and one of five variables when a lady
accepts marriage; the hook copies them into MONEY_BORROWED (field 26) and MARRIED (field 28).

## Alternatives

What `0x0002C250` does on a first marriage was not traced.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
