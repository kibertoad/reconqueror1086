---
id: RULE-ASSAULT-017
title: One movement tick of an actor
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-029, FND-ASSAULT-004, FND-VIEW-004, FND-VIEW-002, FND-ASSAULT-003]
conflicting: []
split_with: []
related: [RULE-VIEW-002, RULE-ASSAULT-018, RULE-ASSAULT-019, RULE-ASSAULT-027, FMT-ASSAULT-001, FMT-ASSAULT-003, FMT-VIEW-001, FMT-VIEW-002]
---

## Summary

A movement tick turns the effect's step by the actor's heading and adds it to the actor's offset
within its cell. A step that brings the offset beyond `0x59` either way looks at the next cell
on that axis and is refused when that cell blocks movement. An offset beyond `0x80` moves the
actor into the next cell.

## When it runs

From the effect scheduler, for each tick of a movement effect (RULE-ASSAULT-018).

## Parameters

- `e`: the effect.

## Inputs

`scene_map` and `scene_blocks`.

## Procedure

```text
let c = effect_owner(e)
let b = scene_blocks[block_of(c)]
let old_x = cell_x_of(c)
let old_y = cell_y_of(c)
let step = [rotate_x(effect_step_x(e), effect_step_y(e), heading_of(c)),
            rotate_y(effect_step_x(e), effect_step_y(e), heading_of(c))]
let offset = [b.offset_x, b.offset_y]
let cell = [old_x, old_y]
for axis in 0..2:
    let n = offset[axis] + step[axis]
    if n < -0x59 or n > 0x59:
        let next = [cell[0], cell[1]]
        if n < 0:
            next[axis] = next[axis] - 1
        else:
            next[axis] = next[axis] + 1
        let blocker = scene_blocks[scene_map.cells[next[0] * 128 + next[1]]]
        if blocker.behaviour & 0x02 != 0:
            if effect_flags(e) & 0x10 != 0:
                stop_effect(e)
                return
            if effect_flags(e) & 0x40 != 0:
                offset[axis] = 0
                set_heading(c, (((heading_of(c) + 0x20) & 0xC0) - 0x40) & 0xFF)
            continue
    if n < -0x80:
        n = n + 0x100
        cell[axis] = cell[axis] - 1
    else if n > 0x80:
        n = n - 0x100
        cell[axis] = cell[axis] + 1
    offset[axis] = n
b.offset_x = offset[0]
b.offset_y = offset[1]
if cell[0] != old_x or cell[1] != old_y:
    call RULE-ASSAULT-019(e, old_x, old_y, cell[0], cell[1])
    set_cell(c, cell[0], cell[1])
c.x = (cell[0] << 8) + 0x80 + b.offset_x
c.y = (cell[1] << 8) + 0x80 + b.offset_y
```

## Outputs

Changes the actor's offset, position, heading and cell, and the map when the cell changes.

## Edge cases

- From an offset of 0 the shipped step of 64 gives 64, 128 and 192, and 192 becomes -64 in the
  next cell: the first cell change comes on the third tick (600 ms at 200 ms a tick) and each
  later one after four ticks (800 ms).
- An offset of exactly `0x80` or `-0x80` stays in the current cell, and an offset from `-0x59`
  to `0x59` never looks at the neighbouring cell.
- A direct mover (flag `0x10`) keeps its offset and heading when refused, and its effect ends so
  the actor decides again (RULE-ASSAULT-018). A wandering mover (flag `0x40`) drops the refused
  axis's offset to 0, turns left to the next cardinal, and keeps going.
- Because every live actor block blocks movement, a second actor sent to an occupied cell stops
  at its edge and retries.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The order in which the two axes are tested, and whether a refused axis under flag `0x10`
  still lets the other axis move in the same tick, is not recorded; the procedure takes x first
  and stops the whole tick.
- Whether the scheduler writes `x` and `y` at each tick is not recorded; the procedure keeps
  them at the cell centre plus the offset, which the tests that read them need.
- What the neighbouring-cell lookup reads outside the map is not recorded.
- Where `scene_blocks` and `scene_map` are kept is not recorded.
