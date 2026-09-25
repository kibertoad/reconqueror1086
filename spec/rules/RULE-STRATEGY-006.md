---
id: RULE-STRATEGY-006
title: Direct and pursuit movement
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-003, FND-STRATEGY-007, FND-STRATEGY-009, FND-STRATEGY-010, FND-STRATEGY-012, FND-STRATEGY-013, FND-STRATEGY-015, FND-STRATEGY-019]
conflicting: []
split_with: []
related: [FMT-STRATEGY-001, RULE-STRATEGY-007, RULE-STRATEGY-009, RULE-STRATEGY-014]
---

## Summary

A direct force steps along its normalized direction at 0.9 times the terrain speed until it passes
its destination. A pursuit re-aims at the hunted player record each step and only reports arrival
when that record disappears. Impassable terrain sends a direct force's troops home and turns a
pursuit into a direct movement back to its castle.

## When it runs

From the hostile pass for modes 1 and 3 (RULE-STRATEGY-002).

## Parameters

`s` the hostile record. Each returns 0, or `0xFFFF` when the force has arrived or stopped.

## Inputs

The floating-point arithmetic runs on the x87 with the precision and rounding in effect at the time, which no finding records; the procedure assumes the processor's starting rounding to nearest even and a 64-bit significand, and `INT32` truncates toward zero.

## Procedure

```text
define step_direct(s):
    let f = hostile_forces[s]
    let ex = f.dest_x - INT32(f.x + f.dir_x)
    let ey = f.dest_y - INT32(f.y + f.dir_y)
    if (ex > 0 and f.dir_x < 0) or (ex < 0 and f.dir_x > 0) or (ey > 0 and f.dir_y < 0) or (ey < 0 and f.dir_y > 0):
        f.dest_x = INT32(f.x)
        f.dest_y = INT32(f.y)
        return 0xFFFF
    place(f, INT32(f.x) + INT32(f.dir_x), INT32(f.y) + INT32(f.dir_y))
    let kind = terrain_kind_at(f.cell_row, f.cell_col)
    if kind == 9:
        f.dest_x = INT32(f.x)
        f.dest_y = INT32(f.y)
        f.active = 0
        let o = properties[f.origin]
        o.garrison = UINT8(o.garrison + f.halberdiers + f.swordsmen + f.knights)
        return 0xFFFF
    let w = terrain_speed(kind)
    f.x = FLOAT32(f.x + f.dir_x * 0.9 * w * strategic_speed)
    f.y = FLOAT32(f.y + f.dir_y * 0.9 * w * strategic_speed)
    place(f, INT32(f.x), INT32(f.y))
    return 0

define step_pursuit(s):
    let f = hostile_forces[s]
    let g = player_forces[f.target]
    if g.active == 0:
        return 0xFFFF
    f.dest_x = INT32(g.x)
    f.dest_y = INT32(g.y)
    let dx = f.dest_x - INT32(f.x + f.dir_x)
    let dy = f.dest_y - INT32(f.y + f.dir_y)
    let n = isqrt(dx * dx + dy * dy)
    if n <= 0:
        f.dir_x = 1.0
        f.dir_y = 1.0
    else:
        f.dir_x = FLOAT32(FLOAT64(dx) / n)
        f.dir_y = FLOAT32(FLOAT64(dy) / n)
    place(f, INT32(f.x) + INT32(f.dir_x), INT32(f.y) + INT32(f.dir_y))
    let kind = terrain_kind_at(f.cell_row, f.cell_col)
    if kind == 9:
        f.dest_x = properties[f.origin].map_x
        f.dest_y = properties[f.origin].map_y
        let hx = f.dest_x - INT32(f.x)
        let hy = f.dest_y - INT32(f.y)
        let r = INT32(square_root(FLOAT64(hx * hx + hy * hy)))
        if r <= 0:
            f.dir_x = 1.0
            f.dir_y = 1.0
        else:
            f.dir_x = FLOAT32(FLOAT64(hx) / r)
            f.dir_y = FLOAT32(FLOAT64(hy) / r)
        f.mode = MOVE_DIRECT
        f.x = FLOAT32(f.x + f.dir_x)
        f.y = FLOAT32(f.y + f.dir_y)
        return 0
    let w = terrain_speed(kind)
    f.x = FLOAT32(f.x + f.dir_x * w * strategic_speed)
    f.y = FLOAT32(f.y + f.dir_y * w * strategic_speed)
    place(f, INT32(f.x), INT32(f.y))
    return 0
```

## Outputs

The record's position, cell, direction, destination and mode, and the origin's garrison.

## Edge cases

The direct crossing test is strict: a difference of 0 does not stop the force. Because the direction
is normalized the probed cell is almost always the current one. A direct force stopped by water
leaves `hostile_count` unchanged (BUG-STRATEGY-004), and the garrison wraps at 256. The direct step
divides by nothing, but a direction of infinity from RULE-STRATEGY-004 gives an undefined position.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
