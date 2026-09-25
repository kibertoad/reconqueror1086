---
id: FND-TOURNEY-006
title: The melee scene name repeats the site digit because both digits share one buffer
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005CDE3..0x0005CE1A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000642CC..0x00064300
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

At `0x0005CDE3` the melee pushes 0 and `.RES`, converts the tier to decimal with `0x000642CC` into
the local buffer at the bottom of its frame, pushes the result, converts the value modulo 3 into
the same buffer, pushes that result, pushes `MELEE`, and calls the concatenation routine
`0x000641A0`. `0x000642CC` writes into the buffer it is given and returns that buffer's address,
so both pushed pointers point at the same bytes, which by then hold the second conversion.

## Interpretation

The name is `MELEE`, the site digit twice, and `.RES`: the tournament melee only ever loads
`MELEE00.RES`, `MELEE11.RES` or `MELEE22.RES`, and the tier never reaches the name. The code was
likely meant to give `MELEE` plus site digit plus tier digit, which would pick among the fifteen
tournament scenes.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0005CDE3` to `0x0005CE1A` and `0x000642CC`, and follow the stack offsets of the two
`lea` instructions.
