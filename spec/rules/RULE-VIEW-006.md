---
id: RULE-VIEW-006
title: Build a scene's distance colour maps
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-VIEW-017, FND-VIEW-013, FND-VIEW-016]
conflicting: []
split_with: []
related: [FMT-VIEW-004, FMT-VIEW-007, FMT-VIEW-009]
---

## Summary

Map `i` of a family of `count` colour maps replaces each palette colour by the palette colour nearest to a blend of it with the fade colour, weighted `i / count` towards the fade colour.

## When it runs

The generator can build a scene's first family; when the game runs it is not recorded (FMT-VIEW-007).

## Parameters

None. The rule defines `build_color_map(i)`, which gives map `i` of the first family.

## Inputs

`combat_palette` and `scene_scenario`.

## Procedure

```text
define blend_channel(a, b, i, n):
    return (2 * (a * (n - i) + b * i) + n) / (2 * n)

define build_color_map(i):
    let n = scene_scenario.color_map_count
    let t = scene_scenario.fade_color
    let result: INT32[] = []
    for c in 0..256:
        let r = blend_channel(combat_palette.rgb[3 * c], combat_palette.rgb[3 * t], i, n)
        let g = blend_channel(combat_palette.rgb[3 * c + 1], combat_palette.rgb[3 * t + 1], i, n)
        let b = blend_channel(combat_palette.rgb[3 * c + 2], combat_palette.rgb[3 * t + 2], i, n)
        let best = 1
        let best_distance = 0x2FD
        for e in 0..256:
            let d = abs(combat_palette.rgb[3 * e] - r) + abs(combat_palette.rgb[3 * e + 1] - g) + abs(combat_palette.rgb[3 * e + 2] - b)
            if d < best_distance:
                best = e
                best_distance = d
        append(result, best)
    return result
```

## Outputs

`build_color_map(i)` gives the 256 entries of map `i`.

## Edge cases

A tie between palette entries keeps the lowest index. When every palette entry is `0x2FD` or more away, the entry is 1.

## What the sources say

None of the sources describe the colour maps.

## Differences between builds

None known.

## Open questions

- Where the game keeps `combat_palette` and `scene_scenario` is not recorded.
- The game blends with floating-point weights, adding 0.5 and truncating; `blend_channel` does the same in integers. Whether the two ever differ is not recorded; they agree on `SKIRMISH.PAL`, where the built maps match the stored ones.
