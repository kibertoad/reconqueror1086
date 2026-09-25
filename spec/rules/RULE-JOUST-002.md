---
id: RULE-JOUST-002
title: Practice joust pass
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-JOUST-001, FND-JOUST-002, FND-JOUST-003, FND-JOUST-004, FND-JOUST-005, FND-JOUST-006, FND-JOUST-007, FND-RNG-002]
conflicting: []
split_with: []
related: [RULE-JOUST-001, RULE-RNG-001]
---

## Summary

The practice joust plays `jousprac.SMK` 90 pixels down the screen with the lance sprite from `lance1.csf` drawn over it, measures the lance against three targets on frames 80 to 82, and decides a hit from the two error totals, or on a miss whether the opponent scores.

## When it runs

When the player picks the joust on the practice screen.

## Parameters

None. The rule defines `practice_pass()`, run once per pass of the worker loop, and `practice_result(player_points, opponent_points)`, run once after it; the caller passes 20 and 50.

## Inputs

`movie_frame`, `movie_frame_count` and the inputs of RULE-JOUST-001.

## Procedure

```text
table practice_lance_rows: INT32[5] = [150, 98, 46, 24, 2]
table practice_target_x: INT32[3] = [191, 161, 122]
table practice_target_y: INT32[3] = [191, 203, 211]

define practice_start():
    lance_start()
    practice_error_x = 0
    practice_error_y = 0
    practice_offset_x = 0
    practice_offset_y = 0

define practice_pass():
    lance_move()
    let frame = lance_frame(practice_lance_rows)
    lance_pull(120)
    if movie_frame >= 80 and movie_frame < 83:
        let k = movie_frame - 80
        practice_error_x = practice_error_x + abs(practice_target_x[k] - lance_x)
        practice_error_y = practice_error_y + abs(practice_target_y[k] - lance_y)
        practice_offset_x = practice_offset_x + practice_target_x[k] - lance_x
        practice_offset_y = practice_offset_y + practice_target_y[k] - lance_y
    return frame

define practice_result(player_points, opponent_points):
    if 90 - player_points > practice_error_x and 90 - player_points > practice_error_y:
        return 1
    if random(100) < opponent_points - player_points + 50:
        return 0
    return 2
```

## Outputs

The worker calls `practice_start()` and then runs passes until `movie_frame` equals
`movie_frame_count - 1`. After the first pass it waits until the player clicks or presses a
key. Each pass draws `lance1.csf` frame `practice_pass()` at `(lance_x, lance_y + 90)`, clipped
to the 640 by 300 movie area. The result 1 adds 2 to the player points and 0 adds 2 to the
opponent points; the points stay in the caller. After the result the worker shows a prompt to
press a key or button and a message, then waits for a click or key:

| Result | `practice_offset_x` | `practice_offset_y` | Message |
|---|---|---|---|
| 1 | any | any | win |
| 0 or 2 | above 0 | 0 | too far left |
| 0 or 2 | below 0 | 0 | too far right |
| 0 or 2 | 0 | below 0 | too low |
| 0 or 2 | 0 | above 0 | too high |
| 0 or 2 | above 0 | below 0 | left and low |
| 0 or 2 | above 0 | above 0 | left and high |
| 0 or 2 | below 0 | above 0 | right and high |
| 0 or 2 | below 0 | below 0 | right and low |
| 0 or 2 | 0 | 0 | none |

## Edge cases

The contact window counts passes, so the error totals grow with the number of passes the loop makes on frames 80 to 82, which depends on the processor. `lance1.csf` is drawn with the palette of the movie (FND-JOUST-007).

## What the sources say

None of the sources describe the practice joust.

## Differences between builds

None known.

## Open questions

- The wording of the messages is content of the game and is left out.
