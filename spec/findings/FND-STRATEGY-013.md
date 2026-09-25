---
id: FND-STRATEGY-013
title: Terrain speed comes from a tile-kind table and four profile pointers, scaled by a speed from 1 to 15
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009AF78
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B610
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B704
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B614
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B68C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009AEEC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000110D7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003D781..0x0003D7DA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E41E..0x0003E468
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The movement handlers read a tile's kind from the byte at `0x0009AF78 + tile`, a table of 331
bytes, and the speed of that kind as a float from the table the dword at `0x0009B704 + 4 * p`
points to, with `p` the dword at `0x0009B610`. The four pointers select the 30-float table at
`0x0009B614` for profiles 0, 1 and 3 and the one at `0x0009B68C` for profile 2. The movement code
multiplies by the dword at `0x0009AEEC`, which `0x000110D7` sets to 1 and the handlers at
`0x0003D781`..`0x0003D7DA` and `0x0003E41E`..`0x0003E468` raise and lower within 1 to 15.

## Interpretation

`0x0009AEEC` is the map's speed setting, and the handlers are its two buttons. Profile 2 is winter
(FND-STRATEGY-014), the only profile with its own, slower table.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0009AF78`, `0x0009B610`, `0x0009B704`, `0x0009B614`, `0x0009B68C`, `0x0009AEEC`, `0x000110D7`, `0x0003D781..0x0003D7DA`, `0x0003E41E..0x0003E468` in `CD:CONQUER.EXE`.
