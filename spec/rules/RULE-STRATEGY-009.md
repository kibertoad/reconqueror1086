---
id: RULE-STRATEGY-009
title: Terrain speed, seasons and the speed setting
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-001, FND-STRATEGY-009, FND-STRATEGY-013, FND-STRATEGY-014, FND-STRATEGY-017]
conflicting: []
split_with: []
related: []
---

## Summary

Every movement reads the kind of a cell's tile and that kind's speed in the season's profile,
scaled by the speed setting. The month picks the profile: summer, autumn, winter or spring. Winter
has its own slower speeds; the other three share one table.

## When it runs

`terrain_kind_at` and `terrain_speed` wherever a movement steps. `set_season` when the map is set up, and `change_season` from the map dispatcher.

## Parameters

`row` and `col` a cell; `kind` a terrain kind.

## Inputs

`world_grid`, `terrain_kinds`, `terrain_speeds`, `profile_of_month` and `current_month`.

## Procedure

```text
define terrain_kind_at(row, col):
    return terrain_kinds[world_grid[row][col] & 0xFFFF]

define terrain_speed(kind):
    return terrain_speeds[terrain_profile][kind]

define set_season():
    terrain_profile = profile_of_month[current_month()]
    # loads the season's atlas: ics.csf for profiles 0 and 3, ica.csf for 1, icw.csf for 2

define change_season():
    if profile_of_month[current_month()] != terrain_profile:
        set_season()
        if g_0009ADB8 == 1:
            # plays the season's movie at (100, 100): tran4.smk, tran2.smk, tran3.smk or tran1.smk for profiles 0 to 3
            return

define raise_speed():
    if strategic_speed < 15:
        strategic_speed = strategic_speed + 1

define lower_speed():
    if strategic_speed > 1:
        strategic_speed = strategic_speed - 1
```

## Outputs

`terrain_profile` and the season's atlas and movie.

## Edge cases

A tile number past the 331 entries of `terrain_kinds` reads beyond the table.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The step by which the two buttons change `strategic_speed`; only the limits 1 and 15 are recorded.
- What `g_0009ADB8` holds.
