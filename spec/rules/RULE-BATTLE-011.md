---
id: RULE-BATTLE-011
title: Field battle drawing
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-004, FND-BATTLE-008, FND-BATTLE-019, FND-STRATEGY-029]
conflicting: []
split_with: []
related: [FMT-BATTLE-001]
---

## Summary

Units are drawn dead first, then from the top of the field down and from left to right. Each frame of
MEN8.CSF shows one side, category, heading, step and state. A small marker is drawn over each selected
unit.

## When it runs

From `resolve_encounter` (RULE-BATTLE-001) after a pass that set `battle_redraw`.

## Parameters

None.

## Inputs

`battle_units`, `battle_selection`, `battle_scroll_x`, `battle_scroll_y` and `men8_image`.

## Procedure

```text
define draw_key(i):
    let u = battle_units[i]
    let alive = 0
    if u.strength > 0:
        alive = 1
    return (INT64(alive) << 40) + (INT64(u.y + 0x80000) << 20) + (u.x + 0x80000)

define draw_units():
    let order = []
    for i in 0..count(battle_units):
        append(order, i)
    stable_sort(order, draw_key)
    for each i in order:
        let u = battle_units[i]
        blit_frame(men8_image, u.lane + u.category + 5 * u.heading + u.phase % 5 + u.state, u.x - battle_scroll_x - 45, u.y - battle_scroll_y - 45)
    for each i in battle_selection:
        let u = battle_units[i]
        blit_frame(men8_image, 720, u.x - battle_scroll_x - 5, u.y - battle_scroll_y - 7)
```

## Outputs

No return value. Draws the units and the selection markers.

## Edge cases

Frames 0 to 719 are 90 by 90 and centred on the unit; frame 720 is 9 by 9 and not centred. The lane
adds 0 or 360, the category 0, 120 or 240, and the state 0, 40 or 80.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The order of two units with the same key: the original sorts with the C library, whose order of
  equal elements this procedure does not reproduce.
- How the backdrop and the control strip are drawn around the units.
