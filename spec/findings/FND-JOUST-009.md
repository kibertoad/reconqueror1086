---
id: FND-JOUST-009
title: The dragon run succeeds when 26 times (lance experience - 20 + item bonus) beats both error totals
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001BB6B..0x0001BB91
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B5DA..0x0001B624
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001BB1B..0x0001BB54
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B4D4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009A938
tool: Ghidra 12.1.3
environment: null
---

## Observation

Wrapper `0x0001BB6B`..`0x0001BB91` reads field `0x10` of row 0 of the character table through
`0x00015EF0` and limits it to 0 to 20. At `0x0001B5DA`..`0x0001B624` the worker tests
possession slots `0x36`, `0x40` and `0x3B` through `0x0004310C`, adds 4 for each one held and a
further 5 when all three are. At `0x0001BB1B`..`0x0001BB54` it computes
`threshold = 26 * (experience - 20 + bonus)` and succeeds only when the threshold is greater
than both totals. A worker result of 0 selects the winning movie at `0x0001B4D4`. The table at
`0x0009A938` maps item selectors 10, 15 and 20 to slots `0x36`, `0x3B` and `0x40`.

## Interpretation

Field `0x10` is the character's experience with the lance. The three slots hold the dragon
slaying lance, the shield of St George and the dragon slaying armour; with all three and full
experience the threshold is 442.

## Alternatives

An earlier reading gave the run a circular hit radius from strength and a single thrust. The
two-total comparison rules it out.

## How to reproduce

Open `0x0001BB1B` for the threshold and `0x0001B5DA` for the three calls to `0x0004310C`.
