---
id: FND-ASSAULT-008
title: The command strip dispatches Attack, Defend, Follow and Retreat, which request modes 6, 2, 16 and 10
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00055D20..0x00055DAE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005700E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00057168
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000572C5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005744B
tool: Ghidra 12.1.3
environment: null
---

## Observation

The input path at `0x00055D20`..`0x00055DAE` accepts a click with `x >= 4 and x < 210` and
`y >= 175 and y < 188` in screen coordinates, and dispatches button `(x - 4) / 56 + 1`. Buttons
1 to 4 call handlers `0x0005700E`, `0x00057168`, `0x000572C5` and `0x0005744B`. Each handler
visits the living side-0 combatants whose block has selection bit 0 of byte `+5` set, or every
living side-0 combatant when none is selected. For each it resets field `+0x18` and writes the
requested mode to field `+0x1C` (6, 2, 16 and 10 respectively). Handlers `0x0005700E` and
`0x0005744B` then call transition helper `0x0004E5F0`. Handler `0x000572C5` also writes the
player's index from `0x0009D4D0` to field `+0x24`. Each handler then clears every selection
bit. The owned `SKIRMISH.PCX` labels the four buttons Attack, Defend, Follow and Retreat.

## Interpretation

The four retainer orders are requests for actor modes 6, 2, 16 and 10. Follow's target is the
player. An order goes to the selected retainers, or to all of them when none is selected.

## Alternatives

Mode 10 was once read as a direct move away from the enemy. The handler only requests mode 10;
what the actor does next is decided by its transition table (FND-ASSAULT-012).

## How to reproduce

Open `0x00055D20` and follow the division by 56 to the four handlers.
