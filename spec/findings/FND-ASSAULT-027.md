---
id: FND-ASSAULT-027
title: The mode-11 strike handler checks reach against the combat row, with a close case that skips the ray, and doubles the effect interval for rows 23 and 24
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005010B..0x00050357
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000502E3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000502F0..0x00050345
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005032C..0x00050332
tool: Ghidra 12.1.3
environment: null
---

## Observation

Handler `0x0005010B` (mode 11) first computes `abs(target_x - x) + abs(target_y - y)` from the
live positions of the actor and the combatant in field `+0x24`. When that is at most `0x154`
it uses the stored combatant with a depth of `0x154`. Otherwise it casts a centred ray towards
the stored combatant through `0x000470A8` and uses whichever combatant the ray returns, which
replaces field `+0x24`. In both cases it requires the combatant to be on the other side and the
depth to be strictly below the dword at `0x0009CE24 + 28 * row` for the actor's combat row. When
the check passes, it copies state base + 1 at `0x000502E3` and starts that state's effect at
`0x000502F0`..`0x00050345`, selected by the state block's word `+0x2E`. For rows 23 and 24,
`0x0005032C`..`0x00050332` doubles the descriptor's interval (`+0x14`) before the effect is
made. The handler applies no damage.

## Interpretation

A striking actor lands its blow when the attack animation ends (FND-ASSAULT-028). An enemy that
steps between the actor and its target becomes the new target. Rows 23 and 24 are the two
crossbows, which attack at half the rate.

## Alternatives

None known.

## How to reproduce

Open `0x0005010B` from the handler table; the Manhattan distance and the comparison with
`0x154` come first, and the call to `0x000470A8` follows on the far branch.
