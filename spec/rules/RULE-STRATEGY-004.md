---
id: RULE-STRATEGY-004
title: Hostile force construction and removal
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-003, FND-STRATEGY-006, FND-STRATEGY-007, FND-STRATEGY-012, FND-STRATEGY-015, FND-STRATEGY-016, FND-STRATEGY-019, FND-STRATEGY-023, FND-STRATEGY-030]
conflicting: []
split_with: []
related: [FMT-STRATEGY-001, FMT-STRATEGY-004, RULE-STRATEGY-005, RULE-STRATEGY-007, RULE-STRATEGY-014]
---

## Summary

A hostile force takes the first free record. A pursuit starts at its property's anchor and heads for
a player record's position. An ordinary movement starts at the property and either walks straight to
the target person's cell or follows a route file: a road between two properties chosen by the
target's group, or the route of the player's starting home.

## When it runs

From the hostile generator (RULE-STRATEGY-003) and the hostile pass (RULE-STRATEGY-002).

## Parameters

`t` a player record for `pursue` and a person for `construct_hostile`; `p` and `o` a property; `m` the mode; `flag` 1 for a road, 0 for the home route.

## Inputs

The floating-point arithmetic runs on the x87 with the precision and rounding in effect at the time, which no finding records; the procedure assumes the processor's starting rounding to nearest even and a 64-bit significand, and `INT32` truncates toward zero.

## Procedure

```text
define free_hostile_slot():
    for s in 0..5:
        if hostile_forces[s].active == 0:
            return s
    return -1

define destroy_hostile(s):
    let f = hostile_forces[s]
    if f.active != 1:
        return
    if f.complete == 0 and f.mode == MOVE_ROUTED:
        free(f.route)
        f.route = 0
        f.complete = 1
    hostile_count = hostile_count - 1
    f.active = 0
    # when hostile_count is now below 0 the game stops with an error message

define pursue(t, p):
    if t >= 5:
        return 0
    let s = free_hostile_slot()
    if s < 0 or properties[p].garrison <= 0:
        return 0
    let f = hostile_forces[s]
    let g = player_forces[t]
    f.origin = p
    f.lord = properties[p].lord
    f.target = t
    f.frame = marker_words[p] << 3
    f.active = 1
    f.dest_x = INT32(g.x)
    f.dest_y = INT32(g.y)
    f.cell_row = g.cell_row
    f.cell_col = g.cell_col
    f.mode = MOVE_PURSUIT
    f.x = FLOAT32(anchor_x(properties[p].cell_row))
    f.y = FLOAT32(anchor_y(properties[p].cell_col))
    let dx = f.dest_x - INT32(f.x)
    let dy = f.dest_y - INT32(f.y)
    let n = isqrt(dx * dx + dy * dy)
    f.dir_x = FLOAT32(FLOAT64(dx) / n)
    f.dir_y = FLOAT32(FLOAT64(dy) / n)
    size_pursuit(s, t)
    hostile_count = hostile_count + 1
    return 1

define construct_hostile(t, o, m, flag):
    if properties[o].alerted == 0:
        return 0
    let s = free_hostile_slot()
    if s < 0:
        return 0
    let f = hostile_forces[s]
    if m == MOVE_DIRECT:
        f.origin = o
        f.lord = properties[o].lord
        f.active = 1
        f.mode = MOVE_DIRECT
        f.frame = marker_words[o] << 3
        f.dest_x = anchor_x(persons[t].cell_row)
        f.dest_y = anchor_y(persons[t].cell_col)
        f.x = FLOAT32(anchor_x(properties[o].cell_row))
        f.y = FLOAT32(anchor_y(properties[o].cell_col))
        f.cell_row = properties[o].cell_row
        f.cell_col = properties[o].cell_col
        let dx = f.dest_x - INT32(f.x)
        let dy = f.dest_y - INT32(f.y)
        let n = square_root(FLOAT64(dx * dx + dy * dy))
        f.dir_x = FLOAT32(dx / n)
        f.dir_y = FLOAT32(dy / n)
    else if m == MOVE_ROUTED:
        if build_route(f, o, t, flag) != 1:
            return 0
        f.lord = properties[o].lord
        f.frame = marker_words[o] << 3
        f.mode = MOVE_ROUTED
        f.origin = o
        f.active = 1
        f.x = FLOAT32(anchor_x(properties[o].cell_row))
        f.y = FLOAT32(anchor_y(properties[o].cell_col))
        f.cell_row = properties[o].cell_row
        f.cell_col = properties[o].cell_col
    else:
        return 0
    size_force(s)
    hostile_count = hostile_count + 1
    return 1

define build_route(f, o, t, flag):
    let name = ""
    if flag == 1:
        let g = persons[t].group
        if g < 0 or g > 13 or route_matrix[o][g] == 0:
            f.route = 0
            f.complete = 1
            return 0
        if route_matrix[o][g] == 1:
            name = sprintf("rt_%d_%d.rat", o + 1, g + 1)
            f.reversed = 0
        else:
            name = sprintf("rt_%d_%d.rat", g + 1, o + 1)
            f.reversed = 1
    else:
        name = start_routes[start_home]
        f.reversed = 0
    let file = resource(name)
    let points = copy(file.points)
    if f.reversed == 1:
        let back = []
        let k = file.point_count - 1
        while k >= 0:
            append(back, points[2 * k])
            append(back, points[2 * k + 1])
            k = k - 1
        points = back
    f.route = points
    f.cursor = 0
    f.complete = 0
    f.count = file.point_count
    return 1
```

## Outputs

A new force is live, counted in `hostile_count` and sized by RULE-STRATEGY-005. `build_route` reads
a route file (FMT-STRATEGY-004).

## Edge cases

Neither constructor guards a zero length: a pursuit from an anchor equal to the player's truncated
position, or a direct movement to its own cell, divides by 0. A routed force keeps the `dir_x` and
`dir_y` the record held before. `construct_hostile` needs the origin to be alerted, so the home
movement from `fallback_origin` needs that property alerted too. A mode other than 1 or 2 gives 0.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
