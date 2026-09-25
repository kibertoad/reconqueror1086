---
id: FND-BATTLE-003
title: The battle choice gate offers five rectangles and maps the first four to formation codes 2, 3, 1 and 0
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000256FC..0x000258FA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000956E8
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000256FC` allocates five rectangles in this order: `(30,30,215,185)`, `(400,30,205,200)`,
`(25,250,185,185)`, `(375,250,185,185)` and `(220,417,148,38)`, and hit-tests them with
`0x0006FF10`. The dispatch table at `0x000956E8` gives the formation codes 2, 3, 1 and 0 for the first
four; the fifth returns 0 from the gate.

## Interpretation

The player picks one of four formations or lets the battle resolve itself. What the four look like to
the player was not recorded.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000256FC..0x000258FA`, `0x000956E8`.
