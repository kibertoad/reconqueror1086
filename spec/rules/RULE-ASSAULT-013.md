---
id: RULE-ASSAULT-013
title: Destination, rally and follow tests, and the test of the strike and hit modes
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-021, FND-ASSAULT-024, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-VIEW-001, RULE-VIEW-003, RULE-ASSAULT-027, FMT-ASSAULT-001]
---

## Summary

Mode 12 ends at the destination cell. Mode 13 succeeds with at least 6 health. Mode 17
succeeds when a ray reaches the player. The test of modes 11, 14 and 15 is not described; after
a hit it gives mode 11 when the stored target has no more health than the actor, and mode 13
otherwise (FND-ASSAULT-024).

## When it runs

As the test of modes 12, 13 and 17 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `at_destination(c)`, `can_rally(c)` and `player_in_sight(c)`.

## Inputs

`acquisition_row`.

## Procedure

```text
define at_destination(c):
    return (c.x >> 8) == c.dest_x and (c.y >> 8) == c.dest_y

define can_rally(c):
    return c.health >= 6

define player_in_sight(c):
    let p = combatants[player_index]
    if not cast_ray(c.x, c.y, heading_to(p.x - c.x, p.y - c.y), 0, acquisition_row):
        return false
    if combatant_of_block(hit_block) != player_index or hit_depth >= 0x7FFF:
        return false
    c.target = player_index
    c.dest_x = p.x >> 8
    c.dest_y = p.y >> 8
    return true
```

## Outputs

Each returns true or false. `player_in_sight` also sets `target`, `dest_x` and `dest_y` when it
succeeds.

## Edge cases

- An actor with exactly 6 health rallies.
- A friendly in mode 13 that fails (under 6 health) goes to mode 10 and walks away from its
  target (RULE-ASSAULT-016).

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `acquisition_row`, the view row the actor rays are tested at, is not recorded.
- `fn_0004F89B` is named only by its address, and how it reaches the comparison the hit
  handlers make is not recorded.
- Which cell `at_destination` compares, the one under the live position or the one the actor
  occupies, is not recorded; the procedure uses the live position.
- Where `combatants` is kept is not recorded.
