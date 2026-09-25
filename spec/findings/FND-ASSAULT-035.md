---
id: FND-ASSAULT-035
title: Pointer dispatch gives selected friendlies a destination first, then toggles selection, targets enemies, or acts on objects
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00055524..0x00055A58
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000555AB..0x0005562E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000556AF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005568D..0x000556E9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000559CA..0x00055A02
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00055DB8..0x00055DE5
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x00055DB8`..`0x00055DE5` a pointer event takes the cursor's top-left position and adds
`(9, 9)`. Handler `0x00055524` passes that point to raycaster `0x000470A8` and receives a block
index, depth, contact coordinates and a hit class. When the primary flag is 0, at
`0x000555AB`..`0x0005562E` each selected combatant receives the contact coordinates shifted
right by 8 (whole cells) in fields `+0x28` and `+0x2C` and requested mode 12, selection is
cleared, and the click is consumed if any combatant was selected. Otherwise, for a block with
behaviour bit `0x80` the handler resolves the combatant through `0x0004CEA0`. A living side-0
combatant has selection bit 0 of its block byte `+5` toggled. For a hostile, each selected
friendly receives it in field `+0x24` and requested mode 8, selection is cleared, and the click
is consumed when at least one friendly was ordered; otherwise the hostile goes to the player's
weapon path, which at `0x000559CA`..`0x00055A02` compares the depth with the player's combat-row
column 4 plus `0x40`. A block with behaviour bit `0x10` is acted on at `0x0005568D`..`0x000556E9`
only when the depth is strictly below `0x280` (compared at `0x000556AF`).

## Interpretation

Clicking the view with retainers selected sends them to the clicked spot. Without a selection,
clicking a retainer selects it, clicking an enemy sets the selected retainers on it or attacks it
yourself, and clicking a door or item uses it if it is within `0x280` (two and a half cells).
The destination can be a wall cell; movement decides whether it can be entered.

## Alternatives

The destination was once thought to be a point on an empty floor plane. The raycaster returns
the contact on a projected surface (RULE-VIEW-003).

## How to reproduce

Open `0x00055524`. The test of the primary flag and the loop over selected combatants come first,
then the branches on behaviour bits `0x80` and `0x10`.
