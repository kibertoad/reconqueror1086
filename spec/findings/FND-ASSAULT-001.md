---
id: FND-ASSAULT-001
title: The combat loader clones placed actor blocks into combatant records in x-major order and promotes the first friendly to player
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00051560
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000517B7..0x0005192E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005185F..0x00051875
tool: Ghidra 12.1.3
environment: null
---

## Observation

Loader `0x00051560` keeps combatant records 0 to 9 as the ten templates and appends placed
actors after them. At `0x000517B7` it walks the 128 by 128 map with x in the outer loop and y
in the inner loop. A map cell becomes an actor when its block's behaviour byte `+0x04` has bit
`0x80` set and the low word at block `+0x48` equals 1. The new 68-byte combatant record takes
its template index from block word `+0x4A` and its combat row from block word `+0x4C`.

At `0x0005185F`..`0x00051875` the loader writes the record's position as
`(cell_x << 8) + 0x80` and `(cell_y << 8) + 0x80`, the centre of the cell in 8.8 fixed point.
At `0x000518FE`..`0x00051919` it derives the record's side from the block's normalised colour
group. At `0x00051919`..`0x0005192E` the first record whose side is 0 is stored in the global
at `0x0009D4D0` (object 2 offset `0xD4D0`), the player's combatant index.

## Interpretation

Placed actors become combatants in x-major map order, which is the order every later actor scan
uses. Side 0 is the player's side. The first side-0 actor is the player; every later side-0
actor is a retainer.

## Alternatives

The side could have come from the template index. It comes from the colour group, and
FND-ASSAULT-002 shows that across the supported scenes templates 0 to 2 are always side 0 and
the others never are, so the two readings agree on every placement the game ships.

## How to reproduce

Load the LE image with object 1 at `0x00010000` and object 2 at `0x00090000`. Open
`0x00051560` and follow the nested loop at `0x000517B7`; the test of bit `0x80` and of word
`+0x48` sits at the top of the inner loop. The store to `0x0009D4D0` follows the side
computation.
