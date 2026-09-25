---
id: RULE-VIEW-005
title: Draw the backdrop
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-VIEW-014, FND-VIEW-015, FND-VIEW-005]
conflicting: []
split_with: []
related: [FMT-VIEW-005, FMT-VIEW-006]
---

## Summary

The first-person view starts each frame with the scene's panorama, turned with the view and set so that its horizon row meets the view horizon.

## When it runs

Each time the world renderer draws the first-person view, before any surface.

## Parameters

None.

## Inputs

`backdrop`, `back_image`, `view_heading`, `horizon`, `view_width`, `view_height` and `view_pixels`.

## Procedure

```text
let column = view_heading * backdrop.scale
if column > backdrop.width:
    column = column - backdrop.width
let first = min(backdrop.width - column, view_width)
let shift = horizon - backdrop.horizon
let skipped = 0
if shift < 0:
    skipped = -shift
    shift = 0
let rows = min(view_height, backdrop.height - skipped)
for r in 0..rows:
    let source = (skipped + r) * backdrop.width
    let target = (shift + r) * view_width
    for c in 0..first:
        view_pixels[target + c] = back_image[source + column + c]
    for c in first..view_width:
        view_pixels[target + c] = back_image[source + c - first]
```

## Outputs

The view rows the loop reaches hold panorama pixels; the others are unchanged.

## Edge cases

Nothing is drawn when the scene has no `BackImage`. For every shipped backdrop `view_heading * 3` stays below 1,088, so the one subtraction never happens.

## What the sources say

None of the sources describe the backdrop.

## Differences between builds

None known.

## Open questions

- Where the game keeps `backdrop`, `back_image`, `view_height`, `view_pixels` and `view_width` is not recorded.
