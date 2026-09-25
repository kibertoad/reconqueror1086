---
id: RULE-ASSAULT-005
title: Pointer clicks in the first-person view
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-035, FND-ASSAULT-044, FND-ASSAULT-008, FND-VIEW-006, FND-ASSAULT-001, FND-ASSAULT-032]
conflicting: []
split_with: []
related: [FMT-VIEW-002, RULE-VIEW-003, RULE-ASSAULT-021, RULE-ASSAULT-023, RULE-ASSAULT-026, RULE-ASSAULT-027, RULE-ASSAULT-028, FMT-ASSAULT-001, FMT-VIEW-001]
---

## Summary

A click in the view casts a ray through the clicked pixel. Selected retainers are sent to the
spot the ray hits. Otherwise a click on a retainer toggles its selection, a click on an enemy
sets the selected retainers on it or makes the player attack it, and a click on a door or item
uses it when it is close enough.

## When it runs

When the player clicks inside the first-person view.

## Parameters

- `column` and `row`: the pointer's top-left position in the view, in pixels.
- `primary`: the click's flag, 0 for the click that can send retainers to a spot.

## Inputs

`view_x`, `view_y` and `view_heading`.

## Procedure

```text
if not cast_ray(view_x, view_y, view_heading, view_lateral(column + 9), row + 9):
    return
if primary == 0:
    let sent = false
    for i in 10..count(combatants):
        let c = combatants[i]
        if scene_blocks[block_of(c)].selected == 1:
            c.dest_x = hit_x >> 8
            c.dest_y = hit_y >> 8
            c.next_mode = 12
            sent = true
    for i in 10..count(combatants):
        scene_blocks[block_of(combatants[i])].selected = 0
    if sent:
        return
let block = scene_blocks[hit_block]
if block.behaviour & 0x80 != 0:
    let index = combatant_of_block(hit_block)
    if index < 0:
        return
    let other = combatants[index]
    if other.side == 0:
        if other.health > 0:
            block.selected = block.selected ^ 1
        return
    let ordered = false
    for i in 10..count(combatants):
        let c = combatants[i]
        if c.side == 0 and scene_blocks[block_of(c)].selected == 1:
            c.target = index
            c.next_mode = 8
            ordered = true
    for i in 10..count(combatants):
        scene_blocks[block_of(combatants[i])].selected = 0
    if ordered:
        return
    let player = combatants[player_index]
    call RULE-ASSAULT-026(column + 9, row + 9)
    if hit_depth < combat_rows[row_of(player)].reach + 0x40:
        call RULE-ASSAULT-023(player, other)
        call RULE-ASSAULT-028(other)
    return
if block.behaviour & 0x10 != 0 and hit_depth < 0x280:
    call RULE-ASSAULT-021(hit_block, hit_cell_x, hit_cell_y)
    return
if block.behaviour & 0x20 != 0:
    let player = combatants[player_index]
    if hit_depth < combat_rows[row_of(player)].reach + 0x40:
        scene_map.cells[hit_cell_x * 128 + hit_cell_y] = UINT16(block.state_target)
```

## Outputs

Changes combatants' destinations, targets, requested modes and selection marks, or passes the
click to the player's attack or to an object action.

## Edge cases

- With a selection, a click never reaches the enemy or object under the pointer: the
  destination is set first and the click ends there.
- The destination is the cell of the contact point, which can be a wall cell; movement decides
  whether it can be entered (RULE-ASSAULT-017).
- An object at depth `0x280` or more is out of reach.
- Scenery marked `weapon_contact` within the weapon's reach plus `0x40` is replaced at once by
  its state target.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The pointer position the view uses relative to the screen, and whether the click row and
  column are those of the view, are not recorded.
- How the ordered combatants' requested modes 12 and 8 are installed, since the pointer path
  does not call the transition helper, is not recorded.
- Whether the weapon path compares the depth with the reach plus `0x40` strictly, and in what
  order it starts the swing, applies the damage and starts the blood effect, is not recorded.
- The crossbow's use of `ammunition` on this path is not recorded in full.
- Whether a block marked `weapon_contact` also starts the swing, and whether the weapon path
  reaches it for a block marked `actionable` as well, is not recorded.
- Where `scene_blocks`, `scene_map`, `view_heading`, `view_x` and `view_y` are kept is not recorded.
