---
id: RULE-ASSAULT-016
title: Escape handler
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-025, FND-VIEW-003, FND-ASSAULT-001, FND-ASSAULT-042]
conflicting: []
split_with: []
related: [RULE-VIEW-001, RULE-ASSAULT-018, RULE-ASSAULT-027, FMT-ASSAULT-001, FMT-ASSAULT-003, FMT-VIEW-001]
---

## Summary

An escaping actor faces directly away from its stored target, in any direction, and walks one
and a half times as far per tick as usual.

## When it runs

As the handler of modes 9 and 10 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `flee(c)`.

## Inputs

`effect_defs`. The conversion of the scaled steps runs on the x87 with the precision and
rounding in effect at the time, which no finding records; the procedure assumes the processor's
starting rounding to nearest even.

## Procedure

```text
define flee(c):
    let t = combatants[c.target]
    set_heading(c, heading_to(c.x - t.x, c.y - t.y))
    let d = effect_defs[scene_blocks[block_of(c)].movement]
    let step_x = round_even(d.step_x * 1.5)
    let step_y = round_even(d.step_y * 1.5)
    let flags = (d.flags & ~0xFF) | (d.flags & 0xA7) | 0x10
    start_effect(c, d, flags, step_x, step_y, d.interval)
```

## Outputs

Sets the actor's heading and starts a movement effect.

## Edge cases

- The heading is not rounded to a cardinal, so the escape runs at any angle.
- With the shipped step of 64 each tick moves 96 units, and the flags `0x142` become `0x112`.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The x87 control word in effect when the steps are converted is not recorded.
- FND-ASSAULT-025 records the resulting flags as `0x112`; whether the handler rewrites the low
  byte as the other direct handlers do or stores `0x112` outright is not recorded.
- Where `combatants`, `effect_defs` and `scene_blocks` are kept is not recorded.
