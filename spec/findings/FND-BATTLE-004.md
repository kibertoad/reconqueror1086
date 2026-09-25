---
id: FND-BATTLE-004
title: The unit constructor fills one 52-byte record per troop, player first
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00028C38..0x00029049
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00028C38` takes the six counts, allocates `52 * total` bytes at `0x000A9CB8`, `4 * total` at
`0x000A9CA4` and `8 * total + 24` at `0x000A9CB0`, and fills one record per unit: the player's first,
second and third categories, then the foe's in the same order. Each record gets `+0x00` of 0, `0x78`
or `0xF0` by category, `+0x24` of 10, 20 or 40, `+0x2C` of 0 for the player and `0x168` for the foe,
`+0x14` of 3 for the player and 7 for the foe, `+0x20` of 100, `+0x0C`, `+0x10` and `+0x30` of -1,
`+0x18` and `+0x1C` of 0, and `+0x28` of 0 for the player and 1 for the foe.

## Interpretation

The category column is also the frame offset of the category in `MEN8.CSF` (FND-BATTLE-019), and the
lane is the frame offset of the side.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00028C38..0x00029049`.
