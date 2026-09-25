---
id: RULE-ASSAULT-018
title: Effect scheduling, tick timing, strike completion and rethink
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-028, FND-ASSAULT-016, FND-ASSAULT-042, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-ASSAULT-007, RULE-ASSAULT-017, RULE-ASSAULT-023, RULE-ASSAULT-027, FMT-ASSAULT-001, FMT-ASSAULT-003, FMT-ASSAULT-004]
---

## Summary

Every animation and movement is an effect in one of 64 slots. An effect ticks only when strictly
more than its interval has passed since its last tick was due. A strike deals its damage at its
last tick, and when an actor's effect ends the actor decides its next state at once.

## When it runs

Once per pass of the assault main loop, after input and before the thinker (RULE-ASSAULT-006).

## Parameters

None. The rule defines `start_effect(c, d, flags, step_x, step_y, interval)`,
`cancel_effect(c)` and `stop_effect(e)`.

## Inputs

`now`, the game's millisecond clock.

## Procedure

```text
define start_effect(c, d, flags, step_x, step_y, interval):
    for i in 0..64:
        if effects[i].active == 0:
            init_effect(effects[i], c, d, flags, step_x, step_y, interval)
            effects[i].deadline = now
            c.effect = i
            return

define cancel_effect(c):
    if c.effect >= 0:
        effects[c.effect].active = 0
        c.effect = -1

define stop_effect(e):
    set_effect_steps(e, 0, 0)
    set_effect_ticks_left(e, 0)

for i in 0..64:
    let e = effects[i]
    if e.active == 0:
        continue
    if now - e.deadline <= effect_interval(e):
        continue
    e.deadline = e.deadline + effect_interval(e)
    advance_effect(e)
    if effect_is_movement(e):
        call RULE-ASSAULT-017(e)
    let c = effect_owner(e)
    if effect_ticks(e) == effect_tick_count(e) and c.mode == 11 and combatants[c.target].health > 0:
        call RULE-ASSAULT-023(c, combatants[c.target])
    if effect_finished(e):
        e.active = 0
        c.effect = -1
        call RULE-ASSAULT-007(c)
```

## Outputs

Advances effects, moves actors, applies strike damage, frees finished effects and runs state
decisions.

## Edge cases

- A two-tick effect of 200 ms lasts a little over 400 ms: each tick waits for the first pass
  more than 200 ms after the previous one was due, and the overrun is kept.
- A strike whose actor has left mode 11, or whose target has died, deals no damage.
- When all 64 slots are in use, no effect starts.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The clock `now` reads is not recorded, and neither is its resolution.
- Whether the comparison with the interval is signed or unsigned is not recorded.
- The layout of the effect record apart from `active` and `deadline` is not recorded; the rule
  reaches the rest through the functions of RULE-ASSAULT-027.
- What the constructor does when no slot is free is not recorded.
- Where `combatants` and `effects` are kept is not recorded.
