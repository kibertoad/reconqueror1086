---
id: FND-ASSAULT-036
title: The object action dispatcher switches on block word +0x48, and cases 5, 7, 9 and 10 give wealth, healing, armour and bolts
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00051E60
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000556B9..0x000556D0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00051F1E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00051E10..0x00051E60
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005254C..0x00052567
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000526EB..0x00052734
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00052941..0x000529CF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00052C36..0x00052C51
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00054EED..0x00054F64
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CE4C
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x00051E60` receives the selected block, its two argument words and a value from the
player's combatant record. At `0x00051F1E` it reads the low word at block `+0x48` and jumps
through the 20-entry relocated table at `0x00051E10` (object 1 offset `0x41E10`); the value, read
from offset 4 of the player's combatant record at `0x000556B9`..`0x000556D0`, is tested inside
the cases and is not the jump selector. Case 5 adds its
first argument to the wealth global at `0x0009D4A4` (`0x0005254C`..`0x00052567`). Case 7 rolls
as many dice as its first argument, each with as many sides as its second, through
`0x0004CE4C` as `random(sides) + 1`, adds the sum to the player's health and limits the result
to the global at `0x0009D4AC` (`0x000526EB`..`0x00052734`). Case 9 sets the bit named by its
first argument in the owned-equipment mask at `0x0009D4C4` (`0x00052941`..`0x000529CF`). Case
10 adds its first argument to the ammunition global at `0x0009D4A8`
(`0x00052C36`..`0x00052C51`). The crossbow code at `0x00054EED`..`0x00054F64` reads and
decrements `0x0009D4A8`. None of the cases checks whether the player is already at full health
or already owns the item.

## Interpretation

Picking up an object applies its reward at once, even when it gives nothing (full health, an
item already owned), and the object is used up.

## Alternatives

None known.

## How to reproduce

Open `0x00051E60`; the jump through the relocated table follows the read of `+0x48` at
`0x00051F1E`. Read each case from the table's fixups.
