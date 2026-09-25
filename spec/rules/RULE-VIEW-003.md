---
id: RULE-VIEW-003
title: Cast a ray through the scene and find the first opaque surface at a view row
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-VIEW-004, FND-VIEW-005, FND-VIEW-006, FND-VIEW-007, FND-VIEW-008, FND-VIEW-009, FND-VIEW-010, FND-VIEW-002]
conflicting: []
split_with: []
related: [RULE-VIEW-002, RULE-VIEW-004, FMT-VIEW-001]
---

## Summary

The game finds what is seen along one column of the first-person view by walking a ray across
the map, collecting up to 31 surfaces it crosses, and taking the first one whose pixel at the
wanted row is not transparent.

## When it runs

When the player clicks the view (RULE-ASSAULT-005), and whenever an actor looks for or at
another actor (RULE-ASSAULT-010, RULE-ASSAULT-011, RULE-ASSAULT-012, RULE-ASSAULT-013,
RULE-ASSAULT-020).

## Parameters

None. The rule defines `cast_ray(x8, y8, heading, lateral, row)`: the origin in 8.8 map units,
the view heading, the sideways component of the ray before rotation (0 for the centre column),
and the view row to test.

## Inputs

`scene_map`, `scene_blocks`, `view_width`, `horizon` and `view_elevation`.

## Procedure

```text
define view_lateral(column):
    return ((0x400000 / view_width) * (column - view_width / 2)) >> 8

define cast_ray(x8, y8, heading, lateral, row):
    let ray_x = rotate_x(0x4000, lateral, heading)
    let ray_y = rotate_y(0x4000, lateral, heading)
    let step_x = 0
    let step_y = 0
    if abs(ray_x) > abs(ray_y):
        step_x = 0x100
        if ray_x < 0:
            step_x = -0x100
        step_y = step_x * ray_y / ray_x
    else:
        step_y = 0x100
        if ray_y < 0:
            step_y = -0x100
        step_x = step_y * ray_x / ray_y
    # candidates, in the order they are found
    let found: INT32[] = []
    let found_cx: INT32[] = []
    let found_cy: INT32[] = []
    let found_face: INT32[] = []
    let px = x8
    let py = y8
    let old_x = (x8 >> 8) & 0x7F
    let old_y = (y8 >> 8) & 0x7F
    let stop = false
    for s in 0..64:
        px = px + step_x
        py = py + step_y
        let new_x = (px >> 8) & 0x7F
        let new_y = (py >> 8) & 0x7F
        let probe_x = [new_x, old_x, new_x]
        let probe_y = [old_y, new_y, new_y]
        if new_x == old_x:
            probe_x = [new_x, (new_x - 1) & 0x7F, (new_x + 1) & 0x7F]
            probe_y = [new_y, new_y, new_y]
        else if new_y == old_y:
            probe_x = [new_x, new_x, new_x]
            probe_y = [new_y, (new_y - 1) & 0x7F, (new_y + 1) & 0x7F]
        for p in 0..3:
            let index = scene_map[probe_x[p] * 128 + probe_y[p]]
            let block = scene_blocks[index]
            while block.kind != BLOCK_EMPTY:
                let face = intersect_face(block, probe_x[p], probe_y[p], x8, y8, ray_x, ray_y)
                if face != 0:
                    append(found, index)
                    append(found_cx, probe_x[p])
                    append(found_cy, probe_y[p])
                    append(found_face, face)
                    if block.behaviour & 1 == 0 or count(found) == 31:
                        stop = true
                        break
                if block.behaviour & 1 == 0:
                    break
                if block.kind != BLOCK_SPRITE and block.behaviour & 8 == 0:
                    break
                index = block.state_target
                block = scene_blocks[index]
            if stop:
                break
        if stop:
            break
        old_x = new_x
        old_y = new_y
    # take the candidates in the order they were found
    for i in 0..count(found):
        let block = scene_blocks[found[i]]
        let cx = contact_x(block, found_cx[i], found_cy[i], x8, y8, ray_x, ray_y)
        let cy = contact_y(block, found_cx[i], found_cy[i], x8, y8, ray_x, ray_y)
        let depth = 0
        let texture = -1
        let column = -1
        if block.kind == BLOCK_SPRITE:
            let rel_x = (found_cx[i] << 8) + 0x80 + block.offset_x - x8
            let rel_y = (found_cy[i] << 8) + 0x80 + block.offset_y - y8
            depth = view_forward(rel_x, rel_y, heading)
            let u = ((view_across(ray_x, ray_y, heading) * depth) >> 14) - view_across(rel_x, rel_y, heading) + 0x80
            if u < 0 or u > 0xFF:
                continue
            let divisions = block.surface2
            let sector = (((block.surface3 - heading + 0x80 / divisions) & 0xFF) * divisions) >> 8
            column = u >> (8 - block.width_shift)
            if block.behaviour & 4 != 0 and sector > divisions / 2:
                sector = divisions - sector
                column = block.texture_width - 1 - column
            texture = block.surface0 + sector
        else:
            depth = wall_depth(cx - x8, cy - y8, heading)
            let u = cx & 0xFF
            if found_face[i] == 0x200 or found_face[i] == 0x800:
                u = cy & 0xFF
            if found_face[i] == 0x100 or found_face[i] == 0x200:
                u = 0xFF - u
            texture = block.surface0
            if found_face[i] == 0x200:
                texture = block.surface1
            else if found_face[i] == 0x400:
                texture = block.surface2
            else if found_face[i] == 0x800:
                texture = block.surface3
            column = u >> (8 - block.width_shift)
        let top = horizon - (block.upper - view_elevation) * view_width / depth
        let bottom = horizon + (view_elevation - block.lower) * view_width / depth
        if row < top or row > bottom:
            continue
        if not pixel_opaque(block, texture, column, row, top, bottom):
            continue
        hit_block = found[i]
        hit_depth = depth
        hit_x = cx
        hit_y = cy
        hit_cell_x = found_cx[i]
        hit_cell_y = found_cy[i]
        return true
    return false
```

## Outputs

`cast_ray` returns true when a candidate passes, and then sets `hit_block` and `hit_depth` to
that candidate's block number and depth, `hit_x` and `hit_y` to its contact point in 8.8 map
units, and `hit_cell_x` and `hit_cell_y` to the cell it was found in. It returns false when
none passes. It changes nothing else.

## Edge cases

- The map wraps: a ray that leaves one edge of the 128 by 128 map comes in at the other, and
  after 64 steps it stops whatever it has found.
- A surface marked see-through (`see_through`) does not stop the walk; any other surface that
  is hit does. The walk also stops once it holds 31 candidates, even though the arrays have 32
  slots.
- A kind-4 candidate whose texture column falls outside 0 to 255 is skipped; that happens when
  the ray passes beside the sprite's own width.
- Candidates are tested in the order they were found, so a transparent pixel of a nearer
  surface lets a farther one be picked. Candidates found in the same step of the walk are in
  probe order, not in order of depth.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `intersect_face`, `contact_x`, `contact_y`, `wall_depth`, `view_forward`, `view_across` and
  `pixel_opaque` are specified only in outline (RULE-VIEW-004). What contact point a kind-4
  candidate reports is not recorded.
- What the original does when `depth` is 0 or negative, where the two projections divide by
  it, is not recorded.
- Where the game keeps `scene_map`, `scene_blocks`, `view_width`, `horizon`, `view_elevation`,
  `hit_block`, `hit_depth`, `hit_x`, `hit_y`, `hit_cell_x` and `hit_cell_y` is not recorded.
- When the two ray components have the same magnitude the procedure takes y as the major
  axis; the original's choice for that tie is not recorded.
- The procedure reads the probe cell's block once and follows state targets in place; whether
  the original keeps the probe cell's offsets or the target block's own while it follows the
  chain is not recorded.
