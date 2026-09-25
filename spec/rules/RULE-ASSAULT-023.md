---
id: RULE-ASSAULT-023
title: Damage of one blow
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-046, FND-ASSAULT-031, FND-ASSAULT-032, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-ASSAULT-022, RULE-ASSAULT-027, RULE-RNG-001, FMT-ASSAULT-001, FMT-ASSAULT-002]
---

## Summary

A blow reaches when the two stand within the attacker's reach plus `0x40`. It hits on a draw
from 200 below 100 plus the attacker's skill less half the defender's, with 30 more for a
crossbow at close range and 30 more when both face the same way. Damage is the attacker's dice
less the defender's armour after penetration.

## When it runs

When a strike effect ends (RULE-ASSAULT-018) and when the player attacks (RULE-ASSAULT-005).

## Parameters

- `a`: the attacker.
- `d`: the defender.

## Inputs

`combat_rows`.

## Procedure

```text
let row = combat_rows[row_of(a)]
if abs(d.x - a.x) + abs(d.y - a.y) > row.reach + 0x40:
    return
let bonus = 0
if (row_of(a) == 23 or row_of(a) == 24) and within_one_cell(a, combatants[player_index]):
    bonus = bonus + 30
if heading_of(a) == heading_of(d):
    bonus = bonus + 30
if random(200) >= 100 + a.skill - d.skill / 2 + bonus:
    return
let rolled = 0
for k in 0..row.dice:
    rolled = rolled + random(row.sides) + 1
let armour = max(d.armor - row.penetration, 0)
let damage = max(rolled - armour, 0)
apply_hit(d, damage)
```

## Outputs

Changes the defender through `apply_hit`.

## Edge cases

- A blow from behind, with both facing the same way, is 30 in 200 likelier to hit.
- Penetration above the armour gives no extra damage.
- A hit whose dice do not beat the armour still counts as a hit with 0 damage.

## What the sources say

SRC-GAMEFAQS-66730 reports that a very high sword experience can make the player
impossible to hit, which agrees with the defender's skill lowering the hit chance. It also
reports occasional critical hits shown by a red flash, which no finding shows.

## Differences between builds

None known.

## Open questions

- Whether the reach comparison is strict is not recorded.
- How "within one cell of the player" is measured is not recorded (`within_one_cell`).
- Whether a hit with 0 damage still reaches `apply_hit` is not recorded.
- Where `combatants` is kept is not recorded.
