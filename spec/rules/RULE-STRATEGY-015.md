---
id: RULE-STRATEGY-015
title: Terrain, markers and the route preview
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-003, FND-STRATEGY-017, FND-STRATEGY-018, FND-STRATEGY-019, FND-STRATEGY-024, FND-STRATEGY-026, FND-STRATEGY-027, FND-STRATEGY-028, FND-STRATEGY-029, FND-STRATEGY-030, FND-STRATEGY-031]
conflicting: []
split_with: []
related: []
---

## Summary

The view draws the tiles of the visible cells in scan order, then the player markers from the
colour block the player chose, the hostile markers from their property's block, and the dots of a
route being drawn.

## When it runs

From the strategic pass (RULE-STRATEGY-001) and when the map is focused (RULE-STRATEGY-014).

## Parameters

`image`, `frame`, `x` and `y` a marker image, its frame and a point in route units.

## Inputs

`world_grid`, the camera, the records, and COLOR (attribute 19 of row 0), which the character options
screen sets to 0, 3 or 5 for red, green or blue. At load time each player record's `frame` is eight
times COLOR, and every player and hostile record's `image` is the handle of `icon_men.CSF`.

## Procedure

```text
define draw_terrain():
    for m in 0..24:
        let col = (camera_col + m) % 400
        let x = -20
        if col % 2 == 1:
            x = 20
        let y = -53 + 20 * m
        let row = camera_row
        while x < 403:
            blit_tile(world_grid[row][col] & 0xFFFF, x, y)
            row = (row + 1) % 200
            x = x + 80

define draw_marker(image, frame, x, y):
    let left = 80 * camera_row + 40
    let top = 20 * (camera_col + 1)
    if x < left or x > left + 383 or y < top or y > top + 474:
        return
    blit_frame(image, frame, x - left - (top + 434) / 2 + 20, y - top - frame_height(image, frame) + 7)

define draw_player_markers():
    for i in 0..6:
        let f = player_forces[i]
        if f.active != 0:
            let offset = 1
            if f.selected == 1:
                offset = 6
            if i == 5:
                offset = 2
                if f.selected == 1:
                    offset = 7
            else if i == ridden_force:
                offset = 5
                if f.selected == 1:
                    offset = 0
            draw_marker(f.image, f.frame + offset, INT32(f.x), INT32(f.y))

define draw_hostile_markers():
    for s in 0..5:
        let f = hostile_forces[s]
        if f.active != 0:
            draw_marker(f.image, f.frame, INT32(f.x), INT32(f.y))

define draw_route_preview():
    if route_drawing == 0 or map_busy != 0:
        return
    let f = player_forces[selected_force]
    fn_00011CC0(INT32(f.x), INT32(f.y), f.points, f.count)
```

## Outputs

The drawn view. Offsets 1 and 6 mark an army, 5 and 0 the army the player rides with, and 2 and 7
the player's figure, unselected and selected. Brigand markers are drawn by the brigand pass
(RULE-STRATEGY-017) with frame 3 of `brigand_image`.

## Edge cases

A marker is drawn only when its point lies inside the view's rectangle in route units, edges
included.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Where `blit_tile` puts a tile relative to its position.
- Which coordinate of the figure's marker can come from the selected record.
- Whether the figure's offsets or the ridden army's win when `ridden_force` is 5.
- What `fn_00011CC0` does in full: its error term, the dot spacing and the counter.
