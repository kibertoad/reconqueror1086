---
id: RULE-STRATEGY-002
title: Hostile pass and arrival
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-001, FND-STRATEGY-003, FND-STRATEGY-015, FND-STRATEGY-019]
conflicting: []
split_with: []
related: [FMT-STRATEGY-001, RULE-STRATEGY-003, RULE-STRATEGY-004, RULE-STRATEGY-006, RULE-STRATEGY-007, RULE-STRATEGY-008, RULE-STRATEGY-014]
---

## Summary

The hostile pass first lets the generator start a force, then moves each live hostile force by its
mode. A force that arrives or stops looks for a person in five cells round it: a person with no
assignment is met, a person of the force's own property takes the troops back into the garrison,
and any other result sends the force on to a new target.

## When it runs

From the strategic pass when the camera is not being panned (RULE-STRATEGY-001).

## Parameters

None.

## Inputs

`hostile_forces`, `properties`, `persons` and `world_grid`.

## Procedure

```text
define hostile_pass():
    generate_hostile()
    for s in 0..5:
        let f = hostile_forces[s]
        if f.active == 1:
            let done = 0
            if f.mode == MOVE_DIRECT:
                done = step_direct(s)
            else if f.mode == MOVE_PURSUIT:
                done = step_pursuit(s)
            else:
                done = step_routed(f)
            if done != 0 and f.active == 1:
                place(f, INT32(f.x), INT32(f.y))
                let p = met_person(f.cell_row, f.cell_col)
                if p != 0:
                    let a = persons[p].assignment
                    if a == 0:
                        if persons[p].eligible == 1:
                            fn_00039900(s, p)
                        if map_session_over == 1:
                            return
                    if properties[f.origin].state == a:
                        let o = properties[f.origin]
                        o.garrison = UINT8(o.garrison + f.halberdiers + f.swordsmen + f.knights)
                        destroy_hostile(s)
                    else:
                        retarget(s)
                else:
                    retarget(s)
```

## Outputs

The records, garrisons and `hostile_count` the called rules change.

## Edge cases

A force whose mode is neither 1 nor 3 is stepped as a routed force. A handler that clears `active`
while it reports arrival skips the arrival. A met person with no assignment also passes the home
test when the origin's state is 0.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_00039900` does when a force meets a person.
