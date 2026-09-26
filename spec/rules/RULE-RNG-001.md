---
id: RULE-RNG-001
title: The game's random number generator
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-RNG-002, FND-RNG-003, FND-ASSAULT-031, FND-SAVE-004, FND-SOUND-003]
conflicting: []
split_with: []
related: [RULE-TALK-001, RULE-SOUND-002]
---

## Summary

The game has one generator, `rng`, a linear congruential generator whose 32-bit state is
`rng_state`. Every random decision goes through `draw()`, and three helpers reduce a draw to a
range: `random(n)` scales it to 0 to `n - 1`, `random_inclusive(n)` takes its remainder to give 0
to `n`, and `dice(count, sides)` adds `count` rolls of 1 to `sides`.

## When it runs

`draw()` runs wherever a rule draws, including the sound mixer's choice of a voice to stop when all
ten are busy (RULE-SOUND-002). `seed_random` is called at two places, and nowhere else:

- once at the start of a session, from the routine at `0x00024C20`, which the session set-up at
  `0x00029FF4` calls as its first instruction;
- by `show_prompt` of RULE-TALK-001, at `0x0001A173`, before it draws one of a node's prompt
  variants, when the node has more than one.

The state is not written to a saved game, so loading a game keeps the state the session already
had.

## Parameters

`seed_random(value)`, `random(n)`, `random_inclusive(n)` and `dice(count, sides)`.

## Inputs

`rng_state`, and `fn_0006B3B4`, whose result seeds the generator at the start of a session.

## Procedure

```text
# draw() is the generator: it sets
#     rng_state = UINT32(rng_state * 0x41C64E6D + 0x3039)
# and gives (rng_state >> 16) & 0x7FFF, a value from 0 to 32767.

define seed_random(value):
    rng_state = UINT32(value)

define random(n):
    return INT32(UINT32(draw() * n) >> 15)

define random_inclusive(n):
    return draw() % (n + 1)

define dice(count, sides):
    let total = 0
    let left = count
    while left > 0:
        total = total + random(sides) + 1
        left = left - 1
    return total
```

## Outputs

`random` and `random_inclusive` return an `INT32`. At the start of a session the game runs
`seed_random(UINT32(fn_0006B3B4(0)) >> 1)`. Before a prompt variant is drawn it runs
`seed_random(fn_0006B3B4(0))`, with no shift.

## Edge cases

`random(n)` keeps only the low 32 bits of the product before the shift, which matters only for
`n` above 131072. `random_inclusive(-1)` divides by 0. `dice` with a count of 0 or less makes no
draw and gives 0.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_0006B3B4` returns; it is likely the C library's `time`. If it is, two prompts drawn in
  the same second get the same variant, and every draw after a prompt follows from the clock.
