---
id: RULE-ASSAULT-015
title: Direct approach and go-to handlers
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-022, FND-ASSAULT-023, FND-VIEW-003, FND-ASSAULT-001, FND-ASSAULT-042]
conflicting: []
split_with: []
related: [RULE-VIEW-001, RULE-ASSAULT-018, RULE-ASSAULT-027, FMT-ASSAULT-001, FMT-ASSAULT-003, FMT-VIEW-001]
---

## Summary

An approaching actor faces its target, or its destination cell in mode 12, rounded to the
nearest of the four cardinal headings, and starts a movement effect that stops at the first
obstacle. It aims again each time the effect ends.

## When it runs

As the handler of modes 7, 8 and 16, and of mode 12 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `approach(c)`, `go_to(c)` and `start_direct(c, heading)`.

## Inputs

`effect_defs`.

## Procedure

```text
define start_direct(c, heading):
    set_heading(c, (heading + 0x20) & 0xC0)
    let d = effect_defs[scene_blocks[block_of(c)].movement]
    let flags = (d.flags & ~0xFF) | (d.flags & 0xA7) | 0x10
    start_effect(c, d, flags, d.step_x, d.step_y, d.interval)

define approach(c):
    let t = combatants[c.target]
    start_direct(c, heading_to(t.x - c.x, t.y - c.y))

define go_to(c):
    start_direct(c, heading_to(c.dest_x - (c.x >> 8), c.dest_y - (c.y >> 8)))
```

## Outputs

Sets the actor's heading and starts a movement effect.

## Edge cases

- A target exactly on a diagonal gives a heading of `0x20`, `0x60`, `0xA0` or `0xE0`, which
  rounds to the next cardinal clockwise.
- With the shipped flags `0x142` the effect runs with flags `0x112`.
- A destination the actor cannot enter makes it stop and aim again at every effect, without
  end.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The go-to handler clears `target`; the value it writes is not recorded.
- Which cell the go-to handler subtracts, the one under the live position or the one the actor
  occupies, is not recorded; the procedure uses the live position.
- Where `combatants`, `effect_defs` and `scene_blocks` are kept is not recorded.
