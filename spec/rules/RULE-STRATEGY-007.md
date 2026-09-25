---
id: RULE-STRATEGY-007
title: Routed movement
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-011, FND-STRATEGY-013]
conflicting: []
split_with: []
related: [RULE-STRATEGY-009, RULE-STRATEGY-014]
---

## Summary

A routed force walks to its route points one at a time. It moves on to the next point when it is
within 6 units on both axes or has passed the point, and it drops its route at the last point, at a
zero length, after a step of more than 50 units, or on impassable terrain.

## When it runs

From the hostile pass for mode 2 (RULE-STRATEGY-002).

## Parameters

`f` the hostile record. `step_routed` returns 0, or `0xFFFF` when the force has stopped.

## Inputs

The floating-point arithmetic runs on the x87 with the precision and rounding in effect at the time, which no finding records; the procedure assumes the processor's starting rounding to nearest even and a 64-bit significand, and `INT32` truncates toward zero.

## Procedure

```text
define isqrt(v):
    if v <= 1:
        return v
    let g = v / 2
    let next = (g + v / g) / 2
    while next < g:
        g = next
        next = (g + v / g) / 2
    return g

define drop_route(f):
    free(f.route)
    f.route = 0
    f.complete = 1

define step_routed(f):
    if f.complete != 0:
        return 0
    f.dest_x = f.route[2 * f.cursor]
    f.dest_y = f.route[2 * f.cursor + 1]
    let ex = f.dest_x - INT32(f.x)
    let ey = f.dest_y - INT32(f.y)
    if (abs(ex) < 6 and abs(ey) < 6) or (ex >= 0 and f.dir_x < 0) or (ex <= 0 and f.dir_x > 0) or (ey >= 0 and f.dir_y < 0) or (ey <= 0 and f.dir_y > 0):
        f.cursor = f.cursor + 1
        if f.cursor == f.count:
            drop_route(f)
            return 0xFFFF
        f.dest_x = f.route[2 * f.cursor]
        f.dest_y = f.route[2 * f.cursor + 1]
        ex = f.dest_x - INT32(f.x)
        ey = f.dest_y - INT32(f.y)
    let n = isqrt(ex * ex + ey * ey)
    if n <= 0 or INT32(f.dir_x) > 50 or INT32(f.dir_x) < -50 or INT32(f.dir_y) > 50 or INT32(f.dir_y) < -50:
        drop_route(f)
        f.active = 0
        return 0xFFFF
    place(f, INT32(f.x) + INT32(f.dir_x), INT32(f.y) + INT32(f.dir_y))
    let kind = terrain_kind_at(f.cell_row, f.cell_col)
    if kind == 9:
        drop_route(f)
        f.active = 0
        return 0xFFFF
    let w = terrain_speed(kind)
    let ux = FLOAT32(FLOAT64(ex) / n)
    let uy = FLOAT32(FLOAT64(ey) / n)
    let step_x = ux * w * strategic_speed
    f.dir_x = FLOAT32(step_x)
    f.dir_y = FLOAT32(uy * w * strategic_speed)
    if abs(step_x) < 50.0:
        f.x = FLOAT32(f.x + f.dir_x)
        f.y = FLOAT32(f.y + f.dir_y)
    return 0
```

## Outputs

The record's position, cell, direction, cursor and route.

## Edge cases

The crossing test is not strict: a difference of 0 with a direction pointing either way counts as
passed. The size test on the previous step comes before the new step, so a step of 50 or more whose
truncation is still 50 leaves the force in place and a larger one stops it on the next call. None of
the three stops lowers `hostile_count` (BUG-STRATEGY-004). A force that reaches its last point keeps
`active`, and the hostile pass then resolves its arrival.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
