---
id: FND-RNG-001
title: The random number generator is a linear congruential generator with its state at 0x0009E044
status: superseded
builds: [BLD-GOG-EN]
superseded_by: [FND-RNG-003]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B3EB..0x0006B412
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B413..0x0006B422
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024C20..0x00024C35
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029FF4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001A167..0x0001A18D
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0006B3F1` calls `0x0006B3EB`, which returns the address `0x0009E044`. It multiplies the dword there
by `0x41C64E6D`, adds `0x3039`, stores the result back, and returns bits 16 to 30 of it (shift right
by 16, then AND `0x7FFF`). `0x0006B413` stores its argument in the same dword. A scan of object 1 for
direct calls finds 19 calls to `0x0006B3F1` and 2 to `0x0006B413`. The first of the two is in
`0x00024C20`, which calls `0x0006B3B4` with 0, shifts the result right by one and passes it to
`0x0006B413`; `0x00024C20` is called once, at `0x00029FF4`, the first instruction of a routine that
goes on to set up a new session. The second is at `0x0001A173`, where a routine stores the result
of `0x0001AB98` in the state when the top byte of a dword it reads is greater than 1, and then
draws once and takes the remainder by that byte.

## Interpretation

The game has one generator, the Watcom C library's `rand` and `srand`, and seeds it once from a
value that is likely the time of day. The reseed at `0x0001A173` makes whatever that routine
draws repeatable from its seed, and it moves every later draw onto the sequence of that seed.

## Alternatives

`0x0006B3B4` may not be `time`; only its call with 0 and the shift were read.

## How to reproduce

Disassemble `0x0006B3EB`, `0x0006B413` and `0x00024C20`, then scan object 1 for `E8` calls whose
target is `0x0006B3F1` or `0x0006B413`.
