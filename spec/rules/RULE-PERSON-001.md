---
id: RULE-PERSON-001
title: Character attributes
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-PERSON-001, FND-PERSON-002]
conflicting: []
split_with: []
related: []
---

## Summary

Each of the 15 characters, row 0 the player, has 30 attributes, numbered from 0 in the order the
game lists them: 0 STRENGTH, 1 DEXTERITY, 2 PIETY, 3 STAMINA, 4 INTELLIGENCE, 5 HONOR, 6 FAME, 7
ARMOR, 8 HEALTH, 9 HEAD_HEALTH, 10 TORSO_HEALTH, 11 LEFT_ARM_HEALTH, 12 RIGHT_ARM_HEALTH, 13
LEFT_LEG_HEALTH, 14 RIGHT_LEG_HEALTH, 15 EXPERIENCE_WITH_SWORD, 16 EXPERIENCE_WITH_LANCE, 17
WEALTH, 18 AGE, 19 COLOR, 20 JOUST_WON, 21 JOUST_LOST, 22 MELEE_WON, 23 MELEE_LOST, 24
BATTLE_WON, 25 BATTLE_LOST, 26 MONEY_BORROWED, 27 PAY_BACK_MONTH, 28 MARRIED, 29 ARROWS. Writing
one keeps the first 15 within 0 to 20 and the rest at 0 or above.

## When it runs

Wherever a rule reads or writes an attribute.

## Parameters

`attr(row, field)` and `set_attr(row, field, value)`.

## Inputs

`character_attributes`.

## Procedure

```text
define attr(row, field):
    return character_attributes[row][field]

define set_attr(row, field, value):
    if field < 15:
        character_attributes[row][field] = min(max(value, 0), 20)
    else:
        character_attributes[row][field] = max(value, 0)
```

## Outputs

`attr` returns the stored value.

## Edge cases

Fields 15 and above have no upper limit, so the callers that want one, such as the joust for lance
experience, apply it themselves. Neither function checks its row or field.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which code writes attributes without `set_attr`.
