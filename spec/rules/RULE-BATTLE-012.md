---
id: RULE-BATTLE-012
title: Pointer events
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-020]
conflicting: []
split_with: []
related: [RULE-BATTLE-010, FMT-BATTLE-002]
---

## Summary

The mouse driver's callback queues each button change, up to 30, with the time and position. The
battle takes one event a pass and turns it into a code: a press, a long press, a click or a double
click, for either button.

## When it runs

`on_mouse_event` runs when the mouse driver calls the game; `next_pointer_code` runs when a screen asks for the next event.

## Parameters

`on_mouse_event(bits, x, y)`: the values of `mouse_event_bits`, `mouse_event_x` and `mouse_event_y`.

## Inputs

`pointer_events`, `pointer_clock`, `primary_down_time`, `secondary_down_time`, `primary_release_time`, `secondary_release_time`, `long_press_limit` and `double_click_limit`.

## Procedure

```text
define on_mouse_event(bits, x, y):
    for b in 1..5:
        if bits & (1 << b) != 0 and count(pointer_events) < 30:
            let e = new FMT-BATTLE-002
            e.time = pointer_clock
            e.x = x
            e.y = y
            e.kind = b - 1
            append(pointer_events, e)

define next_pointer_code():
    if count(pointer_events) == 0:
        return [0, 0, 0]
    let e = pointer_events[0]
    remove_at(pointer_events, 0)
    let code = 0
    if e.kind == 0:
        primary_down_time = e.time
        code = 1
    else if e.kind == 2:
        secondary_down_time = e.time
        code = 5
    else if e.kind == 1:
        if UINT32(e.time - primary_down_time) >= long_press_limit:
            primary_release_time = 0
            code = 2
        else if UINT32(e.time - primary_release_time) < double_click_limit:
            primary_release_time = 0
            code = 4
        else:
            primary_release_time = e.time
            code = 3
    else if e.kind == 3:
        if UINT32(e.time - secondary_down_time) >= long_press_limit:
            secondary_release_time = 0
            code = 6
        else if UINT32(e.time - secondary_release_time) < double_click_limit:
            secondary_release_time = 0
            code = 8
        else:
            secondary_release_time = e.time
            code = 7
    return [code, e.x, e.y]
```

## Outputs

`next_pointer_code` returns a list of the code, x and y: 0 when there is no event, 1 and 5 for a
primary and secondary press, 2 and 6 for a long press, 3 and 7 for a click, and 4 and 8 for a double
click. It changes the queue and the stored times.

## Edge cases

With both limits at 4, a press held for 4 counts of `pointer_clock` (about 220 ms) or more is a long
press, and a click less than 4 counts after the last click is a double click. The first release
after the game starts compares with times of 0. A release with no matching press compares with the
last press of that button.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Whether the callback discards an event or overwrites one when the queue is full.
- When `pointer_clock` starts counting.
