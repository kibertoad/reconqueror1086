---
id: RULE-ASSAULT-029
title: Change an actor's look
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-034, FND-ASSAULT-002]
conflicting: []
split_with: []
related: [RULE-ASSAULT-027, FMT-VIEW-001]
---

## Summary

An actor has four looks, at its base block and the three blocks after it: walking, attacking,
being hit and dying. Changing look copies that block into the actor's live block and keeps the
actor's colour family.

## When it runs

From the handlers and the death path (RULE-ASSAULT-014, RULE-ASSAULT-020, RULE-ASSAULT-022).

## Parameters

None. The rule defines `set_look(c, look)`, with `look` from 0 to 3.

## Inputs

`scene_blocks`.

## Procedure

```text
define set_look(c, look):
    let live = scene_blocks[block_of(c)]
    let family = live.color_family
    let offset_x = live.offset_x
    let offset_y = live.offset_y
    let covered = live.state_target
    let marks = live.selected
    scene_blocks[block_of(c)] = scene_blocks[base_block_of(c) + look]
    live.color_family = family
    live.offset_x = offset_x
    live.offset_y = offset_y
    live.state_target = covered
    live.selected = marks
```

## Outputs

Changes the actor's live block.

## Edge cases

The attack, hit and death looks are single blocks held for the length of their effect.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which fields the copier takes from the state block apart from the texture at `surface0`, and
  which it keeps apart from `color_family`, is not recorded. The procedure also keeps
  `offset_x`, `offset_y`, `state_target` and `selected`, which movement, cell occupancy and
  selection need.
- Where `scene_blocks` is kept is not recorded.
