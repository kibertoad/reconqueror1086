---
id: RULE-STRATEGY-014
title: Strategic grid, projection and camera
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-JOUST-002, FND-STRATEGY-017, FND-STRATEGY-018, FND-STRATEGY-027]
conflicting: []
split_with: []
related: [FMT-STRATEGY-005, RULE-STRATEGY-015]
---

## Summary

Positions are in route units. A cell's anchor is 80 units per row and 20 per column. A point finds
its cell by moving a temporary camera until the point is in view and then scanning the staggered
diamond cells in drawing order; the first cell whose span holds the point wins, so the camera is part
of the state that decides a cell. Focusing puts a record near the view's upper left corner, and edge
panning moves the camera one cell along one axis per call.

## When it runs

Wherever a rule places a force, focuses the map or pans it.

## Parameters

`row` and `col` a cell; `x` and `y` a point in route units; `f` a movement record.

## Inputs

`world_grid` (FMT-STRATEGY-005), `camera_row`, `camera_col`, `pointer_x` and `pointer_y`.

## Procedure

```text
define anchor_x(row):
    return 80 * (row + 1)

define anchor_y(col):
    return 20 * (col + 1)

define cell_owner(row, col):
    return (world_grid[row][col] >> 16) & 0xFF

define cell_person(row, col):
    let v = cell_owner(row, col)
    if v >= 1 and v <= 176:
        return v
    return 0

define met_person(row, col):
    let p = cell_person(row, col)
    if p == 0:
        p = cell_person(row, col - 2)
    if p == 0:
        p = cell_person(row + 1, col)
    if p == 0:
        p = cell_person(row - 1, col - 1)
    if p == 0:
        p = cell_person(row, col + 1)
    return p

define pick_cell(x, y):
    let r = camera_row
    let c = camera_col
    let moved = true
    while moved:
        moved = false
        if 80 * r + 40 > x:
            r = r - (80 * r + 40 - x) / 80
            if r > 0:
                r = r - 1
            moved = true
        else if x > 80 * r + 423:
            r = r + (x - 80 * r - 40) / 80
            if r > 0:
                r = r - 1
            moved = true
        else if 20 * c + 20 > y:
            c = c - 2 * ((20 * c + 20 - y) / 40)
            if c > 0:
                c = c - 1
            moved = true
        else if y > 20 * c + 454:
            c = c + 2 * ((y - 20 * c - 20) / 40)
            if c > 0:
                c = c - 1
            moved = true
    let px = x - 80 * r - 20
    let py = y - 20 * c - 13
    if px < 20 or px > 403 or py < 7 or py > 441:
        return []
    for m in 0..24:
        let k = py - (-53 + 20 * m) - 40
        if k >= 0 and k <= 42:
            let w = 2 * k
            if k > 21:
                w = 84 - 2 * k
            let x0 = -20
            if (c + m) % 2 == 1:
                x0 = 20
            let row = r
            while x0 < 403:
                if abs(px - x0 - 40) <= w:
                    return [row, (c + m) % 400]
                row = (row + 1) % 200
                x0 = x0 + 80
    return []

define place(f, x, y):
    let cell = pick_cell(x, y)
    if count(cell) == 2:
        f.cell_row = cell[0]
        f.cell_col = cell[1]

define focus_on(row, col):
    camera_row = row - 2
    if camera_row < 0:
        camera_row = camera_row + 200
    camera_col = max(col - 11, 0)
    if camera_row > 192:
        camera_row = 192
    if camera_row < 10:
        camera_row = 10
    if camera_col < 2:
        camera_col = 2
    if camera_col > 300:
        camera_col = 300
    draw_terrain()
    draw_player_markers()
    draw_hostile_markers()
    # shows the frame

define pan_camera():
    map_busy = 1
    if pointer_x > 628 and camera_row < 192:
        camera_row = (camera_row + 1) % 200
        return
    if pointer_x < 8 and camera_row > 10:
        camera_row = camera_row - 1
        if camera_row < 0:
            camera_row = 199
        return
    if pointer_y > 474 and camera_col < 300:
        camera_col = (camera_col + 1) % 400
        return
    if pointer_y < 8 and camera_col > 2:
        camera_col = camera_col - 1
        if camera_col < 0:
            camera_col = 399
        return
    map_busy = 0
```

## Outputs

`pick_cell` gives the cell as a row and a column, or an empty list when the point is outside the view
of the moved camera. The global camera is left unchanged. `place` leaves the record's cell alone when
no cell is found.

## Edge cases

A cell covers the 43 lines from 40 below its line's top: its half-width grows by 2 a line to 42 on
line 21 and then shrinks, so the lower half is one line taller than the upper. Where two cells
overlap, the one the scan reaches first wins, and with the camera at `(0, 0)` and at `(199, 399)` 78
of the 8,304 points of the `rt_` and `sc_` routes, at 49 coordinates, find different cells.
`met_person` passes cells outside the grid, such as column -2, to `cell_person` unchecked. A panning
call that moves the camera leaves `map_busy` set, which holds back the hostile pass until a later
call finds the pointer off the edges, and a corner moves the camera along one axis only.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Whether `cell_person` wraps or rejects a row or column outside the grid.
