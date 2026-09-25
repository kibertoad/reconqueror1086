---
id: RULE-BATTLE-002
title: Field battle units and formations
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-004, FND-BATTLE-005, FND-BATTLE-006, FND-BATTLE-008, FND-BATTLE-018]
conflicting: []
split_with: []
related: [RULE-RNG-001, FMT-BATTLE-001]
---

## Summary

Each troop counted becomes one unit of strength 100: the player's first, in category order, then the
foe's. The player's units are placed by the formation the player picked. The foe's units always stand
in columns from the right edge of the field, in one of two orders picked by a draw.

## When it runs

Once, from `resolve_encounter` (RULE-BATTLE-001) before an interactive battle starts.

## Parameters

`build_units(counts)`: the six counts `resolve_encounter` received.

## Inputs

`battle_formation`, `battle_field_width` and `battle_field_height`.

## Procedure

```text
define build_units(counts):
    battle_units = []
    for side in 0..2:
        for k in 0..3:
            for c in 0..counts[3 * side + k]:
                let u = new FMT-BATTLE-001
                u.category = [0, 0x78, 0xF0][k]
                u.value = [10, 20, 40][k]
                u.lane = [0, 0x168][side]
                u.heading = [3, 7][side]
                u.control = side
                u.strength = 100
                u.dest_x = -1
                u.dest_y = -1
                u.phase = 0
                u.state = 0
                u.target = -1
                append(battle_units, u)
    let n = counts[0] + counts[1] + counts[2]
    let m = counts[3] + counts[4] + counts[5]
    let rows = battle_field_height / 60
    if battle_formation == 0:
        for i in 0..n:
            battle_units[i].x = 60 * (i / rows) + 60
            battle_units[i].y = 60 * (i % rows) + 30
    else if battle_formation == 1:
        for i in 0..n:
            battle_units[i].x = 60 * (n / rows) + 60 - 60 * (i / rows)
            battle_units[i].y = 60 * (i % rows) + 30
    else if battle_formation == 2:
        let k = 0
        while k * (k + 1) / 2 < n:
            k = k + 1
        let r = k
        if k * (k + 1) / 2 != n:
            r = k + 1
        let x = 45 * r
        let row_y = battle_field_height / 2
        let y = row_y
        let length = 1
        let placed = 0
        for i in 0..n:
            battle_units[i].x = x
            battle_units[i].y = y
            y = y + 60
            placed = placed + 1
            if placed == length:
                placed = 0
                length = length + 1
                x = x - 45
                row_y = row_y - 30
                y = row_y
    else if battle_formation == 3:
        let c0 = counts[0]
        let h = c0 / 2
        let b = 60 * (h / 4 + 1) + 60 * ((n - c0) / rows)
        for i in 0..h:
            battle_units[i].x = b - 60 * (i / 4) + 120
            battle_units[i].y = 60 * (i % 4) + 30
        for j in 0..c0 - h:
            battle_units[h + j].x = b - 60 * (j / 4) + 120
            battle_units[h + j].y = 60 * (j % 4) + 480
        for j in 0..n - c0:
            battle_units[c0 + j].x = b - 60 * (j / rows)
            battle_units[c0 + j].y = 60 * (j % rows) + 30
    let side = draw() % 2
    for i in 0..m:
        let u = battle_units[n + i]
        if side == 0:
            u.x = battle_field_width - (60 * (i / rows) + 60)
        else:
            u.x = battle_field_width - (60 * (m / rows) + 60) + 60 * (i / rows)
        u.y = 60 * (i % rows) + 30
```

## Outputs

No return value. Sets `battle_units`, one record per troop, and makes one draw.

## Edge cases

The wedge of formation 2 starts one column further right than it needs to when the player's unit count
is not a triangular number: 2 units give `r = 3`. With no player units, `r` is 0. A formation code
above 3 places no player unit; it cannot come from the choice gate. The foe's placement ignores the
formation; the table the draw indexes has a mirrored wedge and an empty entry that a draw from 0 to
32767 never reaches.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `x` and `y` hold for the player's units before a formation writes them, for a code above 3.
- What the four formations look like on the choice screen.
