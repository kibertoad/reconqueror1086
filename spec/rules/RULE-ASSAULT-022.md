---
id: RULE-ASSAULT-022
title: Hits, deaths and removal of the dead
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-024, FND-ASSAULT-026, FND-ASSAULT-034, FND-VIEW-002, FND-ASSAULT-001, FND-ASSAULT-042]
conflicting: []
split_with: []
related: [RULE-ASSAULT-018, RULE-ASSAULT-027, RULE-ASSAULT-029, FMT-ASSAULT-001, FMT-ASSAULT-003, FMT-VIEW-001, FMT-VIEW-002]
---

## Summary

Damage lowers a combatant's health. A combatant brought to 0 or below gives back its cell and
shows its death look; when that effect ends it is removed and the living hostiles are counted.
Killed enemies leave nothing behind. A hit combatant shows its hit look.

## When it runs

`apply_hit` from the damage rule (RULE-ASSAULT-023); `hit_look` and `dying_look` as the handlers
of modes 14 and 15 (RULE-ASSAULT-008); `remove_dead` from the state decision
(RULE-ASSAULT-007).

## Parameters

None. The rule defines `apply_hit(d, damage)`, `hit_look(c)`, `dying_look(c)`,
`remove_dead(c)` and `count_hostiles()`.

## Inputs

`scene_map`, `scene_blocks` and `effect_defs`.

## Procedure

```text
define start_look_effect(c):
    let d = effect_defs[scene_blocks[block_of(c)].effect]
    start_effect(c, d, d.flags, d.step_x, d.step_y, d.interval)

define apply_hit(d, damage):
    d.health = d.health - damage
    if d.health > 0:
        return
    scene_map.cells[cell_x_of(d) * 128 + cell_y_of(d)] = UINT16(scene_blocks[block_of(d)].state_target)
    set_look(d, 3)
    start_look_effect(d)

define hit_look(c):
    set_look(c, 2)
    start_look_effect(c)

define dying_look(c):
    return

define count_hostiles():
    let n = 0
    for i in 10..count(combatants):
        if combatants[i].side != 0 and combatants[i].health > 0:
            n = n + 1
    return n

define remove_dead(c):
    set_live(c, false)
    hostiles_left = count_hostiles()
```

## Outputs

Changes health, the map cell of a dying combatant, looks and effects; `remove_dead` takes the
combatant out of play and sets `hostiles_left`.

## Edge cases

- No path through a death changes `wealth`, `ammunition` or `equipment`.
- The hit and dying handlers compare the stored target's health with the actor's reduced
  health (FND-ASSAULT-024); the result decides between mode 11 and mode 13 at the next
  decision.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- How a combatant that survives a hit enters mode 14 is not recorded.
- How the comparison in the hit handlers reaches the next decision, and what else the mode-15
  handler does, is not recorded.
- Whether the hostile count skips the ten templates is not recorded; the procedure skips them.
- Where `hostiles_left` is kept is not recorded, and neither is how the assault uses it to end.
- Where `combatants`, `effect_defs`, `scene_blocks` and `scene_map` are kept is not recorded.
