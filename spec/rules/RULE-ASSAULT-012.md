---
id: RULE-ASSAULT-012
title: Contact tests of pursuit and regrouping
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-020, FND-ASSAULT-032, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-VIEW-001, RULE-VIEW-003, RULE-ASSAULT-027, FMT-ASSAULT-001, FMT-ASSAULT-002]
---

## Summary

An actor pursuing or regrouping casts a ray towards its target and accepts whichever combatant
the ray hits: in pursuit, any living enemy within its weapon's reach; in regrouping and
following, any living friend closer than `0x200`.

## When it runs

As the test of mode 8, and of modes 7 and 16 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `enemy_in_reach(c)`, `friend_in_front(c)` and `ray_hit_toward(c)`.

## Inputs

`combat_rows` and `acquisition_row`.

## Procedure

```text
define ray_hit_toward(c):
    let t = combatants[c.target]
    if not cast_ray(c.x, c.y, heading_to(t.x - c.x, t.y - c.y), 0, acquisition_row):
        return -1
    return combatant_of_block(hit_block)

define enemy_in_reach(c):
    let index = ray_hit_toward(c)
    if index < 0:
        return false
    let other = combatants[index]
    return other.health > 0 and other.side != c.side and hit_depth < combat_rows[row_of(c)].reach

define friend_in_front(c):
    let index = ray_hit_toward(c)
    if index < 0:
        return false
    let other = combatants[index]
    return other.health > 0 and other.side == c.side and hit_depth < 0x200
```

## Outputs

Returns true or false and changes nothing.

## Edge cases

The combatant hit need not be the stored target: an enemy that steps into the line ends the
pursuit as well.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `acquisition_row`, the view row the actor rays are tested at, is not recorded.
- Where `combat_rows` is kept is recorded; its per-row values are the game's content and stay in
  the executable.
- Where `combatants` is kept is not recorded.
