---
id: RULE-STRATEGY-017
title: Brigand pass and brigand movement
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-001, FND-STRATEGY-001, FND-STRATEGY-013, FND-STRATEGY-014, FND-STRATEGY-015, FND-STRATEGY-019, FND-STRATEGY-021, FND-STRATEGY-022, FND-STRATEGY-024, FND-STRATEGY-026, FND-STRATEGY-029, FND-STRATEGY-030, FND-STRATEGY-032, FND-STRATEGY-033, FND-STRATEGY-034, FND-TOURNEY-001, FND-TOURNEY-004, FND-TALK-006]
conflicting: []
split_with: []
related: [RULE-BATTLE-001, RULE-PERSON-001, RULE-RNG-001, RULE-STRATEGY-007, RULE-STRATEGY-009, RULE-STRATEGY-014, RULE-STRATEGY-015]
---

## Summary

Each pass, every running brigand order first tests the player records for contact: the player's
figure is warned, and an army fights. A won fight pays a bounty and ends the order; a lost one marks
it fought and leaves the brigands on their route. The brigands then take a step along their looping
route, and an order whose date has come ends, costing honour when nobody fought a yearly order.

## When it runs

From the strategic pass (RULE-STRATEGY-001).

## Parameters

`d` a brigand slot; `i` a player army; `f` a brigand record.

## Inputs

The attributes HONOR (5), WEALTH (17), BATTLE_WON (24) and BATTLE_LOST (25) of row 0, and the
conversation state `g_0009A928`. `resolve_encounter` receives the player's pools 1, 0 and 2 then the
three brigand counts, with 0 and 0. The floating-point arithmetic runs on the x87 with the precision and
rounding in effect at the time, which no finding records; the procedure assumes the processor's
starting rounding to nearest even and a 64-bit significand, and `INT32` truncates toward zero.

## Procedure

```text
define brigand_pass():
    for d in 0..3:
        let o = brigand_orders[d]
        if o.active != 0:
            let b = brigand_forces[d]
            for i in 0..6:
                let f = player_forces[i]
                if f.active != 0 and abs(f.x - b.x) < 30.0 and abs(f.y - b.y) < 30.0:
                    if i == 5:
                        if f.cooldown == 0:
                            focus_on(f.cell_row, f.cell_col)
                            # shows the warning that brigands are near
                            f.cooldown = 120
                    else:
                        return fight_brigand(i, d)
            step_brigand(b)
            if current_month() >= o.end_month and current_year() >= o.end_year:
                o.active = 0
                b.active = 0
                free(b.route)
                if o.fought == 0 and properties[o.origin].state != 0 and d == 0:
                    set_attr(0, 5, attr(0, 5) - 1)
                    properties[o.origin].alerted = 1
                    # shows the message that the brigands were left alone
                return 0
            draw_marker(brigand_image, 3, INT32(b.x), INT32(b.y))
    return 1

define fight_brigand(i, d):
    let f = player_forces[i]
    let b = brigand_forces[d]
    let o = brigand_orders[d]
    focus_on(f.cell_row, f.cell_col)
    player_forces[selected_force].selected = 0
    f.selected = 1
    selected_force = i
    set_attr(0, 5, attr(0, 5) + 1)
    # saves the screen and shows the message that the army meets brigands
    fn_0003CED8()
    let counts = [fn_00029E58(i, 1), fn_00029E58(i, 0), fn_00029E58(i, 2), b.halberdiers, b.swordsmen, b.knights]
    f.target = 0
    let e = resolve_encounter(counts, 0, 0)
    # puts the brigand counts first and the pools back in their own order
    counts = [counts[3], counts[4], counts[5], counts[1], counts[0], counts[2]]
    if d == 1:
        fn_000628AC(g_0009A928, 0x2C, 1)
    if d == 2:
        fn_000628AC(g_0009A928, 0x61, 1)
    for k in 0..6:
        counts[k] = max(counts[k], 0)
    if e == 1:
        counts[0] = 0
        counts[1] = 0
        counts[2] = 0
    if counts[0] + counts[1] + counts[2] == 0:
        e = 1
    if counts[3] + counts[4] + counts[5] == 0:
        e = 0
    if e == 1:
        fn_00029DE0(i, counts[3], counts[4], counts[5])
    b.halberdiers = counts[0]
    b.swordsmen = counts[1]
    b.knights = counts[2]
    # plays a sound and restores the screen
    f.count = 0
    f.cursor = 0
    f.dest_x = INT32(f.x)
    f.dest_y = INT32(f.y)
    route_drawing = 0
    f.complete = 1
    if counts[0] + counts[1] + counts[2] != 0 and counts[3] + counts[4] + counts[5] != 0:
        return 1
    if e == 1:
        let bounty = random_inclusive(100) + 50
        if d == 1:
            bounty = 40
        set_attr(0, 17, attr(0, 17) + bounty)
        # shows the bounty message
        set_attr(0, 24, attr(0, 24) + 1)
        o.fought = 1
        o.active = 0
        free(b.route)
        b.active = 0
        return 1
    set_attr(0, 25, attr(0, 25) + 1)
    o.fought = 1
    if i == ridden_force:
        # shows the death message
        fn_000106C0()
        fn_0001BF54(2)
        map_session_over = 1
    else:
        fn_00029D24(i)
    return 1

define step_brigand(f):
    if f.complete != 0:
        return 0
    f.dest_x = f.route[2 * f.cursor]
    f.dest_y = f.route[2 * f.cursor + 1]
    let ex = f.dest_x - INT32(f.x)
    let ey = f.dest_y - INT32(f.y)
    if (abs(ex) < 6 and abs(ey) < 6) or (ex >= 0 and f.dir_x < 0) or (ex <= 0 and f.dir_x > 0) or (ey >= 0 and f.dir_y < 0) or (ey <= 0 and f.dir_y > 0):
        f.cursor = f.cursor + 1
        if f.cursor == f.count:
            f.cursor = 0
        f.dest_x = f.route[2 * f.cursor]
        f.dest_y = f.route[2 * f.cursor + 1]
        ex = f.dest_x - INT32(f.x)
        ey = f.dest_y - INT32(f.y)
    let n = isqrt(ex * ex + ey * ey)
    if n <= 0:
        f.cursor = 0
        f.count = 0
        f.complete = 1
        return 1
    place(f, INT32(f.x) + INT32(f.dir_x), INT32(f.y) + INT32(f.dir_y))
    let w = terrain_speed(terrain_kind_at(f.cell_row, f.cell_col))
    f.dir_x = FLOAT32(FLOAT64(ex) / n * w * strategic_speed)
    f.dir_y = FLOAT32(FLOAT64(ey) / n * w * strategic_speed)
    if INT32(f.x) + INT32(f.dir_x) < 0 or INT32(f.y) + INT32(f.dir_y) < 0:
        f.dir_x = 1.0
        f.dir_y = 1.0
        f.x = FLOAT32(f.x + 1)
        f.y = FLOAT32(f.y + 1)
        return 0
    if f.dir_x > -3.4028234663852886e38 and f.dir_x < 3.4028234663852886e38 and f.dir_y > -3.4028234663852886e38 and f.dir_y < 3.4028234663852886e38:
        f.x = FLOAT32(f.x + f.dir_x)
        f.y = FLOAT32(f.y + f.dir_y)
    else:
        f.complete = 1
        f.cursor = 0
        f.count = 0
        # shows the message that a brigand is in trouble
    return 1
```

## Outputs

The brigand records and orders, the army, the attributes and two conversation variables: 0x2C (44)
is set to 1 after a fight with the Scottish raid and 0x61 (97) after one with the Welsh raid.

## Edge cases

The pass returns at the first fight or the first ended order, so later slots wait for the next pass.
A fight that leaves both sides with troops returns before the bounty. Losing marks the order fought
but keeps the brigands; the player's pools are only written back after a win. The Scottish bounty is
40, and the other two are 50 to 150. A brigand step never stops on water, and the float limits are
the largest finite floats, so only an infinite or undefined step is refused. A brigand whose route is
used up by a zero length stops moving but stays on the map until its order ends.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_000628AC` does with conversation variables, and what reads variables 44 and 97.
- What `fn_000106C0` does.
- What `fn_0001BF54` does.
- What `fn_00029D24` does.
- What `fn_00029DE0` does.
- What `fn_00029E58` does.
- What `fn_0003CED8` does.
- What `g_0009A928` holds.
