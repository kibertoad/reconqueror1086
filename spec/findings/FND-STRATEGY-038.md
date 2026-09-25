---
id: FND-STRATEGY-038
title: The spy report names +0x1C the halberdiers and shows one converted number three times
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038C99..0x00038D4D
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

For each active hostile record the spy report reads `+0x1C`, `+0x20`, `+0x24` and the origin at
`+0x28`, calls `0x00043764(origin)` and skips the record when that returns 0. Otherwise it converts
`+0x1C` to decimal with `0x000642CC`, then `+0x24`, then `+0x20`, each into the local buffer at the
bottom of its frame, and joins, through `0x000641A0`, the name `0x00015EAC` gives for that result,
the words that the force is on the march, the third conversion, the label Swordsmen, the second
conversion, the label Knights, the first conversion and the label Halberdiers. It shows the text in
a Spy Report box through `0x000256A0`. The three `lea` instructions that pass the buffer all resolve
to the same stack address, and `0x000642CC` returns the buffer it was given.

## Interpretation

The labels place halberdiers at `+0x1C`, swordsmen at `+0x20` and knights at `+0x24`, which agrees
with the battle categories (FND-BATTLE-001, FND-BATTLE-007). Since the three numbers share one
buffer, all three show the last conversion, the swordsmen count.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00038C84` to `0x00038D77` and follow the stack offsets of the three `lea`
instructions; read the strings at object-2 offsets `0x4F38` to `0x4F8C`.
