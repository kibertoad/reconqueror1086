---
id: RULE-ASSAULT-025
title: Actor colours follow the player's colour
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-039, FND-ASSAULT-001, FND-PERSON-009]
conflicting: []
split_with: []
related: [RULE-ASSAULT-027, FMT-ASSAULT-001, FMT-VIEW-001]
---

## Summary

Friendly actors take the player's colours. A hostile that would share them takes blue, or green
when the player is blue.

## When it runs

While the combatants are loaded (RULE-ASSAULT-002).

## Parameters

None.

## Inputs

`player_color`.

## Procedure

```text
# red, green and blue
let families = [0, 32, 64]
let walk_textures = [64, 128, 96]
let family = families[player_color]
for i in 10..count(combatants):
    let c = combatants[i]
    let b = scene_blocks[block_of(c)]
    if c.side == 0:
        b.color_family = family
        b.surface0 = walk_textures[player_color]
    else if b.color_family == family:
        if player_color == 2:
            b.color_family = 32
            b.surface0 = 128
        else:
            b.color_family = 64
            b.surface0 = 96
```

## Outputs

Changes the colour family and walk texture of actor blocks.

## Edge cases

Hostiles authored with the fourth family (96) keep it whatever the player's colour.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which colour each value of `player_color` is; the procedure's red, green and blue for 0, 1 and
  2 is a guess.
- Whether the loader writes the actors' own blocks or their base blocks is not recorded.
- Where `combatants` and `scene_blocks` are kept is not recorded.
