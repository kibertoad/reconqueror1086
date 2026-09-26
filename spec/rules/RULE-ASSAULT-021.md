---
id: RULE-ASSAULT-021
title: Using a door or picking up an object
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-036, FND-ASSAULT-037, FND-ASSAULT-038, FND-VIEW-002, FND-ASSAULT-001, FND-ASSAULT-047]
conflicting: []
split_with: []
related: [RULE-RNG-001, FMT-ASSAULT-001, FMT-VIEW-001, FMT-VIEW-002]
---

## Summary

Using a block applies its interaction at once and replaces it in its cell by the block its
state target names. Coins add wealth, food heals the player by dice, an armour item sets an
equipment bit, and bolts add ammunition, whether or not the player gains anything.

## When it runs

- When the player clicks a block marked `actionable` closer than `0x280` (RULE-ASSAULT-005).
- When the player presses the action key and the block one cell ahead of the player is marked
  `actionable`, at any distance.
- When the player's move is stopped by a cell, or the player enters a new cell, and the block in
  that cell is marked `opens_on_contact` (FND-ASSAULT-047). This is how an exit or a gate opens.

## Parameters

- `index`: the block's number in `scene_blocks`.
- `x`, `y`: its cell.

## Inputs

`scene_map`, `scene_blocks`, `wealth`, `ammunition`, `equipment` and `max_health`.

## Procedure

```text
let b = scene_blocks[index]
let player = combatants[player_index]
if b.interaction == 5:
    wealth = wealth + b.argument1
else if b.interaction == 7:
    let healed = 0
    for k in 0..b.argument1:
        healed = healed + random(b.argument2) + 1
    player.health = min(player.health + healed, max_health)
else if b.interaction == 9:
    equipment = equipment | (1 << b.argument1)
else if b.interaction == 10:
    ammunition = ammunition + b.argument1
scene_map.cells[x * 128 + y] = UINT16(b.state_target)
```

## Outputs

Changes the player's wealth, health, equipment or ammunition, and the map cell.

## Edge cases

- Food at full health and an item already owned are still used up.
- The shipped pickups give 25 wealth, 2d6 health, equipment bit 6 and 12 bolts
  (FND-ASSAULT-037).
- A door's state target can name a wall or water as well as floor (FND-ASSAULT-038), and the
  replacement takes effect in one step with no animation.

## What the sources say

SRC-GAMEFAQS-66730 describes food healing and treasure in castles and gives no amounts. It
also reports enemies eating food, which no finding shows.

## Differences between builds

None known.

## Open questions

- The order in which the dispatcher and the map replacement run for one click is not recorded.
- The 15 other cases of the dispatcher are not described. Interaction 0, which exits and gates
  have, changes nothing before the replacement (FND-ASSAULT-047).
- The feedback callback the accepted action calls is not recorded.
- Whether food adds to the player's combatant health or to another copy of it is not
  recorded.
- Where `combatants` and `scene_map` are kept is not recorded.
