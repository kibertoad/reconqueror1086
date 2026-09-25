---
id: RULE-JOUST-003
title: Dragon run
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-JOUST-008, FND-JOUST-009, FND-JOUST-010, FND-JOUST-011]
conflicting: []
split_with: []
related: [RULE-JOUST-001]
---

## Summary

The dragon run plays `DRJSTRUN.SMK` with the lance drawn over it, measures the lance against the dragon's eye on frames 108 to 133, and succeeds when a threshold from lance experience and three items beats both error totals.

## When it runs

When the player's army reaches the dragon (DRAGON area).

## Parameters

None. The rule defines `dragon_pass()`, run once per pass of the worker loop while `movie_frame` is below 134, and `dragon_succeeds()`, run after it.

## Inputs

`lance_experience`, `possession_counts`, `movie_frame` and the inputs of RULE-JOUST-001.

## Procedure

```text
table dragon_lance_rows: INT32[5] = [240, 188, 136, 114, 92]
table dragon_target_x: INT32[26] = [277, 277, 277, 277, 277, 277, 277, 276, 276, 276, 276, 277, 276, 274, 275, 274, 274, 272, 272, 270, 269, 267, 265, 262, 258, 254]
table dragon_target_y: INT32[26] = [125, 124, 124, 123, 122, 122, 122, 120, 119, 117, 117, 115, 115, 114, 111, 108, 108, 104, 101, 98, 95, 91, 85, 81, 75, 68]

define dragon_start():
    lance_start()
    dragon_error_x = 0
    dragon_error_y = 0

define dragon_pass():
    lance_move()
    let frame = lance_frame(dragon_lance_rows)
    lance_pull(50)
    if movie_frame >= 108 and movie_frame < 134:
        dragon_error_x = dragon_error_x + abs(dragon_target_x[movie_frame - 108] - lance_x)
        dragon_error_y = dragon_error_y + abs(dragon_target_y[movie_frame - 108] - lance_y)
    return frame

define dragon_succeeds():
    let bonus = 0
    if possession_counts[0x36] != 0:
        bonus = bonus + 4
    if possession_counts[0x40] != 0:
        bonus = bonus + 4
    if possession_counts[0x3B] != 0:
        bonus = bonus + 4
    if bonus == 12:
        bonus = 17
    let experience = max(0, min(lance_experience, 20))
    let threshold = 26 * (experience - 20 + bonus)
    return threshold > dragon_error_x and threshold > dragon_error_y
```

## Outputs

Each pass draws the lance frame `dragon_pass()` at `(lance_x, lance_y + 90)`. The worker returns 0 when `dragon_succeeds()` is true, which plays the winning movie.

## Edge cases

Equality with either total fails. With no items the threshold is at most 0, so the run always fails. The contact window counts passes, as in RULE-JOUST-002.

## What the sources say

The GameFAQs guide (SRC-GAMEFAQS-66730) gives the lance, shield and armour as the route to the dragon, which agrees with the three items that raise the threshold.

## Differences between builds

None known.

## Open questions

- Where the game keeps the error totals and the lance state is not recorded beyond their being the worker's own.
- The nonzero values the worker returns on failure are not recorded.
- Where the game keeps the character table behind `lance_experience` and the possession table behind `possession_counts` is not recorded.
