---
id: RULE-MEDIA-005
title: Playing a Smacker movie
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-MEDIA-009, FND-MEDIA-008, FND-RES-006, FND-BATTLE-015, FND-JOUST-002, FND-JOUST-006]
conflicting: []
split_with: []
related: [FMT-MEDIA-006, RULE-CONFIG-002]
---

## Summary

A movie is opened through the Smacker library, without sound when `DIG_SPEECH=OFF`, and played frame
by frame into the screen buffer or straight to the screen, repeating a given number of times, with
an optional callback that can stop it.

## When it runs

Conversation, store, transition, title and full-screen movies; `play_movie` has nine callers and
`play_movie_fullscreen` five.

## Parameters

- `path`, `flags`, `extra`: passed to the library's open.
- `x`, `y`: where the movie is drawn.
- `repeats`: how many times it plays; below 1 counts as 1.
- `set_palette`: 1 to load each palette frame into `palette_buffer`.
- `stop`: a function given the frame number, or 0.
- `direct`: not 0 to draw straight to the screen.

## Inputs

`screen_buffer`, `screen_width`, `screen_height`, `palette_buffer`, `cd_path` from `CONQUER.INI`.

## Procedure

```text
define open_movie(path: char[], flags: UINT32, extra: INT32) -> movie:
    let speech = ini_value("DIG_SPEECH")
    if speech != 0 and speech == "OFF":
        flags = flags & 0xFFFF01FF
    return fn_0006C287(path, flags, extra)

define play_movie(m: movie, x: INT32, y: INT32, repeats: INT32, set_palette: INT32, stop: INT32, direct: INT32) -> INT32:
    if m == 0 or x < 0 or y < 0 or x + movie_width > screen_width:
        return 0
    if repeats < 1:
        repeats = 1
    let shown = 0
    while true:
        # when the frame carries a palette and set_palette is 1, its 256 colours are copied to
        # palette_buffer four bytes apart, then shifted right by 2 inside the movie object
        # the frame is decoded; with direct == 0 its changed rectangles are copied from
        # screen_buffer to the display
        shown = shown + 1
        if movie_frame == movie_frame_count - 1:
            repeats = repeats - 1
            if repeats <= 0:
                break
        # the library moves to the next frame and waits for its time, calling stop between waits
        if stop != 0 and stop(movie_frame) == 1:
            break
    # the movie is closed
    return shown

define play_movie_fullscreen(name: char[]):
    # the display switches to 320 by 200
    let dir = ini_value("CD_PATH")
    if dir == 0:
        dir = ".\CD\"
    let m = open_movie(sprintf("%s%s", dir, name), 0x200, -1)
    if m == 0:
        # the game stops with "Unable to open Smack movie"
        return
    play_movie(m, (320 - movie_width) / 2, (200 - movie_height) / 2, 1, 1, stop_on_input, 1)
    fn_00072119(1)
    if ini_value("DELAYVGA") == "ON":
        fn_00072119(3)
    # the display switches back to 640 by 480; a failure stops the game with
    # "Conq-Vid: Initializing video system. Aborting."

define stop_on_input(frame: INT32) -> INT32:
    # the next input event is read; event 3 or 7, or a waiting key, clears the input and returns 1
    return 0
```

## Outputs

The number of frames shown. Pixels in `screen_buffer` or on the screen, and colours in
`palette_buffer`.

## Edge cases

- A movie wider than the screen from `x` is not played at all; there is no check on height.
- `stop` is called only between frames, so a click takes effect at the next frame.
- `play_movie_fullscreen` does not check the movie against 320 by 200; a larger movie gets a
  negative position and is not played.

## What the sources say

The executable (FND-MEDIA-009); the shipped movies (FND-MEDIA-008).

## Differences between builds

None known.

## Open questions

- The input event codes 3 and 7.
- What `fn_00072119` does with 1 and 3.
- The meaning of the open flags `0x200` and `0x80` inside the library, which `fn_0006C287` opens with.
