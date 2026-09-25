---
id: RULE-ASSAULT-019
title: An actor takes its new cell
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-030, FND-ASSAULT-003, FND-VIEW-002]
conflicting: []
split_with: []
related: [FMT-ASSAULT-004, FMT-VIEW-001, FMT-VIEW-002]
---

## Summary

When an actor crosses into another cell, the block it covered goes back into the old cell, the
new cell's block is saved, and the actor's block is written into the new cell, before any other
effect is processed.

## When it runs

From a movement tick that changes the actor's cell (RULE-ASSAULT-017).

## Parameters

- `e`: the movement effect.
- `old_x`, `old_y`: the cell the actor leaves.
- `new_x`, `new_y`: the cell it enters.

## Inputs

`scene_map` and `scene_blocks`.

## Procedure

```text
scene_map.cells[old_x * 128 + old_y] = e.covered_block
e.covered_block = scene_map.cells[new_x * 128 + new_y]
scene_blocks[e.moving_block].state_target = e.covered_block
scene_map.cells[new_x * 128 + new_y] = e.moving_block
```

## Outputs

Changes two map cells, the effect's saved block and the actor block's `state_target`.

## Edge cases

Because the actor's block blocks movement (FND-ASSAULT-003), no other mover can enter the cell
while the actor holds it, and actors sent to the same cell never share it.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- FND-ASSAULT-030 names `+0x24` and `+0x28` of the record the scheduler moves; the rule reads
  them as fields of the effect record (FMT-ASSAULT-004), which is not settled.
- Where `scene_blocks` and `scene_map` are kept is not recorded.
