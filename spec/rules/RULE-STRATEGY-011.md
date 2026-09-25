---
id: RULE-STRATEGY-011
title: Encounter with a hostile force
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-001, FND-STRATEGY-001, FND-STRATEGY-002, FND-STRATEGY-003, FND-STRATEGY-015, FND-STRATEGY-019, FND-STRATEGY-021, FND-STRATEGY-022, FND-STRATEGY-023, FND-STRATEGY-024, FND-STRATEGY-026, FND-STRATEGY-033]
conflicting: []
split_with: []
related: [RULE-BATTLE-001, RULE-PERSON-001, RULE-RNG-001, RULE-STRATEGY-004, RULE-STRATEGY-009, RULE-STRATEGY-014]
---

## Summary

A player army with troops and no cooldown fights a hostile force it touches. Both sides above 60 set
their excess aside. A win removes the force, lowers its lord's rating by 3 and adds to fame,
intelligence and the battle count; a loss costs 30 to 70 percent of each pool and pulls the army back
halfway toward home. An army with no troops left is removed, and the game ends when the player rode
with it.

## When it runs

From the player pass when an army touches a hostile force (RULE-STRATEGY-010).

## Parameters

`i` the player army; `j` the hostile record.

## Inputs

The attributes FAME (6), INTELLIGENCE (4), HONOR (5), BATTLE_WON (24) and BATTLE_LOST (25) of row 0,
and the troop pools through `fn_00029E58`. `resolve_encounter` receives the six troop counts,
the player's pools 1, 0 and 2 and then the three hostile counts, with the morale value and 0.

## Procedure

```text
define fight_hostile(i, j):
    let f = player_forces[i]
    let h = hostile_forces[j]
    if map_session_over != 0:
        return
    if fn_00029E58(i, 0) + fn_00029E58(i, 1) + fn_00029E58(i, 2) == 0 or f.cooldown > 0:
        return
    if king_order_pending == 1 and king_order_done == 0 and h.origin == king_order_target:
        king_order_done = 1
    # attribute 6 is FAME
    set_attr(0, 6, attr(0, 6) + 1)
    focus_on(f.cell_row, f.cell_col)
    # saves the screen and shows the message that the army meets a hostile force
    fn_0003CED8()
    let e = stage_battle(i, j)
    # plays sound 0x14A for a win, and 0x14B for a loss unless the ridden army lost every troop; restores the screen
    if e == 1:
        # attribute 24 is BATTLE_WON, 4 INTELLIGENCE
        set_attr(0, 24, attr(0, 24) + 1)
        destroy_hostile(j)
        f.cooldown = 0
        set_attr(0, 4, attr(0, 4) + 1)
        persons[h.lord].rating = max(1, persons[h.lord].rating - 3)
    else:
        # attribute 25 is BATTLE_LOST
        set_attr(0, 25, attr(0, 25) + 1)
        if fn_00029E58(i, 0) > 0 or fn_00029E58(i, 1) > 0 or fn_00029E58(i, 2) > 0:
            let hx = anchor_x(home_row)
            let hy = anchor_y(home_col)
            let mx = (INT32(f.x) + hx) / 2
            let my = (INT32(f.y) + hy) / 2
            f.x = FLOAT32(mx)
            f.y = FLOAT32(my)
            place(f, mx, my)
            let kind = terrain_kind_at(f.cell_row, f.cell_col)
            if kind == 0x16 or kind == 9:
                f.x = FLOAT32(hx)
                f.y = FLOAT32(hy)
                f.cell_row = home_row
                f.cell_col = home_col
            f.count = 0
            f.cursor = 0
            route_drawing = 0
            f.complete = 1
            f.dest_x = INT32(f.x)
            f.dest_y = INT32(f.y)
            focus_on(f.cell_row, f.cell_col)
            f.cooldown = 0
            if random_inclusive(10) > 5:
                f.cooldown = 20
        else:
            f.cooldown = 1
            if i == ridden_force:
                # shows the death message
                fn_000106C0()
                fn_0001BF54(2)
                map_session_over = 1
            else:
                fn_00029D24(i)
    if g_0009AEFC != 0:
        fn_0005B3B0(g_0009AEFC, 0x87B, 0, 0x7FFF)
    fn_0001146C(h.cell_row, h.cell_col)

define stage_battle(i, j):
    let b = 0
    if i == ridden_force:
        b = 20
    # attribute 5 is HONOR
    let morale = (attr(0, 6) + attr(0, 5) + b) / 6
    let h = hostile_forces[j]
    let total = h.swordsmen + h.halberdiers + h.knights
    if total > 60:
        let r = (total - 60) / 3
        if r + 1 < h.swordsmen:
            h.swordsmen = h.swordsmen - r
        if r + 1 < h.halberdiers:
            h.halberdiers = h.halberdiers - r
        if r + 1 < h.knights:
            h.knights = h.knights - r
    let p = [fn_00029E58(i, 0), fn_00029E58(i, 1), fn_00029E58(i, 2)]
    let kept = [0, 0, 0]
    let ptotal = p[0] + p[1] + p[2]
    if ptotal > 60:
        let n = 0
        for k in 0..3:
            if p[k] >= 20:
                n = n + 1
        if n != 0:
            let r = (ptotal - 60) / n
            for k in 0..3:
                if r + 1 < p[k]:
                    p[k] = p[k] - r
                    kept[k] = r
    let counts = [p[1], p[0], p[2], h.swordsmen, h.halberdiers, h.knights]
    let e = resolve_encounter(counts, morale, 0)
    # puts the hostile counts first and the pools back in their own order
    counts = [counts[3], counts[4], counts[5], counts[1], counts[0], counts[2]]
    if e == 1:
        set_attr(0, 24, attr(0, 24) + 1)
    else:
        set_attr(0, 25, attr(0, 25) + 1)
    h.swordsmen = counts[0]
    h.halberdiers = counts[1]
    h.knights = counts[2]
    for k in 0..3:
        p[k] = counts[3 + k]
    if e == 0 and (p[0] != 0 or p[1] != 0 or p[2] != 0):
        let d = random_inclusive(40) + 30
        for each k in [0, 2, 1]:
            if p[k] == 1:
                p[k] = 0
            else:
                p[k] = p[k] - p[k] * d / 100
    for k in 0..3:
        p[k] = max(0, p[k]) + kept[k]
    fn_00029DE0(i, p[0], p[1], p[2])
    return e
```

## Outputs

The army's pools, the hostile record, the attributes, the king's order and the map session. A lost
army that still has troops stands at the halfway point, or at home when that point is on tile kind
`0x16` or 9, with its route cleared.

## Edge cases

Each result is counted twice in BATTLE_WON or BATTLE_LOST, once in each routine (BUG-STRATEGY-003).
The player's set-aside troops return whatever the result, and the hostile side's excess is lost. A
loss with a pool of exactly 1 empties that pool. The rating drop happens after the force is
destroyed and reads the record's lord, which destroying leaves in place. A battle against a force of
the property the king named carries out the order before the battle is fought, whatever its result.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_00029DE0`, `fn_000106C0`, `fn_0005B3B0`, `g_0009AEFC` and `fn_0001146C`
  do beyond what the findings record.
- What `fn_0001BF54` does.
- What `fn_00029D24` does.
- What `fn_00029E58` does.
- What `fn_0003CED8` does.
