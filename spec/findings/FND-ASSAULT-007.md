---
id: FND-ASSAULT-007
title: After an assault, each retainer lost removes one strategic soldier, taken from swordsmen, then halberdiers, then knights
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000586A8
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A101..0x0003A17D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029E80
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029D24
tool: Ghidra 12.1.3
environment: null
---

## Observation

Combat exit calls `0x0004D870` at `0x000586A8` and stores the count of living retainers at
`0x000AF9E4` (object 2 offset `0x1F9E4`). The campaign return code at
`0x0003A101`..`0x0003A123` limits that count to the cap passed on entry and subtracts it from
the cap to get the number of retainers lost. At `0x0003A129`..`0x0003A17D` it takes the losses
from the three per-type contributions computed on entry (FND-ASSAULT-005), in type order 0, 1,
2, subtracts the same amounts from the strategic unit counts, and stores them through
`0x00029E80`. An army whose three counts reach 0 is removed through `0x00029D24`.

## Interpretation

Every retainer who dies in the assault costs the army one soldier, first from the swordsmen's
share, then the halberdiers', then the knights'.

## Alternatives

None known.

## How to reproduce

Follow the store at `0x000586A8` to its reader after the assault returns, at `0x0003A101`.
