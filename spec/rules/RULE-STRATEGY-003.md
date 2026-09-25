---
id: RULE-STRATEGY-003
title: Hostile generator and reactive finder
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-003, FND-STRATEGY-004, FND-STRATEGY-005, FND-STRATEGY-006, FND-STRATEGY-013, FND-STRATEGY-015, FND-STRATEGY-019, FND-STRATEGY-023, FND-STRATEGY-024, FND-STRATEGY-026]
conflicting: []
split_with: []
related: [FMT-STRATEGY-001, RULE-RNG-001, RULE-STRATEGY-004, RULE-STRATEGY-014]
---

## Summary

Each hostile pass the generator first asks the finder whether a player force has alerted a property
with a garrison; if so, and it is not held back, that property sends a pursuit. Otherwise the timed
accumulator grows by the speed setting, and each time it reaches 5,000 the game tries, rarely, a
pursuit from an alerted property and then an ordinary movement: along the home route from the
fallback origin while the property list is empty, and otherwise from a random alerted property.

## When it runs

At the start of each hostile pass (RULE-STRATEGY-002).

## Parameters

None.

## Inputs

`player_forces`, `properties`, `hostile_forces`, `generator_property` and the counters.

## Procedure

```text
define generate_hostile():
    reactive_counter = reactive_counter + 1
    let s = find_reaction()
    if s >= 0:
        let p = generator_property
        let held = false
        if p == 7 and reactive_counter < 1000 and s != ridden_force:
            held = true
        if has_pursuer(s) == 1 and (s != ridden_force or reactive_counter <= 50):
            held = true
        if not held and pursue(s, p) == 1:
            reactive_counter = 0
            movement_accumulator = UINT32(movement_accumulator + strategic_speed)
            if random_inclusive(6) < 2 and hostile_count < 5:
                if construct_hostile(fallback_person, p, 2, 0) == 0:
                    construct_hostile(fallback_person, p, 1, 0)
                movement_accumulator = 0
            return
    if movement_accumulator < 5000:
        movement_accumulator = UINT32(movement_accumulator + strategic_speed)
        return
    movement_accumulator = 0
    if hostile_count >= 5:
        return
    if random_inclusive(100) > 96 and properties_present() == 1 and properties[generator_property].alerted != 0:
        for k in 0..5:
            if player_forces[k].active == 1:
                let q = pick_alerted_property()
                generator_property = q
                if q >= 0 and pursue(k, q) != 0:
                    return
    if properties_present() == 0:
        if construct_hostile(fallback_person, fallback_origin, 2, 0) == 0:
            construct_hostile(fallback_person, fallback_origin, 1, 0)
    else:
        let q = pick_alerted_property()
        generator_property = q
        if q >= 0:
            if construct_hostile(fallback_person, q, 2, 1) == 0:
                construct_hostile(fallback_person, q, 1, 1)

define properties_present():
    if property_list_head != 0xFF:
        return 1
    return 0

define pick_alerted_property():
    let found = []
    for i in 0..14:
        if properties[i].state != 0 and properties[i].alerted == 1:
            append(found, i)
    if count(found) == 0:
        return -1
    return found[random_inclusive(count(found) - 1)]

define has_pursuer(s):
    for j in 0..5:
        let h = hostile_forces[j]
        if h.active == 1 and h.target == s and h.mode == MOVE_PURSUIT:
            return 1
    return 0

define find_reaction():
    for p in 0..14:
        let o = properties[p]
        if o.state != 0:
            for i in 0..5:
                let f = player_forces[i]
                if f.active == 1:
                    let v = cell_owner(f.cell_row, f.cell_col)
                    let x = INT32(f.x)
                    let y = INT32(f.y)
                    let dx = abs(x - o.map_x)
                    let dy = abs(y - o.map_y)
                    let close = 30
                    let reach = 250
                    let in_london = false
                    if p == 7:
                        close = 40
                        reach = 200
                        in_london = x >= 700 and x < 12640 and y >= 600 and y < 4560
                    if (dx < close and dy < close) or o.lord == v:
                        o.alerted = 1
                        if o.garrison > 0:
                            generator_property = p
                            return i
                        break
                    else if ((dx < reach and dy < reach) or in_london) and o.approached == 0:
                        focus_on(f.cell_row, f.cell_col)
                        # shows the warning that the force approaches a castle armed
                        f.complete = 1
                        f.target = 0
                        f.count = 0
                        route_drawing = 0
                        o.approached = 1
    return -1
```

## Outputs

`find_reaction` returns the index of the player record that set off a pursuit, or -1, and leaves the
property in `generator_property`. The approach warning is a message box, shown once per property.

## Edge cases

`movement_accumulator` is compared as an unsigned value. After a pursuit the accumulator is cleared
only when the draw below 2 was made with fewer than five live forces, so it can keep its value. The
rare branch reads `properties[generator_property]` with whatever the local held from an earlier
write, including -1 (BUG-STRATEGY-001). A property with no garrison is still alerted, and the finder
then goes on with the next property. London's rectangle holds from `(700, 600)` up to but not
including `(12640, 4560)`. The lord test compares the lord's index with the person bits of the
player force's cell without the range check `cell_person` makes.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The value `generator_property` holds before its first write in a session.
