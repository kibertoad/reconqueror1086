---
id: RULE-STRATEGY-005
title: Hostile force size
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-003, FND-STRATEGY-008, FND-STRATEGY-015]
conflicting: []
split_with: []
related: []
---

## Summary

An ordinary force has, in each troop type, the lord's household plus a twelfth of the lord's
rating, with one knight when that comes to nothing. A pursuit takes up to the hunted player force's
size plus three from the garrison, with support from the household.

## When it runs

When a hostile force is built (RULE-STRATEGY-004).

## Parameters

`s` the hostile record; `t` the hunted player record.

## Inputs

`persons`, `properties` and the player pools through `fn_00029E58`.

## Procedure

```text
define household_of(o):
    if o == 7:
        return 176 - person_list_length()
    return household_count(o)

define size_force(s):
    let f = hostile_forces[s]
    let q = persons[properties[f.origin].lord].rating / 4
    let n = household_of(f.origin) + q / 3
    f.swordsmen = n
    f.halberdiers = n
    f.knights = n
    if 3 * n < 1:
        f.knights = 1

define size_pursuit(s, t):
    let f = hostile_forces[s]
    let o = f.origin
    let g = properties[o].garrison
    let n = fn_00029E58(t, 0) + fn_00029E58(t, 1) + fn_00029E58(t, 2) + 3
    let d = min(g, n)
    let support = 1
    if d != 0:
        support = min(household_of(o), 30)
    let c = d + support
    if c <= 3:
        f.swordsmen = 3
    else:
        f.swordsmen = c / 3
        f.halberdiers = c / 3
        f.knights = c / 3
    properties[o].garrison = UINT8(g - d)

define household_count(g):
    let n = 0
    for p in 1..176:
        if persons[p].eligible == 1 and persons[p].group == g and persons[p].assignment != 0:
            n = n + 1
    return n

define person_list_length():
    let n = 0
    let p = person_list_head
    while p != 0xFF:
        n = n + 1
        p = persons[p].next
    return n
```

## Outputs

The three troop counts of the record, and the origin's garrison for a pursuit.

## Edge cases

In the small pursuit case only `swordsmen` is written, so `halberdiers` and `knights` keep what the
record held before (BUG-STRATEGY-002). London's household is every person not on the person list.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_00029E58` returns beyond a player army's pool of one troop type.
