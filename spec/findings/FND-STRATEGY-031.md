---
id: FND-STRATEGY-031
title: The route preview at 0x00011CC0 dots the selected record's route with marker frames
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00011CC0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00011EE4
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

When `0x0009AE58` is set and `0x0009AEF8` clear, `0x00011EE4` passes the selected record's truncated
position and its route pairs to `0x00011CC0`. That routine subtracts 18 from each segment's end x,
walks each segment along its longer axis with an integer error term, and draws a dot when either
axis is more than 20 from the last dot. Each dot uses frame `counter % 4` of the handle at
`0x0009AE84`, and the starting counter advances every third call.

## Interpretation

The dots are the planned route while the player draws it.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00011CC0`, `0x00011EE4` in `CD:CONQUER.EXE`.
