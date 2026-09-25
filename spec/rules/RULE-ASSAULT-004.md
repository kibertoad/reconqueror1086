---
id: RULE-ASSAULT-004
title: Retainer orders from the command strip
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-008, FND-ASSAULT-010, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-ASSAULT-007, RULE-ASSAULT-027, FMT-ASSAULT-001, FMT-VIEW-001]
---

## Summary

The four buttons of the command strip order the selected retainers, or all living retainers
when none is selected, to Attack (mode 6), Defend (mode 2), Follow the player (mode 16) or
Retreat (mode 10), and clear the selection.

## When it runs

When the player clicks inside the strip, `x` from 4 to 209 and `y` from 175 to 187 in screen
coordinates.

## Parameters

- `x`: the pointer's screen x.

## Inputs

None.

## Procedure

```text
let button = (x - 4) / 56 + 1
let requested = [6, 2, 16, 10]
let any_selected = false
for i in 10..count(combatants):
    let c = combatants[i]
    if c.side == 0 and c.health > 0 and scene_blocks[block_of(c)].selected == 1:
        any_selected = true
for i in 10..count(combatants):
    let c = combatants[i]
    if c.side != 0 or c.health <= 0:
        continue
    if any_selected and scene_blocks[block_of(c)].selected == 0:
        continue
    c.mode = 0
    c.next_mode = requested[button - 1]
    if button == 3:
        c.target = player_index
    if button == 1 or button == 4:
        take_transition(c, true)
for i in 10..count(combatants):
    scene_blocks[block_of(combatants[i])].selected = 0
```

## Outputs

Changes the modes of the ordered combatants and clears every selection mark.

## Edge cases

- The player is on side 0 too, so the orders reach the player's own combatant as well when it
  is selected or when none is selected.
- Retreat asks for mode 10, which walks away from the stored target; it is distinct from
  leaving the battle.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The value the handlers reset `mode` to is not recorded; the procedure writes 0. How the next
  state decision treats that value for Defend and Follow, which do not call the transition
  helper, is not recorded.
- Which result the Attack and Retreat handlers pass to the helper is not recorded; the
  procedure passes true.
- Where `combatants` and `scene_blocks` are kept is not recorded.
