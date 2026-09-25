---
id: RULE-VIEW-007
title: Colour map of a drawn surface
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-VIEW-011, FND-VIEW-013, FND-VIEW-016]
conflicting: []
split_with: []
related: [FMT-VIEW-001, FMT-VIEW-004, FMT-VIEW-007]
---

## Summary

A surface is drawn through the colour map picked by its depth, less the block's colour offset.

## When it runs

Each time the renderer draws a column of a surface.

## Parameters

None. The rule defines `color_map_for(block, depth)`.

## Inputs

`scene_scenario`.

## Procedure

```text
define color_map_for(block, depth):
    if scene_scenario.color_maps_on == 0:
        return -1
    let n = scene_scenario.color_map_count
    let step = (depth >> (scene_scenario.distance_shift - 2)) - block.color_offset
    return max(0, min(step, n - 1))
```

## Outputs

`color_map_for` gives the step within the block's family of colour maps (FMT-VIEW-007), or -1 for none.

## Edge cases

None known.

## What the sources say

None of the sources describe the shading.

## Differences between builds

None known.

## Open questions

- How the step combines with the block's `color_family` to name one of the 128 maps is not recorded.
- Where the game keeps `scene_scenario` is not recorded.
