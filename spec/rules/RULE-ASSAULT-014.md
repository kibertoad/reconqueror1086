---
id: RULE-ASSAULT-014
title: Standing and wandering handlers
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-022, FND-ASSAULT-004, FND-ASSAULT-042]
conflicting: []
split_with: []
related: [RULE-ASSAULT-018, RULE-ASSAULT-027, RULE-ASSAULT-029, FMT-ASSAULT-001, FMT-ASSAULT-003, FMT-VIEW-001]
---

## Summary

In modes 1 to 4 an actor stands and shows its base look. In modes 5, 6 and 17 it walks on in the
direction it faces, with the movement effect's flags changed so that it turns left at obstacles.

## When it runs

As the handler of modes 1 to 4, and of modes 5, 6 and 17 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `hold(c)` and `wander(c)`.

## Inputs

`effect_defs`.

## Procedure

```text
define hold(c):
    set_look(c, 0)

define wander(c):
    let d = effect_defs[scene_blocks[block_of(c)].movement]
    let flags = (d.flags & ~0xFF) | (d.flags & 0xA7) | 0x40
    start_effect(c, d, flags, d.step_x, d.step_y, d.interval)
```

## Outputs

`hold` changes the actor's look; `wander` starts a movement effect.

## Edge cases

The shipped movement descriptors have flags `0x142`, which the rewrite leaves as they are.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The wandering handler clears the target fields; which fields and to what value is not
  recorded, and no test reads them before setting them again.
- Where `effect_defs` and `scene_blocks` are kept is not recorded.
