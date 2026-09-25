---
id: FND-ASSAULT-020
title: The mode-8 and mode-7 contact tests accept whichever combatant the ray actually hits, within the combat-row reach or below 0x200
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F7A2..0x0004F89A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F8B0..0x0004F98C
tool: Ghidra 12.1.3
environment: null
---

## Observation

Test `0x0004F7A2` (mode 8) casts a centred ray from the actor towards the combatant in field
`+0x24` and resolves the returned block with `0x0004CEA0`. It returns true when the block
belongs to a living combatant on the other side and the ray depth is strictly below the dword
at `0x0009CE24 + 28 * row`, where `row` is the actor's combat row. The combatant hit need not be
the one in `+0x24`.

Test `0x0004F8B0` (modes 7 and 16) casts towards the combatant in `+0x24` the same way and
returns true when the block belongs to a living combatant on the actor's own side and the depth
is below `0x200`.

## Interpretation

Mode 8 ends pursuit when any enemy is within the actor's weapon reach along its line of sight.
Mode 7 ends regrouping when any friend is close in front of it.

## Alternatives

None known.

## How to reproduce

Open `0x0004F7A2` and find the read of `0x0009CE24` indexed by the combat row times 28.
`0x0004F8B0` follows the test at `0x0004F89B`.
