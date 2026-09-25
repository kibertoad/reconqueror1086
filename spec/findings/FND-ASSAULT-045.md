---
id: FND-ASSAULT-045
title: A table maps each weapon item to a combat row, and the two crossbows use rows 23 and 24
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000588BB..0x00058AC6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CB20..0x0004CCB0
tool: Ghidra 12.1.3
environment: null
---

## Observation

Assault setup at `0x000588BB`..`0x00058AC6` builds a mask of the weapons the player owns. Routine
`0x0004CB20`..`0x0004CCB0` maps weapon item numbers 0 to 22 to combat rows 0 to 22 through a
permutation, and item numbers 43 and 44 directly to rows 23 and 24. The executable's fixed-width
row-name table names rows 23 and 24 as the light and the heavy crossbow.

## Interpretation

The player's weapon decides the combat row, and so the dice, penetration, reach and swing speed
(FMT-ASSAULT-002). Rows 23 and 24 are the crossbows.

## Alternatives

None known.

## How to reproduce

Open `0x0004CB20`; the two items outside the permutation are compared first.
