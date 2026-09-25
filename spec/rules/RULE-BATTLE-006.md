---
id: RULE-BATTLE-006
title: Field battle turning
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-004, FND-BATTLE-008, FND-BATTLE-013]
conflicting: []
split_with: []
related: [FMT-BATTLE-001]
---

## Summary

A unit faces one of eight headings. Before it fights a target or walks to a destination it turns one
heading per update toward it, and a unit that already faces its target starts to fight.

## When it runs

From the idle update (RULE-BATTLE-007).

## Parameters

`octant(sx, sy, tx, ty)`: a source point and a target point, where -1 in `tx` or `ty` stands for an
unset coordinate. `turn_toward(u, o)`: a unit and a heading. `face_target(i, j)`: two unit indices.

## Inputs

`battle_units`.

## Procedure

```text
define octant(sx, sy, tx, ty):
    if sx == tx or tx == -1:
        if sy < ty:
            return 1
        return 5
    if sy == ty or ty == -1:
        if sx < tx:
            return 3
        return 7
    if sy > ty:
        if sx < tx:
            return 4
        return 6
    if sx < tx:
        return 2
    return 0

define turn_toward(u, o):
    u.phase = 0
    let up = abs(o - (u.heading + 1)) % 8
    let down = abs(o - (u.heading - 1)) % 8
    if up < down:
        u.heading = (u.heading + 1) % 8
    else:
        u.heading = u.heading - 1
        if u.heading < 0:
            u.heading = 7

define face_target(i, j):
    let u = battle_units[i]
    let t = battle_units[j]
    let o = octant(u.x, u.y, t.x, t.y)
    if o == u.heading:
        u.phase = 0
        u.state = 0x28
    else:
        turn_toward(u, o)
```

## Outputs

`octant` returns a heading from 0 to 7: with y growing down the screen, 1 is down, 3 right, 5 up, 7
left, and 0, 2, 4 and 6 the diagonals between them. `turn_toward` and `face_target` change the unit's
heading, phase and state.

## Edge cases

The turn compares plain differences, so it does not always take the shorter way: from heading 1 toward
6 it turns up through 2 to 5. `face_target` does not check that the target lives.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
