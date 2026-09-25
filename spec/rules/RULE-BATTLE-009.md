---
id: RULE-BATTLE-009
title: Field battle keys
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-001, FND-BATTLE-004, FND-BATTLE-007, FND-BATTLE-008, FND-BATTLE-012, FND-BATTLE-016, FND-STRATEGY-001, FND-STRATEGY-019]
conflicting: []
split_with: []
related: [FMT-BATTLE-001]
---

## Summary

H, K and S add every living player unit of the halberdiers, knights or swordsmen to the selection. P
pauses and resumes the battle once it has started. A and Shift+A put the selection or every living unit
on automatic. Alt+X and Alt+F4 leave the battle. Two more keys, which need three other bytes set, let
the player win or lose.

## When it runs

Once in each battle pass (RULE-BATTLE-003), after the unit updates.

## Parameters

None.

## Inputs

`battle_key_ready`, `battle_key`, `battle_units`, `battle_selection`, `battle_started`, `g_000B0C36`, `g_000B0C2A` and `g_000B0C38`.

## Procedure

```text
define key_dispatch():
    if not battle_key_ready:
        return 0
    let k = battle_key
    if k == 0x48 or k == 0x68:
        select_category(0)
    else if k == 0x4B or k == 0x6B:
        select_category(0xF0)
    else if k == 0x53 or k == 0x73:
        select_category(0x78)
    else if k == 0x50 or k == 0x70:
        if battle_started != 0:
            battle_paused = battle_paused ^ 1
    else if k == 0x12D or k == 0x16B:
        map_session_over = 1
        return 1
    else if k == 0x61:
        for each s in battle_selection:
            battle_units[s].control = 1
    else if k == 0x41:
        for each u in battle_units:
            if u.strength > 0:
                u.control = 1
    else if k == 0x57 or k == 0x17 or k == 0x111:
        if g_000B0C36 != 0 and g_000B0C2A != 0 and g_000B0C38 != 0:
            for each u in battle_units:
                if u.lane == 0x168 and u.strength > 0:
                    u.strength = 20
            battle_contact_filter = 1
    else if k == 0x4C or k == 0x0C or k == 0x126:
        if g_000B0C36 != 0 and g_000B0C2A != 0 and g_000B0C38 != 0:
            battle_contact_filter = -1
    return 0

define select_category(category):
    for i in 0..count(battle_units):
        let u = battle_units[i]
        let listed = false
        for each s in battle_selection:
            if s == i:
                listed = true
        if not listed and u.lane == 0 and u.category == category and u.strength > 0:
            append(battle_selection, i)
```

## Outputs

Returns 1 for Alt+X and Alt+F4 and 0 otherwise. Changes the selection, the control codes, the pause
flag, `battle_contact_filter`, the foe's strengths and `map_session_over`.

## Edge cases

Only the upper-case W and L and their Ctrl and Alt forms reach the two hidden switches. With the W
switch the foe drops to strength 20, one blow from dying, and the player's units cannot be hit; with the
L switch the foe cannot be hit. The keys that leave the battle also end the session of the strategic
map. The pause key does nothing before the first control has started the battle.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `g_000B0C36`, `g_000B0C2A` and `g_000B0C38` hold; they are likely states of shift keys.
- What the caller of the resolver does with `map_session_over` after Alt+X.
