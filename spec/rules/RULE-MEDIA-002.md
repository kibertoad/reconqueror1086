---
id: RULE-MEDIA-002
title: Drawing a sprite frame and a line of text
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-MEDIA-001, FND-MEDIA-003, FND-MEDIA-004, FND-MEDIA-005, FND-BATTLE-015]
conflicting: []
split_with: []
related: [FMT-MEDIA-002, RULE-MEDIA-001]
---

## Summary

A frame is drawn row by row into the screen buffer: literal segments copy their pixels, fill
segments repeat one pixel and skip segments leave the screen alone. The mask form draws every
literal or fill pixel in one colour, and text is a row of mask frames from `CONFONT.CSF`, each
moving x by its width.

## When it runs

`draw_sprite` is the unclipped path of every sprite draw; `draw_text` has 338 callers.

## Parameters

- `x`, `y`: the upper left corner on the screen.
- `colour`: a palette index.
- `start`: the offset of a frame in `s.data`.
- `text`: bytes up to a NUL.

## Inputs

`screen_buffer`, `screen_width`, `confont`.

## Procedure

```text
define draw_rows(x: INT32, y: INT32, s: sprite_set, start: UINT32, masked: INT32, colour: UINT8):
    let b = s.data
    let width = b[start] + 256 * b[start + 1]
    let height = b[start + 2] + 256 * b[start + 3]
    let p = start + 4
    for r in 0..height:
        let d = (y + r) * screen_width + x
        let n = b[p]
        p = p + 1
        for k in 0..n:
            let op = b[p]
            # the length is an INT16LE
            let length = b[p + 1] + 256 * b[p + 2]
            p = p + 3
            if op == 0:
                for j in 0..length:
                    if masked != 0:
                        screen_buffer[d + j] = colour
                    else:
                        screen_buffer[d + j] = b[p + j]
                p = p + length
            else if op == 2:
                for j in 0..length:
                    if masked != 0:
                        screen_buffer[d + j] = colour
                    else:
                        screen_buffer[d + j] = b[p]
                p = p + 1
            d = d + length
    fn_0006DF90(x, y, x + width - 1, y + height - 1)

define draw_sprite(x: INT32, y: INT32, s: sprite_set, start: UINT32):
    draw_rows(x, y, s, start, 0, 0)

define draw_sprite_mask(x: INT32, y: INT32, colour: UINT8, s: sprite_set, start: UINT32):
    draw_rows(x, y, s, start, 1, colour)

define draw_text(x: INT32, y: INT32, colour: UINT8, text: char[]):
    let i = 0
    while text[i] != 0:
        let start = confont.frames[text[i]]
        draw_sprite_mask(x, y, colour, confont, start)
        # the width is read as an INT16LE
        x = x + confont.data[start] + 256 * confont.data[start + 1]
        i = i + 1
```

## Outputs

Pixels in `screen_buffer`, and the drawn rectangle passed on for copying to the display.

## Edge cases

- Nothing is clipped: a frame that crosses the screen edge writes past the row or the buffer. The
  clipped path (FND-MEDIA-003) is separate and not described here.
- A row with a segment count of 0 would be drawn as 256 segments, since the count is decremented
  before it is tested. No shipped frame has one.
- `draw_text` does not stop at the screen edge or break lines.

## What the sources say

The executable (FND-MEDIA-003, FND-MEDIA-004).

## Differences between builds

None known.

## Open questions

- The clipped path through `0x0007DC00` and the clip rectangle at `0x000B0810`.
- What the variant `0x00064524` adds with `0x0006E0B0` and `0x000633D0`.
- What `fn_0006DF90` does with the rectangle after it is recorded.
