---
id: RULE-BATTLE-001
title: Field battle resolution
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-001, FND-BATTLE-002, FND-BATTLE-003, FND-BATTLE-004, FND-BATTLE-005, FND-BATTLE-008, FND-BATTLE-012, FND-BATTLE-015, FND-BATTLE-016, FND-BATTLE-017, FND-BATTLE-018, FND-STRATEGY-029, FND-CONFIG-004]
conflicting: []
split_with: []
related: [RULE-BATTLE-002, RULE-BATTLE-003, RULE-BATTLE-008, RULE-BATTLE-011, RULE-RNG-001, RULE-STRATEGY-011, RULE-STRATEGY-017, FMT-BATTLE-001, RULE-CONFIG-004]
---

## Summary

A field battle between a player army and a hostile force takes six troop counts: the player's three
categories, then the foe's. The player picks one of four formations for an interactive battle or lets
the battle resolve itself. The automatic battle compares the totals, each raised by the remainder of
its morale value by 3. The interactive battle runs until one side has no units left or the player
retreats, then writes the surviving units back into the six counts. The result is 1 for a win and 0
otherwise.

## When it runs

When the encounter with a hostile force (RULE-STRATEGY-011) or with brigands (RULE-STRATEGY-017) calls `resolve_encounter`. Two more callers exist whose purpose is not known.

## Parameters

`resolve_encounter(counts, morale, foe_morale)`: `counts` is a list of six `INT32` that the caller
keeps, the player's counts of categories 0, `0x78` and `0xF0` and then the foe's in the same order.
`morale` and `foe_morale` are `INT32`.

## Inputs

`battle_choice_region`, `player_ends_survey` and `screen_height`.

## Procedure

```text
define resolve_encounter(counts, morale, foe_morale):
    battle_paused = 1
    battle_started = 0
    battle_scroll_y = 0
    battle_scroll_x = 0
    battle_redraw = 1
    battle_mode_width = 0
    battle_control_margin = 0
    battle_contact_filter = 0
    battle_morale = morale
    battle_foe_morale = foe_morale
    battle_player_alive = counts[0] + counts[1] + counts[2]
    battle_foe_alive = counts[3] + counts[4] + counts[5]
    # shows the choice of five regions and reads the one the player picks
    let region = battle_choice_region
    if region < 1 or region > 4:
        return auto_resolve(counts)
    battle_formation = [2, 3, 1, 0][region - 1]
    battle_selection = []
    # loads BATTLE.PCX, switches the display mode, and loads men8_image
    battle_mode_width = select_battle_display()
    build_units(counts)
    if battle_mode_width == 1024:
        battle_control_margin = 160
    else if battle_mode_width == 800:
        battle_control_margin = 80
    let m = battle_control_margin
    battle_control_rects = [[m + 400, screen_height - 34, 84, 31], [m + 524, screen_height - 34, 41, 31], [m + 566, screen_height - 34, 41, 31]]
    let r = battle_control_rects[0]
    blit_frame(men8_image, 722, r[0] + 20, r[1] + 6)
    let result = 0
    while result == 0:
        result = battle_pass()
        if battle_redraw == 1:
            # draws the visible part of BATTLE.PCX, then the units
            draw_units()
            battle_redraw = 0
    # shows "You Have WON!" for result 2, "You Have RETREATED!" when
    # battle_player_alive is above 0, and "You Have LOST!" otherwise, in a dialog
    while not player_ends_survey:
        edge_scroll()
        if battle_redraw == 1:
            draw_units()
            battle_redraw = 0
    count_survivors(counts)
    # restores the display mode
    return result - 1

define auto_resolve(counts):
    if battle_player_alive + battle_morale % 3 <= battle_foe_alive + battle_foe_morale % 3:
        counts[0] = 0
        counts[1] = 0
        counts[2] = 0
        return 0
    let t = battle_foe_alive
    for k in 0..3:
        let r = draw() % t
        counts[k] = max(0, counts[k] - r)
        t = t - r
    return 1

define count_survivors(counts):
    for k in 0..6:
        counts[k] = 0
    for each u in battle_units:
        if u.strength > 0:
            let side = 0
            if u.lane != 0:
                side = 3
            if u.category == 0:
                counts[side] = counts[side] + 1
            else if u.category == 0x78:
                counts[side + 1] = counts[side + 1] + 1
            else if u.category == 0xF0:
                counts[side + 2] = counts[side + 2] + 1
```

## Outputs

Returns 1 when the player wins and 0 otherwise, and changes `counts`. After the automatic battle a win
lowers each player count by up to the foe's remaining total and leaves the foe's counts alone, and a
loss empties the player's counts. After the interactive battle the counts are the living units of each
side and category. The globals of the battle keep their last values.

## Edge cases

The interactive result is 1 when the foe has no units left, including when both sides are empty, and 0
for a retreat or a loss. An automatic battle with a foe total of 0 that the player wins divides by
zero in its first draw. The automatic battle adds the remainder of each morale value by 3, so a morale
of 3 adds nothing (BUG-BATTLE-001). The survivor count reads the category a unit has
when the battle ends, so a knight that died is not counted at all.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the gate `0x000256FC` returns when the player leaves it without picking a region, and how it
  reads the pointer.
- What the callers at `0x0002A80B` and `0x00042363` are.
- How the resolver draws the backdrop in each display mode.
- What an automatic battle with a foe total of 0 does on the original machine.
