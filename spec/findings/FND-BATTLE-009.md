---
id: FND-BATTLE-009
title: The battle clock counts at 250 Hz and its reader returns four times the count
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018420
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000183C0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000183E8..0x0001840D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00069AAC..0x00069B59
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006A025..0x0006A0EA
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00018420` returns the dword at `0x0009A688` shifted left by 2. `0x000183C0` adds 1 to it.
`0x000183E8` registers `0x000183C0` with the timer service `0x00069AAC` at 250 Hz, which programs the
timer with divisor `0x1234DC / 250`, and the timer interrupt dispatcher `0x0006A025` calls it.

## Interpretation

Each count is 4 ms, so the reader advances in steps of 4 and a gate of 200 passes at 204.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00018420`, `0x000183C0`, `0x000183E8..0x0001840D`, `0x00069AAC..0x00069B59`, `0x0006A025..0x0006A0EA`.
