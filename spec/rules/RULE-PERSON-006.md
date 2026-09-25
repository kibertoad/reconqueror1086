---
id: RULE-PERSON-006
title: Retirement at 30
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-PERSON-001, FND-PERSON-003, FND-PERSON-008, FND-STRATEGY-014, FND-STRATEGY-019, SRC-MANUAL, FND-STRATEGY-036]
conflicting: []
split_with: []
related: [RULE-PERSON-001]
---

## Summary

When the player's AGE reaches 30 the campaign ends with the retirement movie.

## When it runs

Each time the routine at `0x0002AD62` runs; what calls it is not recorded.

## Parameters

None.

## Inputs

`g_0009ADC0`, `characters_loaded`, `g_0009ADB8` and the character table.

## Procedure

```text
define check_retirement():
    if g_0009ADC0 != 0:
        return 0
    if characters_loaded and attr(0, 18) == 30:
        g_0009AABC = 0
        fn_00024CA0()
        # when g_0009ADB8 is not 0, plays the movie avg_end.smk from the CD path
```

## Outputs

None known.

## Edge cases

The test is equality, so an AGE that passes 30 without being checked at 30 does not end the game.

## What the sources say

SRC-MANUAL (p. 2) sets the goal of becoming King or King's Champion before age 30, when the knight
is forced into retirement.

## Differences between builds

None known.

## Open questions

- What writes AGE after dubbing; no immediate write of field 18 was found.
- What `g_0009ADC0`, `g_0009AABC`, `fn_00024CA0` and `g_0009ADB8` hold or do here.
- What the routine does after the movie, what it returns, and what calls it.
