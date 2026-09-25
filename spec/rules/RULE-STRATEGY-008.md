---
id: RULE-STRATEGY-008
title: Retargeting after arrival
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-003, FND-STRATEGY-007, FND-STRATEGY-012, FND-STRATEGY-015, FND-STRATEGY-019, FND-STRATEGY-023]
conflicting: []
split_with: []
related: [FMT-STRATEGY-001, RULE-STRATEGY-014]
---

## Summary

A force that has arrived without meeting anyone becomes a direct movement toward the first player
army within 300 units, or else toward the nearer of the player's home and its own castle, a tie
going to the home.

## When it runs

From the hostile pass after an arrival (RULE-STRATEGY-002).

## Parameters

`s` the hostile record.

## Inputs

The floating-point arithmetic runs on the x87 with the precision and rounding in effect at the time, which no finding records; the procedure assumes the processor's starting rounding to nearest even and a 64-bit significand, and `INT32` truncates toward zero.

## Procedure

```text
define retarget(s):
    let f = hostile_forces[s]
    f.mode = MOVE_DIRECT
    for i in 0..5:
        let g = player_forces[i]
        if g.active == 1:
            f.dest_x = INT32(g.x)
            f.dest_y = INT32(g.y)
            let dx = f.dest_x - INT32(f.x)
            let dy = f.dest_y - INT32(f.y)
            let root = square_root(FLOAT64(dx * dx + dy * dy))
            if root < 300.0:
                let d = FLOAT32(root)
                f.dir_x = FLOAT32(dx / d)
                f.dir_y = FLOAT32(dy / d)
                return
    let hx = anchor_x(home_row)
    let hy = anchor_y(home_col)
    f.dest_x = hx
    f.dest_y = hy
    let hdx = hx - INT32(f.x)
    let hdy = hy - INT32(f.y)
    let home_dist = FLOAT32(square_root(FLOAT64(hdx * hdx + hdy * hdy)))
    f.dest_x = properties[f.origin].map_x
    f.dest_y = properties[f.origin].map_y
    let odx = f.dest_x - INT32(f.x)
    let ody = f.dest_y - INT32(f.y)
    let origin_dist = FLOAT32(square_root(FLOAT64(odx * odx + ody * ody)))
    if home_dist <= origin_dist:
        f.dest_x = hx
        f.dest_y = hy
        f.dir_x = FLOAT32(hdx / home_dist)
        f.dir_y = FLOAT32(hdy / home_dist)
    else:
        f.dir_x = FLOAT32(odx / origin_dist)
        f.dir_y = FLOAT32(ody / origin_dist)
```

## Outputs

The record's mode, destination and direction.

## Edge cases

Only records 0 to 4 are tested, so the player's figure is never a target. A force standing on its
target divides by 0.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
