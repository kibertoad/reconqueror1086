---
id: RULE-BATTLE-007
title: Field battle idle update
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-004, FND-BATTLE-008, FND-BATTLE-011, FND-BATTLE-013, FND-BATTLE-014]
conflicting: []
split_with: []
related: [RULE-BATTLE-004, RULE-BATTLE-006, RULE-STRATEGY-007, FMT-BATTLE-001]
---

## Summary

A unit with nothing to do looks for a foe touching a corner of its rectangle and turns to fight it.
Otherwise it turns toward its destination and walks there, the y axis first, 10 pixels an update for
a knight and 5 for others. A friend in the way pushes the destination back; a foe in the way becomes the
target. A unit on automatic with no destination heads for the nearest foe.

## When it runs

From `update_unit` (RULE-BATTLE-003) for a unit in state 0, after any contact in the same update.

## Parameters

`update_idle(i)`: a unit index.

## Inputs

`battle_units`, `battle_rects`, `battle_player_alive`, `battle_foe_alive`, `battle_field_width` and
`battle_field_height`. The distance is computed on the x87 as the square root of an exact integer and
truncated, which gives the same value as `isqrt` for the distances a field of 1024 by 728 allows.

## Procedure

```text
define update_idle(i):
    let u = battle_units[i]
    let last = acquire_target(i)
    if u.target != -1:
        face_target(i, last)
        return
    if u.dest_x == -1 and u.dest_y == -1:
        auto_destination(i)
        return
    let o = octant(u.x, u.y, u.dest_x, u.dest_y)
    if o != u.heading:
        turn_toward(u, o)
        return
    if u.dest_y != -1:
        walk_y(i)
    if u.dest_x != -1:
        walk_x(i)

define acquire_target(i):
    let u = battle_units[i]
    let r = battle_rects[i]
    battle_rects[i] = [battle_field_width + 1, battle_field_height + 1, 1, 1]
    let points = [[r[0], r[1]], [r[0], r[1] + r[3]], [r[0] + r[2], r[1]], [r[0] + r[2], r[1] + r[3]]]
    let seen = false
    let last = 0
    for each p in points:
        let s = hit_test(p[0], p[1], battle_rects, count(battle_units))
        if s == 0:
            last = 0
            if seen:
                break
            continue
        last = s - 1
        let t = battle_units[s - 1]
        if t.lane != u.lane and t.strength > 0:
            u.target = s - 1
            break
        seen = true
    battle_rects[i] = r
    return last

define walk_y(i):
    let u = battle_units[i]
    if u.y == u.dest_y:
        u.dest_y = -1
        u.control = 1
        return
    let d = u.dest_y - u.y
    if abs(d) < 10:
        let p = probe_neighbour(i, 0, d)
        if p[0] == 0:
            u.phase = (u.phase + 1) % 5
            u.y = u.dest_y
            u.dest_y = -1
        else if p[0] == 2:
            u.phase = (u.phase + 1) % 5
            u.y = u.dest_y
            u.target = p[1]
            face_target(i, p[1])
        else:
            u.control = 1
            u.dest_y = -1
        return
    let step = 5
    if u.category == 0xF0:
        step = 10
    if d < 0:
        step = -step
    let p = probe_neighbour(i, 0, step)
    if p[0] == 0 or p[0] == 2:
        u.phase = (u.phase + 1) % 5
        u.y = u.y + step
        if p[0] == 2:
            u.target = p[1]
            face_target(i, p[1])
        return
    u.control = 1
    if d > 0:
        let v = u.dest_y - 5
        if v < 0:
            v = battle_field_height - 90
        u.dest_y = v
    else:
        let v = u.dest_y + 5
        if v >= battle_field_height - 90:
            v = 90
        u.dest_y = v

define walk_x(i):
    let u = battle_units[i]
    if u.x == u.dest_x:
        u.dest_x = -1
        u.control = 1
        return
    let d = u.dest_x - u.x
    if abs(d) < 10:
        let p = probe_neighbour(i, d, 0)
        if p[0] == 0:
            u.phase = (u.phase + 1) % 5
            u.x = u.dest_x
            u.dest_x = -1
        else if p[0] == 2:
            u.phase = (u.phase + 1) % 5
            u.x = u.dest_x
            u.target = p[1]
            face_target(i, p[1])
        else:
            u.control = 1
            u.dest_x = -1
        return
    let step = 5
    if u.category == 0xF0:
        step = 10
    if d < 0:
        step = -step
    let p = probe_neighbour(i, step, 0)
    if p[0] == 0 or p[0] == 2:
        u.phase = (u.phase + 1) % 5
        u.x = u.x + step
        if p[0] == 2:
            u.target = p[1]
            face_target(i, p[1])
        return
    u.control = 1
    if d > 0:
        let v = u.dest_x - 5
        if v < 0:
            v = battle_field_width - 90
        u.dest_x = v
    else:
        let v = u.dest_x + 5
        if v >= battle_field_width - 90:
            v = 90
        u.dest_x = v

define auto_destination(i):
    let u = battle_units[i]
    if u.control != 1 or u.strength <= 0 or battle_foe_alive <= 0 or battle_player_alive <= 0:
        return
    let best = -1
    let best_distance = 0x7FFF
    for j in 0..count(battle_units):
        let t = battle_units[j]
        if j == i or t.lane == u.lane or t.strength <= 0:
            continue
        let d = isqrt((u.x - t.x) * (u.x - t.x) + (u.y - t.y) * (u.y - t.y))
        if d < best_distance:
            best = j
            best_distance = d
    if best == -1:
        return
    let t = battle_units[best]
    if t.dest_x == -1 or t.dest_x == u.x:
        u.dest_x = t.x
    else:
        u.dest_x = t.dest_x
    if t.dest_y == -1:
        u.dest_y = t.y
    else:
        u.dest_y = t.dest_y
```

## Outputs

No return value. Changes the unit's target, destination, position, heading, phase, state and control
code.

## Edge cases

A unit that finds a foe while walking on y still walks on x in the same update. A unit that kept its
target from an earlier update, because it was still turning toward it, turns toward whatever
`acquire_target` hit last, or toward unit 0 after a miss (BUG-BATTLE-002). Reaching a destination on
either axis, or meeting a friend, puts the unit on automatic (control code 1). A friend in the way
moves a far destination 5 pixels back toward the unit and wraps it to 90 from the far edge when it
would leave the field. A unit on automatic that chose a foe which is itself walking copies that foe's
destination on each axis where it has one, except that on x it takes the foe's position when the foe
is heading for the unit's own x.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Whether `0x00063EC0` makes the distance truncate, as the procedure assumes.
