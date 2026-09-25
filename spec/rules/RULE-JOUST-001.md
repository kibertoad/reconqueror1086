---
id: RULE-JOUST-001
title: Lance motion and sprite frame
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-JOUST-002, FND-JOUST-003, FND-JOUST-010, FND-JOUST-011]
conflicting: []
split_with: []
related: []
---

## Summary

The joust lance is a point in movie coordinates with 8.8 velocities. Each pass it moves, is held inside x 50 to 400 and y 20 to 280, loses a fifth of its speed, is pulled towards the pointer, and jerks upward every half second of movie. Its sprite frame comes from its height and horizontal position.

## When it runs

Each pass of the practice joust (RULE-JOUST-002) and dragon run (RULE-JOUST-003) loops.

## Parameters

None. The rule defines `lance_start()`, `lance_move()`, `lance_pull(p)` and `lance_frame(rows)`: `p` is 120 for the practice joust and 50 for the dragon run, and `rows` is that run's table of five thresholds.

## Inputs

`pointer_x`, `pointer_y`, `movie_frame` and `movie_frame_ms`.

## Procedure

```text
define lance_start():
    lance_x8 = 225 << 8
    lance_y8 = 150 << 8
    lance_x = 225
    lance_y = 150
    lance_vx = 0
    lance_vy = 0

define lance_move():
    lance_x8 = lance_x8 + lance_vx
    lance_y8 = lance_y8 + lance_vy
    lance_x = lance_x8 / 256
    lance_y = lance_y8 / 256
    if lance_x < 50:
        lance_x = 50
        lance_x8 = 50 << 8
    if lance_y < 20:
        lance_y = 20
        lance_y8 = 20 << 8
    if lance_x > 400:
        lance_x = 400
        lance_x8 = 400 << 8
    if lance_y > 280:
        lance_y = 280
        lance_y8 = 280 << 8

define lance_pull(p):
    lance_vx = lance_vx * 80 / 100 + ((pointer_x - lance_x) / 10) * 0x3200 / p
    lance_vy = lance_vy * 80 / 100 + ((pointer_y - lance_y) / 10) * 0x3200 / p
    if movie_frame % (500 / movie_frame_ms) == 0:
        lance_vy = lance_vy - (150 - p) * 50

define lance_frame(rows):
    let count = 1
    while rows[count - 1] > lance_y:
        count = count + 1
    let column = (lance_x - 50) / ((400 - 50 + 2) / 5)
    return max(0, min(5 * count - 1 - column, 24))
```

## Outputs

`lance_x` and `lance_y` are the lance point in movie pixels; `lance_frame` gives the frame of the lance sprite file to draw there (RULE-JOUST-002).

## Edge cases

`lance_frame` has no limit on its scan. The practice table ends at 2, below any lance y. The dragon table ends at 92, and for a lance above y 92 the scan reads past the table and the frame is always 24 (BUG-JOUST-001).

## What the sources say

None of the sources describe the lance.

## Differences between builds

None known.

## Open questions

- Where the game keeps `movie_frame_ms` is not recorded.
- The loop runs passes as fast as the processor allows, so how many passes fall on one movie frame is not fixed.
