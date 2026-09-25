---
id: RULE-STRATEGY-012
title: New game, joining armies and field placement
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-001, FND-STRATEGY-002, FND-STRATEGY-003, FND-STRATEGY-004, FND-STRATEGY-006, FND-STRATEGY-007, FND-STRATEGY-013, FND-STRATEGY-015, FND-STRATEGY-016, FND-STRATEGY-019, FND-STRATEGY-021, FND-STRATEGY-023, FND-STRATEGY-024, FND-STRATEGY-025, FND-STRATEGY-026, FND-STRATEGY-029, FND-STRATEGY-030, FND-STRATEGY-031, FND-STRATEGY-032, FND-STRATEGY-035, FND-TOURNEY-001, FND-TOURNEY-003]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-STRATEGY-007, RULE-STRATEGY-014]
---

## Summary

A new game places the player at one of seven homes, drawn at random, and clears every strategic
record. The player can place up to five armies round the home, join one to ride with it, leave it
again, and remove it.

## When it runs

`new_strategic_game` when a campaign starts; `reset_strategic` from the reset routine; the others from the army commands of the map.

## Parameters

`i` a player record.

## Inputs

`start_persons`, `persons` and `home_offsets`.

## Procedure

```text
define new_strategic_game():
    field_army_count = 0
    planting_notice = 1
    start_home = random_inclusive(6)
    let s = start_persons[start_home]
    home_row = persons[s].cell_row
    home_col = persons[s].cell_col
    persons[s].assignment = 0
    fallback_person = s
    fallback_origin = persons[s].group
    persons[s].rating = 12
    for i in 0..6:
        let f = player_forces[i]
        f.selected = 0
        f.unk_08 = 0
        f.cooldown = 0
        f.target = 0
        f.active = 0
        f.complete = 1
    ridden_force = 5
    selected_force = 5
    let a = player_forces[5]
    a.active = 1
    a.selected = 1
    a.complete = 1
    a.cursor = 0
    a.count = 0
    a.target = 0
    a.dest_x = anchor_x(home_row)
    a.dest_y = anchor_y(home_col)
    a.x = FLOAT32(a.dest_x)
    a.y = FLOAT32(a.dest_y)
    a.cell_row = home_row
    a.cell_col = home_col
    for s2 in 0..5:
        hostile_forces[s2].active = 0
        hostile_forces[s2].route = 0
    for d in 0..3:
        brigand_forces[d].active = 0
        brigand_forces[d].count = 0
        brigand_forces[d].complete = 0
        brigand_forces[d].route = 0
        brigand_orders[d].fought = 0
        brigand_orders[d].active = 0
    king_order_done = 0
    king_order_pending = 0
    next_brigand_year = 1086
    next_king_order_year = 1086
    movement_accumulator = 5000

define reset_strategic():
    field_army_count = 0
    selected_force = 0
    spy_out = 0
    route_drawing = 0
    hostile_count = 0
    jousts_today = 0
    melees_today = 0
    tournament_wins = 0
    for s in 0..5:
        hostile_forces[s].active = 0
        free(hostile_forces[s].route)
    for d in 0..3:
        free(brigand_forces[d].route)
    next_brigand_year = 1086
    next_king_order_year = 1086
    movement_accumulator = 5000
    route_marker_image = 0
    brigand_image = 0
    strategic_speed = 1
    ridden_force = 5

define join_army(i):
    ridden_force = i
    player_forces[i].active = 1
    player_forces[selected_force].selected = 0
    selected_force = i
    player_forces[i].selected = 1
    player_forces[5].selected = 0
    player_forces[5].active = 0

define leave_army(i):
    let a = player_forces[5]
    if a.active == 1:
        return 0
    ridden_force = 5
    player_forces[i].active = 1
    player_forces[i].selected = 0
    player_forces[selected_force].selected = 0
    a.x = player_forces[i].x
    a.y = player_forces[i].y
    a.cell_row = player_forces[i].cell_row
    a.cell_col = player_forces[i].cell_col
    route_drawing = 0
    selected_force = 5
    a.target = 0
    a.count = 0
    a.cursor = 0
    a.complete = 1
    a.selected = 1
    a.active = 1
    return 1

define army_away(i):
    let f = player_forces[i]
    let dx = INT32(f.x) - anchor_x(home_row)
    let dy = INT32(f.y) - anchor_y(home_col)
    if f.active == 1 and isqrt(dx * dx + dy * dy) >= 150:
        return 1
    return 0

define rides_with(i):
    if ridden_force != 5 and ridden_force == i:
        return 1
    return 0

define place_army(i):
    if field_army_count > 5:
        return 0
    field_army_count = field_army_count + 1
    let f = player_forces[i]
    f.dest_x = anchor_x(home_row) + home_offsets[i][0]
    f.dest_y = anchor_y(home_col) + home_offsets[i][1]
    f.x = FLOAT32(f.dest_x)
    f.y = FLOAT32(f.dest_y)
    f.unk_08 = 0
    f.cooldown = 0
    f.cell_row = home_row
    f.cell_col = home_col
    f.active = 1
    f.complete = 1
    let g = player_forces[selected_force]
    g.count = 0
    g.cursor = 0
    g.target = 0
    g.selected = 0
    selected_force = i
    f.selected = 1
    return 1

define remove_army(i):
    if field_army_count <= 0:
        return 0
    let f = player_forces[i]
    let chosen = selected_force
    if i == selected_force:
        for k in 0..6:
            if k != i and player_forces[k].active == 1:
                chosen = k
                break
    if i == ridden_force:
        ridden_force = 5
        player_forces[chosen].selected = 0
        let a = player_forces[5]
        a.selected = 1
        a.active = 1
        a.x = f.x
        a.y = f.y
        a.cell_row = f.cell_row
        a.cell_col = f.cell_col
        a.complete = 1
        chosen = 5
    f.cooldown = 0
    f.active = 0
    f.selected = 0
    f.unk_08 = 0
    f.complete = 1
    field_army_count = field_army_count - 1
    selected_force = chosen
    return 1
```

## Outputs

The records and globals named. `new_strategic_game` also sets `place_person`, and the home person
keeps its cell.

## Edge cases

`place_army` lets six placements succeed, one for each record including the figure's, although the
figure is always present. A placed army keeps whatever route and target fields the record held, and
it is the previously selected record whose route is cleared. The reset routine leaves `selected_force`
at 0 while `ridden_force` is 5.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What value `new_strategic_game` stores in `place_person`.
- Whether the reset routine clears the freed route pointers.
