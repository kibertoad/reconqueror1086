---
id: RULE-ASSAULT-010
title: Acquire the nearest visible enemy
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-018, FND-VIEW-003, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-VIEW-001, RULE-VIEW-003, RULE-ASSAULT-027, FMT-ASSAULT-001]
---

## Summary

An actor casts a ray from its position towards each enemy in turn and keeps the enemy whose ray
reaches it at the smallest depth. An enemy behind anything else, including another enemy, is not
chosen through that ray.

## When it runs

As the test of modes 4, 6 and 10 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `seek(c, same_side, stop_depth)` and `seek_enemy(c)`.

## Inputs

`acquisition_row`.

## Procedure

```text
define seek(c, same_side, stop_depth):
    let best = -1
    let best_depth = 0x7FFF
    for i in 0..count(combatants):
        let other = combatants[i]
        if not is_live(other) or other == c:
            continue
        if same_side and other.side != c.side:
            continue
        if not same_side and other.side == c.side:
            continue
        let heading = heading_to(other.x - c.x, other.y - c.y)
        if not cast_ray(c.x, c.y, heading, 0, acquisition_row):
            continue
        if combatant_of_block(hit_block) != i:
            continue
        if hit_depth < best_depth:
            best = i
            best_depth = hit_depth
            if best_depth < stop_depth:
                break
    if best < 0:
        return false
    c.target = best
    return true

define seek_enemy(c):
    return seek(c, false, 0x154)
```

## Outputs

Returns true and sets `c.target` when an enemy was chosen; otherwise returns false and leaves
`target` as it was.

## Edge cases

- A tie in depth keeps the earlier combatant in record order, which is x-major map order.
- The scan stops at the first kept enemy closer than `0x154`, even if a later one is closer
  still.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `acquisition_row`, the view row the actor rays are tested at, is not recorded.
- Where `combatants` and `hit_depth` are kept is not recorded.
