---
id: FND-ASSAULT-005
title: Campaign castle assaults pass a retainer cap of one per three soldiers of each type, at most three per type, and at least one
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000399F6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00039EAA..0x00039F79
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005877C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000587E9..0x000587F4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029E58
tool: Ghidra 12.1.3
environment: null
---

## Observation

The campaign assault caller reads the strategic unit counts of types 0, 1 and 2 (swordsmen,
halberdiers, knights) through `0x00029E58`. At `0x00039EAA`..`0x00039F30` it computes
`min(count / 3, 3)` for each type with unsigned division, sums the three results, and at
`0x00039F40`..`0x00039F79` replaces a sum of 0 with 1. It passes the result to setup routine
`0x0005877C`, which stores it at `0x0009D49C` (object 2 offset `0xD49C`) at
`0x000587E9`..`0x000587F4`.

A separate caller at `0x000399F6` passes a count divided by 50 to the same setup. It is reached
from a different path than the campaign castle assault.

## Interpretation

The global at `0x0009D49C` is the number of retainers the player may bring into a castle
assault.

## Alternatives

The `/ 50` path could have been the campaign formula. The complete list of callers of
`0x0005877C` places it on another path; which game situation reaches it is not recorded.

## How to reproduce

List the callers of `0x0005877C`. The campaign caller loads the three unit counts through
`0x00029E58` shortly before the call.
