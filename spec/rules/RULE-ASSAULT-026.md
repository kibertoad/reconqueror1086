---
id: RULE-ASSAULT-026
title: The player's foreground weapon swing
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-040, FND-ASSAULT-032, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-ASSAULT-027, RULE-RNG-001, FMT-ASSAULT-001, FMT-ASSAULT-002]
---

## Summary

The player's weapon moves from a start point below or beside the view towards a point near the
click and back, at a speed set by the weapon's swing divisor. Crossbows rise from the bottom of
the view instead.

## When it runs

When the player attacks (RULE-ASSAULT-005), and then on every drawn frame until the swing ends.

## Parameters

- `x`, `y`: the click position in the view.

## Inputs

`combat_view_width`, `combat_view_height`, `swing_frame_width` and `swing_frame_height`.

## Procedure

```text
let w = swing_frame_width
let h = swing_frame_height
let row = row_of(combatants[player_index])
if row <= 3:
    swing_target_x = x - random(8)
    swing_target_y = max(y - random(16), combat_view_height - h)
    swing_start_x = swing_target_x
    swing_start_y = swing_target_y + combat_view_height / 2
else if row <= 14:
    swing_target_x = x + 16 - random(40)
    swing_target_y = max(y - random(40), combat_view_height - h)
    if swing_target_y > combat_view_height / 3:
        swing_start_x = swing_side_x(swing_target_x)
        swing_start_y = swing_target_y - combat_view_height / 2
    else:
        swing_start_x = swing_target_x
        swing_start_y = swing_target_y + combat_view_height / 2
else if row <= 22:
    swing_target_x = x + 16 - random(32)
    swing_target_y = max(y - random(24), combat_view_height - h)
    swing_start_x = swing_side_x(swing_target_x)
    swing_start_y = swing_target_y - 56
else:
    swing_target_x = x - w / 2
    swing_target_y = combat_view_height - h
    swing_start_x = swing_target_x
    swing_start_y = swing_target_y
let divisor = combat_rows[row].swing_divisor
swing_vx = ((swing_target_x - swing_start_x) << 9) / divisor
swing_vy = ((swing_target_y - swing_start_y) << 9) / divisor
swing_x = swing_start_x << 8
swing_y = swing_start_y << 8
```

## Outputs

Sets the swing's target, start, position in 8.8 pixels and velocity. Each drawn frame then adds
velocity times the elapsed milliseconds to the position; at the target the horizontal velocity
reverses, and for melee rows the vertical velocity reverses and halves. The frame drawn is
offset 2 while approaching, offset 1 while the horizontal distance to the target is below a
quarter of the view width, and offset 0 while returning; rows 23 and 24 keep offset 2.

## Edge cases

- A smaller swing divisor gives a faster swing; the shipped divisors run from 380 to 1,000.
- Rows 23 and 24 do not move: their start and target are the same point.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `swing_side_x`, which picks the left or right start from the target's x and mirrors the frame
  for the left side, is not recorded in detail.
- Which side of the `H / 3` line counts as below it is not recorded; the procedure takes larger
  y, lower on the screen.
- The order of the two random draws in each branch is not recorded; the procedure draws x first.
- Where `combatants`, `swing_frame_height`, `swing_frame_width`, `swing_start_x`, `swing_start_y`, `swing_target_x`, `swing_target_y`, `swing_vx`, `swing_vy`, `swing_x` and `swing_y` are kept is not recorded.
