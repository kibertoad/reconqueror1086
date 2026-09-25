---
id: RULE-ASSAULT-009
title: Friend or enemy in the 3 by 3 cells around an actor
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-017, FND-VIEW-002, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-ASSAULT-027, FMT-ASSAULT-001, FMT-VIEW-002]
---

## Summary

The tests of modes 1 and 2 look at the nine cells around the actor's live position, row by row
from the one at lower y, and succeed at the first living combatant on the same side (mode 1) or
on another side (mode 2).

## When it runs

As the test of modes 1 and 2 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `near_friend(c)`, `near_enemy(c)` and `scan_around(c, same_side)`.

## Inputs

`scene_map`.

## Procedure

```text
define scan_around(c, same_side):
    let cx = c.x >> 8
    let cy = c.y >> 8
    for dy in -1..2:
        for dx in -1..2:
            let index = combatant_of_block(scene_map.cells[(cx + dx) * 128 + cy + dy])
            if index < 0:
                continue
            let other = combatants[index]
            if other == c or other.health <= 0:
                continue
            if same_side and other.side == c.side:
                return true
            if not same_side and other.side != c.side:
                return true
    return false

define near_friend(c):
    return scan_around(c, true)

define near_enemy(c):
    return scan_around(c, false)
```

## Outputs

Returns true or false and changes nothing.

## Edge cases

The live position includes the part-way offset of a moving actor, so an actor more than half
way into the next cell scans around that cell.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the scan reads for a cell outside the map is not recorded; actors are never placed on
  the edge of a shipped map.
- Where `combatants` and `scene_map` are kept is not recorded.
