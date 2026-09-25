---
id: FND-ASSAULT-026
title: A combatant's death path copies its death state, and after that effect it is removed and the hostiles counted, with no reward
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004EA40..0x0004ECD8
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004EC59
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004D8C0
tool: Ghidra 12.1.3
environment: null
---

## Observation

The death path at `0x0004EA40`..`0x0004ECD8` subtracts the damage from the combatant's health at
`+0x40`, restores the map cell from its block's state target `+0x40`, copies state base + 3,
and at `0x0004EC59` starts that state's effect. When the effect finishes, state routine
`0x0004F49C` removes the combatant and calls `0x0004D8C0`, which counts combatants whose side
(field `+0x30`) is not 0 and whose health is above 0. No code on this path reads or writes
wealth at `0x0009D4A4`, ammunition at `0x0009D4A8` or the equipment mask at `0x0009D4C4`.

## Interpretation

Killed enemies leave nothing behind. The count of living hostiles decides whether the assault is
won.

## Alternatives

Loot from defeated enemies was once assumed. A full list of the references to the three reward
globals finds them only on the pickup and crossbow paths (FND-ASSAULT-036).

## How to reproduce

Open `0x0004EA40`; list the references to `0x0009D4A4`, `0x0009D4A8` and `0x0009D4C4` and
check that none lies inside it or in `0x0004D8C0`.
