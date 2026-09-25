---
id: RULE-VIEW-004
title: Surface intersection, depth and pixel test inside the raycaster
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
evidence: []
conflicting: []
split_with: []
related: []
---

## Summary

The arithmetic the raycaster uses to cross a block's shape, measure depth, and test a pixel.
What is known: the block's kind picks the shape (no shape, the cell as a box, a plane through
the middle of the cell along x or along y, or one of the two diagonals), the face hit is
reported as `0x100` north, `0x200` east, `0x400` south or `0x800` west, a kind-4 block's depth
is the forward distance of its centre, and palette index 0 is transparent.

## When it runs

Inside RULE-VIEW-003.

## Parameters

None. This entry names the functions `intersect_face(block, cell_x, cell_y, x8, y8, ray_x,
ray_y)`, `contact_x(...)` and `contact_y(...)` with the same arguments, `wall_depth(dx, dy,
heading)`, `view_forward(dx, dy, heading)`, `view_across(dx, dy, heading)` and
`pixel_opaque(block, texture, column, row, top, bottom)`.

## Inputs

None known.

## Procedure

None known.

## Outputs

None known.

## Edge cases

None known.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- How `0x00044A28` computes the contact point for each shape, including rounding, which face a
  diagonal reports, and whether the contact includes the block's offsets.
- How the depth of a wall contact is measured (along the ray or along the view direction).
- How `0x000447C4` rotates a kind-4 block's centre by the negative view heading, which gives
  `view_forward` and `view_across`.
- Which texture row `0x000444E8` reads for a view row between `top` and `bottom`.
