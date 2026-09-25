---
id: FND-VIEW-011
title: Distance shading picks one of the scene's colour maps from the depth, less a per-block offset
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004581F..0x00045854
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00045D1B..0x00045D4F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00045F3E..0x00045F70
tool: Ghidra 12.1.3
environment: null
---

## Observation

Three drawing paths, at `0x0004581F`..`0x00045854`, `0x00045D1B`..`0x00045D4F` and
`0x00045F3E`..`0x00045F70`, select a colour map as
`clamp((depth >> (shift - 2)) - offset, 0, count - 1)`, where `depth` is the 8.8 depth, `shift`
and `count` are the dwords at `Scenario` offsets 48 and 44, and `offset` is the signed dword at
block `+0x0C`. `Scenario` offset 40 enables the mapping and offset 52 names the colour the maps
fade towards.

## Interpretation

Surfaces darken with distance, and a block can be drawn brighter or darker than its distance
alone gives by its own offset.

## Alternatives

None known.

## How to reproduce

Open the three paths; each shifts the depth by a value loaded from the scenario before
subtracting the block's `+0x0C`.
