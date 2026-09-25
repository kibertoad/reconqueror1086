---
id: RULE-STRATEGY-013
title: Map clicks and drawn routes
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-JOUST-002, FND-STRATEGY-003, FND-STRATEGY-018, FND-STRATEGY-019, FND-STRATEGY-026, FND-STRATEGY-027, FND-STRATEGY-032]
conflicting: []
split_with: []
related: []
---

## Summary

A click on the map picks a player record, asks to attack a clicked hostile or brigand force, or adds
a point to the selected record's route. A route holds at most 20 points.

## When it runs

When the player clicks the map view; `remove_route_point` and `cancel_route` from the route buttons.

## Parameters

`x` and `y` a point in route units.

## Inputs

`pointer_x`, `pointer_y`, the camera and `player_confirms_attack`.

## Procedure

```text
define in_rect(x, y, left, top, width, height):
    return x >= left and x < left + width and y >= top and y < top + height

define map_click():
    let x = pointer_x + 80 * camera_row + 40
    let y = pointer_y + 20 * (camera_col + 1)
    for i in 0..6:
        let f = player_forces[i]
        if f.active != 0 and in_rect(x, y, INT32(f.x) + 11, INT32(f.y) - 30, 24, 31):
            selected_force = i
            route_drawing = 1
            return
    for j in 0..5:
        let h = hostile_forces[j]
        if h.active != 0 and in_rect(x, y, INT32(h.x), INT32(h.y) - 20, 40, 40):
            if player_confirms_attack:
                let g = player_forces[selected_force]
                g.target = j | 0x1000
                g.count = 0
                g.cursor = 0
                g.complete = 0
                route_drawing = 0
            return
    for d in 0..3:
        let b = brigand_forces[d]
        if b.active != 0 and in_rect(x, y, INT32(b.x), INT32(b.y) - 20, 40, 40):
            if player_confirms_attack:
                let g = player_forces[selected_force]
                g.target = d | 0x10000
                g.count = 0
                g.cursor = 0
                g.complete = 0
                route_drawing = 0
            return
    add_route_point(x, y)

define add_route_point(x, y):
    let f = player_forces[selected_force]
    if route_drawing == 0:
        f.target = 0
        f.count = 0
        f.cursor = 0
        f.complete = 0
        route_drawing = 1
    if f.count < 20:
        f.points[2 * f.count] = x
        f.points[2 * f.count + 1] = y
        f.count = f.count + 1
        if f.count == 1:
            fn_000120E4()

define remove_route_point():
    let f = player_forces[selected_force]
    if f.count > 0:
        f.count = f.count - 1
    if f.count == 0:
        f.complete = 1
        f.target = 0
        route_drawing = 0

define cancel_route():
    route_drawing = 0
```

## Outputs

The selection, the selected record's target or route, and `route_drawing`. The attack prompt is a
message box.

## Edge cases

The first hit wins, player records before hostile forces before brigands. A click on a force that the
player declines is used up and adds no point. A player target sets bit `0x1000` for a hostile force
and bit `0x10000` for a brigand, and RULE-STRATEGY-010 reads the index from the low byte. A removed
point is left in the list.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_000120E4` sets for the first route point beyond the destination and direction.
- What `remove_route_point` does with no point left; the procedure assumes it removes nothing.
