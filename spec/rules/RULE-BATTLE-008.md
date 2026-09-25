---
id: RULE-BATTLE-008
title: Field battle pointer handling
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-001, FND-BATTLE-004, FND-BATTLE-008, FND-BATTLE-010, FND-BATTLE-012, FND-BATTLE-015, FND-BATTLE-018, FND-BATTLE-019, FND-BATTLE-020, FND-JOUST-002, FND-STRATEGY-021, FND-STRATEGY-029]
conflicting: []
split_with: []
related: [RULE-BATTLE-004, RULE-BATTLE-012, FMT-BATTLE-001]
---

## Summary

The field scrolls by 10 pixels when the pointer is within 5 pixels of an edge. Hovering over a unit
shows its strength or marks it as a foe, and leaving it shows whether the player is winning. A click
selects one of the player's living units, a click on the control strip starts the battle, asks to
retreat, or puts units on automatic, and a secondary click sends the selected units to a point.

## When it runs

Once at the start of each battle pass (RULE-BATTLE-003). `edge_scroll` also runs in each pass of the survey after the battle (RULE-BATTLE-001).

## Parameters

None.

## Inputs

`pointer_x`, `pointer_y`, `screen_width`, `screen_height`, `battle_units`, `battle_rects`,
`battle_control_rects`, `battle_selection`, `battle_started`, `battle_status_shown`,
`player_confirms_retreat` and `g_000A9C80`.

## Procedure

```text
define edge_scroll():
    if pointer_x <= 5 and battle_scroll_x > 0:
        battle_scroll_x = battle_scroll_x - 10
        battle_redraw = 1
    if pointer_x >= screen_width - 5 and battle_field_width - screen_width - 10 > battle_scroll_x:
        battle_scroll_x = battle_scroll_x + 10
        battle_redraw = 1
    if pointer_y <= 5 and battle_scroll_y > 0:
        battle_scroll_y = battle_scroll_y - 10
        battle_redraw = 1
    if pointer_y >= screen_height - 5 and battle_field_height - screen_height - 50 > battle_scroll_y:
        battle_scroll_y = battle_scroll_y + 10
        battle_redraw = 1

define pointer_dispatch():
    edge_scroll()
    let h = hit_test(pointer_x + battle_scroll_x, pointer_y + battle_scroll_y, battle_rects, count(battle_units))
    if h >= 1:
        battle_status_shown = 1
        # shows "OUR" with the unit's strength as a percentage for lane 0 and "FOE" for lane 0x168,
        # in the 83 by 20 panel at (battle_control_margin + 160, screen_height - 25)
    else if battle_status_shown != 0:
        # shows "WINNING" when battle_foe_alive <= battle_player_alive and "LOSING" otherwise
        battle_status_shown = 0
    let e = next_pointer_code()
    let code = e[0]
    let x = e[1]
    let y = e[2]
    if code == 2:
        return select_at(x, y)
    if code == 3:
        let c = hit_test(x, y, battle_control_rects, 3)
        if c == 1:
            if battle_started == 0:
                battle_paused = 0
                battle_started = 1
                let r = battle_control_rects[0]
                blit_frame(men8_image, 721, r[0] + 15, r[1] + 6)
            else if player_confirms_retreat:
                return 1
        else if c == 2:
            for each k in battle_selection:
                battle_units[k].control = 1
        else if c == 3:
            for each u in battle_units:
                if u.strength > 0:
                    u.control = 1
        return select_at(x, y)
    if code == 6 or code == 7:
        if count(battle_selection) == 0:
            return 0
        if y < 45:
            y = 45
        if y > screen_height - 85:
            y = screen_height - 85
        for each k in battle_selection:
            let u = battle_units[k]
            u.control = 0
            u.dest_x = x + battle_scroll_x
            u.dest_y = y + battle_scroll_y
        return 0
    return 0

define select_at(x, y):
    let h = hit_test(x + battle_scroll_x, y + battle_scroll_y, battle_rects, count(battle_units))
    if h < 1:
        return 0
    let u = battle_units[h - 1]
    if u.lane != 0 or u.strength <= 0:
        return 0
    for each k in battle_selection:
        if k == h - 1:
            return 0
    append(battle_selection, h - 1)
    battle_redraw = 1
    fn_0005B3B0(g_000A9C80, 0xC3BC, 0, 0x7FFF)
    return 0
```

## Outputs

`pointer_dispatch` returns 1 when the player confirms a retreat and 0 otherwise. It changes the scroll
position, the selection, the destinations and control codes of the selected units, and the battle's
start and pause flags.

## Edge cases

The first click on the first control starts the battle; every later click on it asks whether to
retreat. The control strip is tested at the event's screen position and units at the scrolled one,
so a click on the strip can also select the unit drawn under it. A click never removes a unit from the
selection. The destination is not limited on x, and on y it is limited to 45 to `screen_height - 85` of
the visible part before the scroll is added. A dead unit stays in the selection and gets destinations
too.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_0005B3B0` does beyond what the findings record.
- What `g_000A9C80` does beyond what the findings record.
