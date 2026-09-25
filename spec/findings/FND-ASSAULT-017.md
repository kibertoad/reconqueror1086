---
id: FND-ASSAULT-017
title: The mode-1 and mode-2 tests scan the 3 by 3 cells around the actor for the first same-side or opposite-side actor
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F648..0x0004F79D
tool: Ghidra 12.1.3
environment: null
---

## Observation

Test `0x0004F648` (mode 1) takes the actor's live position, shifts each coordinate right by 8 to
get its cell, and visits the nine cells from relative `y = -1` to `1` in the outer loop and
`x = -1` to `1` in the inner loop. For each cell it resolves the map block to a combatant
through `0x0004CEA0`, skips the actor itself and combatants whose health is not above 0, and
returns true at the first combatant on the same side. Test `0x0004F6F7` (mode 2) does the same
over the same cells and returns true at the first combatant on the other side.

## Interpretation

Mode 1 asks whether a friend stands next to the actor, and mode 2 whether an enemy does. The
live position includes the sub-cell offset, so an actor that is part-way into the next cell
scans around that cell.

## Alternatives

None known.

## How to reproduce

Open `0x0004F648`; the two nested loops from -1 to 1 surround the call to `0x0004CEA0`. The
opposite-side test follows at `0x0004F6F7`.
