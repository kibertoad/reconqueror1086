---
id: RULE-ASSAULT-020
title: Strike handler
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-027, FND-ASSAULT-032, FND-ASSAULT-034, FND-ASSAULT-001, FND-ASSAULT-042]
conflicting: []
split_with: []
related: [RULE-VIEW-001, RULE-VIEW-003, RULE-ASSAULT-018, RULE-ASSAULT-027, RULE-ASSAULT-029, FMT-ASSAULT-001, FMT-ASSAULT-002, FMT-ASSAULT-003, FMT-VIEW-001]
---

## Summary

A striking actor starts its attack when an enemy is within its weapon's reach: the stored target
when the two stand close, otherwise whichever combatant a ray towards the target hits. The damage
comes when the attack effect ends (RULE-ASSAULT-018). Crossbows attack at half the rate.

## When it runs

As the handler of mode 11 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `strike(c)`.

## Inputs

`combat_rows`, `effect_defs` and `acquisition_row`.

## Procedure

```text
define strike(c):
    let t = combatants[c.target]
    let index = -1
    let depth = 0
    if abs(t.x - c.x) + abs(t.y - c.y) <= 0x154:
        index = c.target
        depth = 0x154
    else:
        if not cast_ray(c.x, c.y, heading_to(t.x - c.x, t.y - c.y), 0, acquisition_row):
            return
        index = combatant_of_block(hit_block)
        depth = hit_depth
        if index < 0:
            return
        c.target = index
    if combatants[index].side == c.side:
        return
    let row = row_of(c)
    if depth >= combat_rows[row].reach:
        return
    set_look(c, 1)
    let d = effect_defs[scene_blocks[block_of(c)].effect]
    let interval = d.interval
    if row == 23 or row == 24:
        interval = interval * 2
    start_effect(c, d, d.flags, d.step_x, d.step_y, interval)
```

## Outputs

Shows the attack look and starts its effect, and may change `target`. Applies no damage.

## Edge cases

- A reach of `0x154` or less can never pass the close case, whose depth is exactly `0x154`.
- An enemy who steps between the actor and its target becomes the new target.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `acquisition_row`, the view row the actor rays are tested at, is not recorded.
- What the handler does when the ray hits no combatant is not recorded; the procedure returns.
- Where `combatants`, `effect_defs`, `hit_depth` and `scene_blocks` are kept is not recorded.
