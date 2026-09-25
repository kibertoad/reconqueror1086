---
id: RULE-BATTLE-004
title: Field battle rectangle tests
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-004, FND-BATTLE-008, FND-BATTLE-010, FND-BATTLE-011]
conflicting: []
split_with: []
related: [FMT-BATTLE-001]
---

## Summary

A point hits the first unit rectangle that contains it, left and top edges included. A unit looks for
a neighbour at the four corners of its rectangle, moved by an offset; a living unit found there counts
as a friend or a foe, and a dead unit found at one corner makes a miss at a later corner end the search.

## When it runs

Whenever another battle rule tests a point or looks for a neighbour.

## Parameters

`hit_test(x, y, rects, n)`: a point and the first `n` rectangles of `rects`, each a list of four
`INT16`, `[left, top, width, height]`. `probe_neighbour(i, dx, dy)`: a unit index and an offset.

## Inputs

`battle_units`, `battle_rects`, `battle_field_width` and `battle_field_height`.

## Procedure

```text
define hit_test(x, y, rects, n):
    for k in 0..n:
        let r = rects[k]
        if r[0] <= x and x < r[0] + r[2] and r[1] <= y and y < r[1] + r[3]:
            return k + 1
    return 0

define probe_neighbour(i, dx, dy):
    let u = battle_units[i]
    let r = battle_rects[i]
    battle_rects[i] = [battle_field_width + 1, battle_field_height + 1, 1, 1]
    let points = [[r[0] + dx, r[1] + dy], [r[0] + dx, r[1] + r[3] + dy], [r[0] + r[2] + dx, r[1] + dy], [r[0] + r[2] + dx, r[1] + r[3] + dy]]
    let seen = false
    let result = 0
    let index = -1
    for each p in points:
        let s = hit_test(p[0], p[1], battle_rects, count(battle_units))
        if s == 0:
            if seen:
                break
            continue
        index = s - 1
        let t = battle_units[index]
        if t.strength > 0:
            result = 2
            if t.lane == u.lane:
                result = 1
            break
        seen = true
    battle_rects[i] = r
    return [result, index]
```

## Outputs

`hit_test` returns the one-based index of the first rectangle that holds the point, or 0.
`probe_neighbour` returns a list of two values: 0 for nothing living found, 1 for a unit of the same
lane and 2 for a unit of the other lane, and the index of the last unit hit, or -1 for none. It leaves
`battle_rects` as it found it.

## Edge cases

A rectangle of width or height 0, as a dead unit has, holds no point. The probe moves the unit's own
rectangle outside the field while it runs, so the unit never finds itself. The corners at `left + width`
and `top + height` lie just outside the rectangle.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
