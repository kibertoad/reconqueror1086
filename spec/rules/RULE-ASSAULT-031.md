---
id: RULE-ASSAULT-031
title: The player's weapon breaking on a miss
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-043, FND-ASSAULT-032, FND-ASSAULT-045, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-ASSAULT-027, RULE-RNG-001, FMT-ASSAULT-001, FMT-ASSAULT-002]
---

## Summary

When the player's weapon misses, it breaks on a zero draw from `200 + 50 * penetration`. The
weapon in combat row 0 never breaks.

## When it runs

When the player's weapon contact misses. Which outcomes of the weapon path count as a miss is
not recorded.

## Parameters

None. The rule defines `weapon_breaks()`.

## Inputs

`combat_rows`.

## Procedure

```text
define weapon_breaks():
    let row = row_of(combatants[player_index])
    if row == 0:
        return false
    return random(200 + 50 * combat_rows[row].penetration) == 0
```

## Outputs

Returns true when the weapon breaks. What breaking does to the player's equipment is not
recorded.

## Edge cases

- Row 0 makes no draw at all, so a miss with it leaves the random number routine where it was.
- The two crossbow rows have a penetration of 6, a chance of 1 in 500.

## What the sources say

SRC-GAMEFAQS-66730 says weapons can break; it gives no chance.

## Differences between builds

None known.

## Open questions

- Which outcomes count as a miss, and what the game does with a broken weapon.
- Where `combatants` is kept is not recorded.
