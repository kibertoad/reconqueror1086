---
id: RULE-STRATEGY-016
title: Brigand orders, raids and orders from the king
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-002, FND-STRATEGY-015, FND-STRATEGY-016, FND-STRATEGY-021, FND-STRATEGY-032]
conflicting: []
split_with: []
related: [FMT-STRATEGY-001, FMT-STRATEGY-004, FMT-STRATEGY-006, RULE-RNG-001, RULE-STRATEGY-014]
---

## Summary

An order creates a brigand force in its slot: slot 0 is the yearly brigand order along one of its
origin's two brigand routes, and slots 1 and 2 are the fixed Scottish and Welsh raids from London. An
order from the king names a property to attack from London by a month of the next year.

## When it runs

`issue_order` from the strategic pass; the raids when a conversation has asked for them (RULE-STRATEGY-001).

## Parameters

`kind` 1 for a brigand order, 2 for an order from the king; `a` the month; `b` the year; `c` the origin; `d` the property to attack; `desc` a brigand order descriptor; `s` a slot.

## Inputs

`brigand_orders`, `brigand_forces`, `properties` and the route files (FMT-STRATEGY-004).

## Procedure

```text
define issue_order(kind, a, b, c, d):
    if kind == 1:
        let desc = new FMT-STRATEGY-006
        desc.unk_00 = 1
        desc.end_month = a
        desc.end_year = b
        desc.origin = c
        desc.slot = 0
        create_brigand(desc)
        return
    if kind == 2:
        if king_order_pending != 0:
            return 1
        if c == d:
            return 0
        king_order_kind = 2
        king_order_month = a
        king_order_year = b
        king_order_target = d
        king_order_from = c
        king_order_pending = 1
        king_order_done = 0
        # shows the order from the king, naming the lord of property d, month a and year b
        return
    return 1

define create_brigand(desc):
    let s = desc.slot
    let o = brigand_orders[s]
    if o.active == 1:
        return 0
    o.slot = s
    o.unk_00 = desc.unk_00
    o.end_month = desc.end_month
    o.end_year = desc.end_year
    o.origin = desc.origin
    let f = brigand_forces[s]
    if load_brigand_route(f, o.origin, s) != 1:
        return 0
    f.lord = properties[o.origin].lord
    f.mode = MOVE_ROUTED
    f.active = 1
    f.origin = o.origin
    f.halberdiers = random_inclusive(1)
    f.swordsmen = random_inclusive(1) + 1
    f.knights = 0
    o.active = 1
    o.fought = 0
    if s == 0:
        # shows the order from the player's lord, worded one way when o.end_year is 1086, with the month and year
        return 1
    return 1

define load_brigand_route(f, o, s):
    let name = "scot.rat"
    if s == 0:
        let letter = "a"
        if random_inclusive(2) == 0:
            letter = "b"
        name = sprintf("br_%d%s.rat", o + 1, letter)
    else if s == 2:
        name = "wales.rat"
    let file = resource(name)
    f.route = copy(file.points)
    f.cursor = 0
    f.reversed = 0
    f.complete = 0
    f.count = file.point_count
    f.x = FLOAT32(f.route[0])
    f.y = FLOAT32(f.route[1])
    place(f, f.route[0], f.route[1])
    return 1

define raid_scotland():
    let desc = new FMT-STRATEGY-006
    desc.unk_00 = 1
    desc.end_month = 1
    desc.end_year = 2000
    desc.origin = 7
    desc.slot = 1
    create_brigand(desc)

define raid_wales():
    let desc = new FMT-STRATEGY-006
    desc.unk_00 = 1
    desc.end_month = 1
    desc.end_year = 2000
    desc.origin = 7
    desc.slot = 2
    create_brigand(desc)
```

## Outputs

A new brigand force with 0 or 1 halberdiers and 1 or 2 swordsmen, its order, and the king's pending
order. The two orders are message boxes.

## Edge cases

A slot runs one order at a time, so a raid or yearly order issued while its slot is busy is lost.
The draw for the route letter gives `b` one time in three. The raids end in the year 2000, so in
practice they last until beaten. The order is copied into its slot before the route is loaded, so a
failed load leaves the order's date and origin written but inactive. The origin of a yearly brigand
order is `fallback_origin`, one of the seven homes' groups, for which the `br_` files exist.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `issue_order` returns after a brigand order and after showing the king's order.
