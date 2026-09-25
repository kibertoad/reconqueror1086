---
id: RULE-ASSAULT-024
title: The player's skill and health in an assault
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-033, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [FMT-ASSAULT-001]
---

## Summary

The player's combatant takes its skill from strength, dexterity and twice sword experience, and
its health from strength, stamina and honour.

## When it runs

During assault setup, after the combatants are loaded.

## Parameters

None.

## Inputs

`character_strength`, `character_dexterity`, `character_stamina`, `character_honour` and
`character_sword_experience`.

## Procedure

```text
let p = combatants[player_index]
p.skill = character_strength + character_dexterity + 2 * character_sword_experience
p.health = character_strength + character_stamina + character_honour
```

## Outputs

Sets the player's `skill` and `health`.

## Edge cases

None known.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `max_health`, which caps healing, is not shown to be set from the same sum.
- Where `character_honour`, `character_sword_experience` and `combatants` are kept is not recorded.
