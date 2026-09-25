---
id: RULE-STRATEGY-010
title: Player records on the map
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-DRAGON-001, FND-STRATEGY-001, FND-STRATEGY-003, FND-STRATEGY-013, FND-STRATEGY-019, FND-STRATEGY-020, FND-STRATEGY-021, FND-STRATEGY-024, FND-STRATEGY-026, FND-STRATEGY-032, FND-STRATEGY-036]
conflicting: []
split_with: []
related: [RULE-DRAGON-001, RULE-JOUST-003, RULE-STRATEGY-007, RULE-STRATEGY-009, RULE-STRATEGY-011, RULE-STRATEGY-014]
---

## Summary

Each pass moves every live player record along its drawn route or toward the force it chases, then
tests it against the hostile forces. An army that comes within 30 units of one fights it; the
player's figure alone is only warned, at most once every 120 passes. The record the player rides
with ends the map session at the dragon's lair.

## When it runs

From the strategic pass (RULE-STRATEGY-001).

## Parameters

`i` the player record.

## Inputs

The floating-point arithmetic runs on the x87 with the precision and rounding in effect at the time, which no finding records; the procedure assumes the processor's starting rounding to nearest even and a 64-bit significand, and `INT32` truncates toward zero.

## Procedure

```text
define player_pass():
    for i in 0..6:
        let f = player_forces[i]
        if f.active == 1:
            if f.cooldown > 0:
                f.cooldown = f.cooldown - 1
            step_player(i)
            for j in 0..5:
                let h = hostile_forces[j]
                if h.active != 0 and abs(f.x - h.x) < 30.0 and abs(f.y - h.y) < 30.0:
                    player_forces[selected_force].selected = 0
                    selected_force = i
                    f.selected = 1
                    if i != 5:
                        if map_session_over == 0:
                            fight_hostile(i, j)
                    else if f.cooldown == 0:
                        focus_on(f.cell_row, f.cell_col)
                        # shows the warning that a hostile force is near
                        f.cooldown = 120
            let lair = [[63, 116], [62, 114], [64, 115], [63, 114], [63, 113]]
            for each c in lair:
                if i == ridden_force and f.cell_row == c[0] and f.cell_col == c[1]:
                    # saves the screen
                    fn_0003CED8()
                    fn_0005B2C0()
                    g_0009AAC4 = -1
                    let won = dragon_encounter()
                    # restores the screen and shows the victory message and media when won is 1, the death message otherwise
                    if won == 1:
                        fn_0001BF54(1)
                    else:
                        fn_0001BF54(0)
                    fn_00024CA0()
                    map_session_over = 1
                    return

define aim_player(f):
    let dx = f.dest_x - INT32(f.x)
    let dy = f.dest_y - INT32(f.y)
    let n = isqrt(dx * dx + dy * dy)
    f.dir_x = FLOAT32(FLOAT64(dx) / n)
    f.dir_y = FLOAT32(FLOAT64(dy) / n)

define stop_player(i):
    let f = player_forces[i]
    player_forces[selected_force].selected = 0
    f.target = 0
    f.count = 0
    selected_force = i
    f.selected = 1
    route_drawing = 0
    f.complete = 1
    # calls the pointer routines
    f.dest_x = INT32(f.x)
    f.dest_y = INT32(f.y)
    return 1

define step_player(i):
    let f = player_forces[i]
    if f.active == 0 or f.complete != 0:
        return 0
    if f.target != 0:
        let n = f.target & 0xFF
        let t = brigand_forces[n]
        if (f.target & 0x1000) != 0:
            t = hostile_forces[n]
        if t.active == 0:
            f.count = 0
            f.cursor = 0
            f.complete = 0
            f.target = 0
            return 0
        f.dest_x = INT32(t.x)
        f.dest_y = INT32(t.y)
        aim_player(f)
    else:
        f.dest_x = f.points[2 * f.cursor]
        f.dest_y = f.points[2 * f.cursor + 1]
    let ex = f.dest_x - INT32(f.x)
    let ey = f.dest_y - INT32(f.y)
    if (abs(ex) < 6 and abs(ey) < 6) or (ex >= 0 and f.dir_x < 0) or (ex <= 0 and f.dir_x > 0) or (ey >= 0 and f.dir_y < 0) or (ey <= 0 and f.dir_y > 0):
        f.cursor = f.cursor + 1
        if f.cursor == f.count or f.target != 0:
            f.complete = 1
            f.count = 0
            f.cursor = 0
            f.target = 0
            route_drawing = 0
            # calls the pointer routines
            return 0
        f.dest_x = f.points[2 * f.cursor]
        f.dest_y = f.points[2 * f.cursor + 1]
        aim_player(f)
    let w = terrain_speed(f.terrain_kind)
    let k = strategic_speed
    let px = INT32(f.x + INT32(f.dir_x) * INT32(w) * k)
    let py = INT32(f.y + INT32(f.dir_y) * INT32(w) * k)
    if not (f.dir_x <= 40.0 and f.dir_x >= -40.0 and f.dir_y <= 40.0 and f.dir_y >= -40.0):
        return stop_player(i)
    place(f, px, py)
    let kind = terrain_kind_at(f.cell_row, f.cell_col)
    if kind == 9:
        let qx = INT32(f.x) + INT32(f.dir_x * 50.0)
        let qy = INT32(f.y) + INT32(f.dir_y * 50.0)
        if qx < 0 or qy < 0:
            return stop_player(i)
        let c = pick_cell(qx, qy)
        if count(c) == 2 and terrain_kind_at(c[0], c[1]) == 9:
            focus_on(f.cell_row, f.cell_col)
            # shows the message that the force cannot cross
            return stop_player(i)
    f.terrain_kind = kind
    f.x = FLOAT32(f.x + f.dir_x * w * k)
    let ny = f.y + f.dir_y * w * k
    f.y = FLOAT32(ny)
    place(f, INT32(f.x), INT32(ny))
    return 0
```

## Outputs

The records' positions, cells, routes and selection, the warnings, and the battles of
RULE-STRATEGY-011. At the lair the dragon encounter of RULE-DRAGON-001 runs, and the map session ends whatever its
result.

## Edge cases

The speed used for a step is the kind the record last moved onto, which can be 9 when the record
stepped into water whose far side was dry. A chase ends at the first waypoint arrival, so the record
stops on reaching its target's position. A lost chase target clears `complete` together with the
route, so the record then walks point 0 of its own route list. The dragon's lair is tested for the
ridden record only, after the hostile scan, and the pass ends there. A direction that is not a
number stops the record.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_0003CED8`, `fn_0005B2C0`, `fn_0001BF54`, `fn_00024CA0` and `g_0009AAC4` are.
- What `pick_cell` leaves in the second probe's cell when the probe finds no cell; the procedure
  treats it as passable.
