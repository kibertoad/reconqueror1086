---
id: RULE-SOUND-004
title: Playing CD music
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-SOUND-006, FND-SOUND-004]
conflicting: []
split_with: []
related: [RULE-SOUND-003]
---

## Summary

With `CDMUSIC` on, a screen plays one audio track of the disc, and the screen loop replays it
whenever the drive reports it stopped.

## When it runs

`play_cd_music` when the start-up sequence (track 2), the title (track 3), the field battle (track
4), the game options screen (track 5) or the credits (track 6) begin, unless the CD is already
playing; `service_cd` from nine screen loops.

## Parameters

- `first`, `last`: the track range; callers pass one track as both.
- `loop`: 1 to replay the range when it ends; every caller passes 1.

## Inputs

`cd_music_on`, `cd_present` and the drive's track table.

## Procedure

```text
define play_cd_music(first: INT32, last: INT32, loop: INT32):
    if cd_music_on != 0:
        play_cd(first, last, loop)

define stop_cd_music():
    if cd_busy() != 0:
        stop_cd()

define cd_busy() -> INT32:
    if cd_present == 0:
        return 0
    # an audio status request is sent to the drive; 1 when the busy bit (bit 9) is set
    return 0

define play_cd(first: INT32, last: INT32, loop: INT32):
    if cd_present == 0:
        return
    stop_cd()
    cd_valid = 1 <= first and first <= cd_track_count and 1 <= last and last <= cd_track_count
    cd_loop = loop
    cd_start = cd_track_starts[first] + cd_adjust
    # both addresses are converted to sector numbers before the subtraction
    cd_length = cd_track_starts[last + 1] - cd_start - 1 - cd_adjust
    # the drive plays cd_length sectors from cd_start, even when cd_valid is 0

define service_cd():
    if cd_present == 0 or cd_loop == 0 or cd_valid == 0 or cd_busy() != 0:
        return
    if cd_paused != 0:
        # the drive resumes
        return
    stop_cd()
    # the drive plays cd_length sectors from cd_start again

define stop_cd():
    if cd_present == 0 or cd_busy() == 0:
        return
    cd_loop = 0
    cd_paused = 0
    # a stop audio request (0x85) is sent to the drive
```

## Outputs

Audio from the drive.

## Edge cases

- A replay starts only when a screen loop calls `service_cd`, so the gap after a track depends on
  the loop.
- `stop_cd` clears `cd_loop`, so a stopped track stays stopped until a screen plays it again.
- An invalid range is still sent to the drive.

## What the sources say

The executable (FND-SOUND-006); the disc's cue sheet.

## Differences between builds

None known.

## Open questions

- How `cd_track_starts`, `cd_track_count` and `cd_adjust` are filled, and the address conversion
  the game does with `0x00083BB0` and `0x00083C10`.
- What sets `cd_paused`.
