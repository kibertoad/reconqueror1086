---
id: RULE-CONFIG-003
title: View, controller and movie settings
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-CONFIG-002, FND-CONFIG-003]
conflicting: []
split_with: []
related: [RULE-CONFIG-002, RULE-MEDIA-005]
---

## Summary

`POVWINSIZE` picks one of five rectangles for the first-person view, and the game stores the size
in use back into the file. `SLOWMACHINE` turns off part of the view's drawing, `USE_CYBERMAN` turns
on a CyberMan controller when one answers, and `FULL_MOVIE` picks the longer opening movie.

## When it runs

`read_view_settings` and `read_cyberman_setting` when the first-person view is set up;
`save_view_size` when the view closes; `opening_movie_name` when the title screen starts with
`MOVIE` on.

## Parameters

None.

## Inputs

`cyberman_present`, a value from outside the game.

## Procedure

```text
define read_view_settings():
    let size = ini_value(sprintf("POVWINSIZE"))
    if size == sprintf("NORMAL"):
        set_view_window(3, 6, 210, 152)
    else if size == sprintf("FULLSCREEN"):
        set_view_window(0, 0, 320, 200)
    else if size == sprintf("SMALL"):
        set_view_window(11, 12, 194, 140)
    else if size == sprintf("SMALLER"):
        set_view_window(19, 18, 178, 128)
    else if size == sprintf("SMALLEST"):
        set_view_window(23, 22, 170, 120)
    slow_machine = 0
    let slow = ini_value(sprintf("SLOWMACHINE"))
    if slow != 0 and slow == sprintf("TRUE"):
        slow_machine = 1

define set_view_window(x: INT32, y: INT32, width: INT32, height: INT32):
    view_window_x = x
    view_window_y = y
    view_window_width = width
    view_window_height = height

define save_view_size():
    if view_window_width == 320:
        set_ini_value(sprintf("POVWINSIZE"), sprintf("FULLSCREEN"))
    else if view_window_width == 194:
        set_ini_value(sprintf("POVWINSIZE"), sprintf("SMALL"))
    else if view_window_width == 178:
        set_ini_value(sprintf("POVWINSIZE"), sprintf("SMALLER"))
    else if view_window_width == 170:
        set_ini_value(sprintf("POVWINSIZE"), sprintf("SMALLEST"))
    else:
        set_ini_value(sprintf("POVWINSIZE"), sprintf("NORMAL"))

define read_cyberman_setting():
    use_cyberman = 0
    let setting = ini_value(sprintf("USE_CYBERMAN"))
    if setting != 0 and setting == sprintf("ON") and cyberman_present == 1:
        use_cyberman = 1

define opening_movie_name() -> char[]:
    if ini_value(sprintf("FULL_MOVIE")) == 0:
        return sprintf("movie3.smk")
    return sprintf("movie3f.smk")
```

## Outputs

`view_window_x`, `view_window_y`, `view_window_width`, `view_window_height`, `slow_machine` and
`use_cyberman`; the `POVWINSIZE` line of the file.

## Edge cases

- A missing or unknown `POVWINSIZE` keeps the rectangle the view set up before the read.
- `FULLSCREEN` also sets the four values at `0x0009CDBC` to `0x0009CDC8` to 320, 200, 0 and 0.
- `FULL_MOVIE` with any value, even `OFF`, picks `movie3f.smk`.
- `GRAPHICS` is read when the village options screen opens, and the value is dropped.

## What the sources say

None of the sources describe these settings.

## Differences between builds

None known.

## Open questions

- What the four values at `0x0009CDBC` to `0x0009CDC8` are.
- What the block `slow_machine` skips draws.
- What `cyberman_present` asks the mouse driver.
