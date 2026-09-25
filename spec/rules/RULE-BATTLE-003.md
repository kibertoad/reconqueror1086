---
id: RULE-BATTLE-003
title: Field battle pass
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-001, FND-BATTLE-004, FND-BATTLE-007, FND-BATTLE-008, FND-BATTLE-009, FND-BATTLE-012, FND-BATTLE-017, FND-BATTLE-018]
conflicting: []
split_with: []
related: [RULE-BATTLE-004, RULE-BATTLE-005, RULE-BATTLE-007, RULE-BATTLE-008, RULE-BATTLE-009, RULE-BATTLE-010, FMT-BATTLE-001]
---

## Summary

Each pass of the interactive battle rebuilds the unit rectangles, handles one pointer event, and,
when the battle is running and at least 200 units of the battle clock have passed, updates every unit
once. It then handles one key and reports whether the battle is over. A unit dies over five updates
after its strength falls below 20; a unit in contact with a foe strikes on every fifth update.

## When it runs

Repeatedly from `resolve_encounter` (RULE-BATTLE-001) until it returns a nonzero result.

## Parameters

None.

## Inputs

`battle_units`, `battle_paused`, `battle_last_pass`, `battle_clock_count`, `battle_player_alive`, `battle_foe_alive` and `screen_height`.

## Procedure

```text
define battle_pass():
    for i in 0..count(battle_units):
        let u = battle_units[i]
        if u.strength <= 0:
            battle_rects[i] = [0, 0, 0, 0]
        else:
            battle_rects[i] = [u.x - 15, u.y - 20, 25, 30]
    if pointer_dispatch() != 0:
        return 1
    if battle_paused == 0 and UINT32(battle_last_pass + 200) < battle_clock():
        battle_last_pass = battle_clock()
        battle_redraw = 1
        for i in 0..count(battle_units):
            update_unit(i)
        # shows battle_player_alive in the panel at (battle_control_margin + 40, screen_height - 25)
    if key_dispatch() == 1:
        return 1
    if battle_foe_alive == 0:
        return 2
    if battle_player_alive == 0:
        return 1
    return 0

define update_unit(i):
    let u = battle_units[i]
    if u.state == 0x50 and u.phase != 4:
        u.phase = (u.phase + 1) % 5
        if u.phase == 4:
            if u.lane == 0:
                battle_player_alive = battle_player_alive - 1
            else:
                battle_foe_alive = battle_foe_alive - 1
            u.strength = 0
            if u.category == 0xF0:
                u.category = 0x78
    if u.state == 0x28:
        let p = probe_neighbour(i, 0, 0)
        if p[0] == 0 or p[0] == 1:
            u.state = 0
            u.target = -1
            if u.control == 1:
                u.dest_y = -1
                u.dest_x = -1
        else:
            u.target = p[1]
            u.phase = (u.phase + 1) % 5
            if u.phase == 2:
                apply_contact(i)
    if u.state == 0:
        update_idle(i)
```

## Outputs

Returns 0 while the battle goes on, 2 when the foe has no units left, and 1 when the player has none
left, has retreated, or has left with a key. Changes the units, the lane totals and the timing globals.

## Edge cases

The pass uses the rectangles it built at its start for every probe, so a unit that moved earlier in the
pass is still found where it stood. The clock is read twice, and the second reading becomes the new
start, so a late pass is never made up. A unit whose death completes still has its rectangle in this
pass. The foe total is tested first, so a battle where both sides die in the same pass is a win. A
dying unit can still be struck, but it neither strikes nor moves.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `battle_last_pass` holds when the first battle of a session starts.
