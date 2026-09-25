---
id: RULE-BATTLE-005
title: Field battle contact
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-004, FND-BATTLE-007, FND-BATTLE-008, FND-BATTLE-012, FND-STRATEGY-021]
conflicting: []
split_with: []
related: [RULE-RNG-001, FMT-BATTLE-001]
---

## Summary

A unit in contact strikes its target for a draw below 10 when its category beats the target's and
below 7 otherwise. A foe target also loses the remainder of the player's morale by 3. A target left
below 20 starts to die; once the target is at 0 or below, the striker looks for new work.

## When it runs

From `update_unit` (RULE-BATTLE-003) when a unit in state `0x28` reaches phase 2.

## Parameters

`apply_contact(i)`: the index of the striking unit.

## Inputs

`battle_units`, `battle_contact_filter`, `battle_morale` and `g_000A9C80`.

## Procedure

```text
define apply_contact(i):
    let u = battle_units[i]
    let t = battle_units[u.target]
    let skip = false
    if battle_contact_filter == 1 and t.lane == 0:
        skip = true
    if battle_contact_filter == -1 and t.lane == 0x168:
        skip = true
    if not skip:
        let die = 7
        if u.category == 0xF0 and t.category == 0x78:
            die = 10
        else if u.category == 0 and t.category == 0xF0:
            die = 10
        else if u.category == 0x78 and t.category == 0:
            die = 10
        let damage = draw() % die
        if t.lane == 0x168:
            damage = damage + battle_morale % 3
        t.strength = t.strength - damage
    let sound = draw() % 5
    fn_0005B3B0(g_000A9C80, [4, 0xF8C, 0x1E6C, 0x3B8C, 0x5C85][sound], 0, 0x7FFF)
    if t.strength < 20 and t.strength > 0 and t.state != 0x50:
        t.state = 0x50
        t.phase = 0
    if t.strength <= 0:
        u.target = -1
        u.state = 0
        if u.control == 1:
            u.dest_y = -1
            u.dest_x = -1
```

## Outputs

No return value. Lowers the target's strength without a floor, can start its death, and can return the
striker to state 0. Makes two draws, or one when the contact is skipped.

## Edge cases

Halberdiers (category 0) beat knights (`0xF0`), knights beat swordsmen (`0x78`), and swordsmen beat
halberdiers. The morale bonus is the remainder by 3, so it is 0 to 2 and falls to 0 at every multiple
of 3 (BUG-BATTLE-001). The foe's own morale value plays no part. A striker keeps hitting a dying target
until its strength reaches 0 or below. A unit at 20 or more loses at most 11 in one strike, so it
always passes through the death state before its strength runs out.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the three bytes that allow `battle_contact_filter` to change hold (RULE-BATTLE-009).
- What `fn_0005B3B0` does beyond what the findings record.
- What `g_000A9C80` does beyond what the findings record.
