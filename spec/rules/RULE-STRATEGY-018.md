---
id: RULE-STRATEGY-018
title: Spies
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-STRATEGY-003, FND-STRATEGY-015, FND-STRATEGY-035, FND-STRATEGY-038]
conflicting: []
split_with: []
related: [RULE-PERSON-001]
---

## Summary

A spy costs 80 from the war-planning wealth. Only one spy is out at a time. It stays out until a
hostile force exists, then reports every live hostile force of a property that still stands, in slot
order, and comes back.

## When it runs

`buy_spy` from the war-planning screen; `spy_report` from the strategic pass (RULE-STRATEGY-001).

## Parameters

None.

## Inputs

`planning_wealth`, `player_confirms_spy` and `hostile_forces`.

## Procedure

```text
define buy_spy():
    # plays a sound
    if planning_wealth < 80:
        # shows the refusal
        return
    if player_confirms_spy:
        planning_wealth = planning_wealth - 80
        spies_pending = spies_pending + 1
        # attribute 17 is WEALTH
        set_attr(0, 17, planning_wealth)
        send_spies()

define send_spies():
    for k in 0..spies_pending:
        dispatch_spy()

define dispatch_spy():
    if spy_out == 1:
        return 0
    spy_out = 1
    return 1

define spy_report():
    if spy_out != 1:
        return
    let any = false
    for s in 0..5:
        let f = hostile_forces[s]
        if f.active != 0:
            any = true
            if properties[f.origin].state != 0:
                # shows the lord's name and the force's swordsmen count three times, labelled swordsmen, knights and halberdiers (BUG-STRATEGY-005)
                continue
    if any:
        spy_out = 0
```

## Outputs

The wealth, the spy flag and one report per live force. The war-planning screen sets `spies_pending`
to 0 when it opens.

## Edge cases

A second spy bought while one is out costs 80 and does nothing. Because `send_spies` runs for every
spy bought since the screen opened, the count only matters for how often `dispatch_spy` is called.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
