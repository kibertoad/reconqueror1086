---
id: RULE-ASSAULT-002
title: Load the combatants of a scene and remove retainers above the cap
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-001, FND-ASSAULT-002, FND-ASSAULT-006, FND-ASSAULT-009, FND-VIEW-002]
conflicting: []
split_with: []
related: [RULE-ASSAULT-025, RULE-ASSAULT-027, RULE-ASSAULT-030, FMT-ASSAULT-001, FMT-VIEW-001, FMT-VIEW-002]
---

## Summary

The loader sets up the ten combatant templates, walks the map in x-major order, turns each
placed actor block into a combatant copied from its template, makes the first friendly the
player, recolours the actors, and removes retainers until no more than the cap remain.

## When it runs

When an assault scene is loaded, after RULE-ASSAULT-001 has set the cap.

## Parameters

None. The rule also defines `count_retainers()`.

## Inputs

`scene_map`, `scene_blocks` and `retainer_cap`.

## Procedure

```text
define count_retainers():
    let n = 0
    for i in 10..count(combatants):
        if i != player_index and combatants[i].side == 0 and combatants[i].health > 0:
            n = n + 1
    return n

# The ten templates: actor kind, mode, next_mode and fallback_mode
let kinds = [0, 0, 1, 2, 3, 4, 5, 6, 2, 7]
let modes = [4, 4, 4, 6, 4, 4, 1, 4, 6, 4]
let next_modes = [4, 4, 4, 8, 10, 8, 4, 10, 8, 8]
let fallback_modes = [4, 4, 4, 6, 6, 1, 2, 1, 4, 4]
for t in 0..10:
    set_kind(combatants[t], kinds[t])
    combatants[t].mode = modes[t]
    combatants[t].next_mode = next_modes[t]
    combatants[t].fallback_mode = fallback_modes[t]
let player_found = false
for x in 0..128:
    for y in 0..128:
        let block = scene_blocks[scene_map.cells[x * 128 + y]]
        if block.behaviour & 0x80 == 0 or block.interaction != 1:
            continue
        let c = copy(combatants[block.argument1])
        c.x = (x << 8) + 0x80
        c.y = (y << 8) + 0x80
        set_row(c, block.argument2)
        c.side = side_of_block(block)
        append(combatants, c)
        if c.side == 0 and not player_found:
            player_index = count(combatants) - 1
            player_found = true
call RULE-ASSAULT-025()
while count_retainers() > retainer_cap:
    prune_one_retainer()
```

## Outputs

Fills `combatants` and sets `player_index`. The number of retainers left is at most
`retainer_cap`; the loader never adds a combatant the scene does not place.

## Edge cases

- The player is never counted as a retainer, so a scene with the player and three friendlies
  keeps all three under a cap of 3.
- Across the shipped scenes templates 0 to 2 are always placed on side 0 and templates 3, 5, 8
  and 9 never are, although the side comes from the block's colour rather than from the
  template (FND-ASSAULT-002).

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- How the loader derives `side` from the block's colour group, and how it gives each combatant
  its own live block, is not recorded (`side_of_block`, RULE-ASSAULT-027).
- The templates' health, skill and armour are set by the same initializer and are not
  recorded.
- Which retainer `prune_one_retainer` removes is not recorded (RULE-ASSAULT-030).
- Where `combatants`, `scene_blocks` and `scene_map` are kept is not recorded.
