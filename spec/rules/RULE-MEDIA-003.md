---
id: RULE-MEDIA-003
title: Drawing a PCX picture
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-MEDIA-005, FND-MEDIA-006, FND-MEDIA-003, FND-MEDIA-009, FND-BATTLE-015]
conflicting: []
split_with: []
related: [FMT-MEDIA-003, RULE-RES-001]
---

## Summary

A picture entry is read from the open archive, its header is checked, its RLE rows are decoded
straight into the screen buffer at `(x, y)`, and its palette is loaded when asked.

## When it runs

Whenever a screen draws a full picture or portrait; 11 places call it.

## Parameters

- `x`, `y`: the upper left corner on the screen.
- `index`: a directory index, or -1 to look up `name`.
- `name`: the entry name.
- `set_palette`: not 0 to load the picture's palette.

## Inputs

The open archive, `screen_buffer`, `screen_width`, `palette_buffer`.

## Procedure

```text
define draw_picture(x: INT32, y: INT32, index: INT32, name: char[], set_palette: INT32):
    let e = 0
    if index != -1:
        e = entry_at(index)
    else:
        e = find_entry(name)
    let b: UINT8[] = []
    # a missing entry, a failed allocation or a failed read stops the game
    read_entry(e, b)
    let f: FMT-MEDIA-003 = b
    if f.manufacturer != 0x0A or f.version != 5 or f.bits != 8:
        # the game stops with error code 4 and the name
        return
    let width = f.xmax - f.xmin + 1
    if width % 2 == 1:
        width = width + 1
    let height = f.ymax - f.ymin + 1
    let p = 128
    for r in 0..height:
        p = decode_pcx_row(b, p, (y + r) * screen_width + x, width)
    if set_palette != 0 and f.palette_marker == 0x0C:
        for c in 0..256:
            for k in 0..3:
                palette_buffer[4 * c + k] = f.palette[3 * c + k]
    fn_0006DF90(x, y, x + width - 1, y + height - 1)

define decode_pcx_row(b: UINT8[], p: UINT32, d: UINT32, width: INT32) -> UINT32:
    let n = 0
    while n < width:
        let v = b[p]
        p = p + 1
        if v & 0xC0 == 0xC0:
            let count = v & 0x3F
            for j in 0..count:
                screen_buffer[d + n + j] = b[p]
            p = p + 1
            n = n + count
        else:
            screen_buffer[d + n] = v
            n = n + 1
    return p
```

## Outputs

Pixels in `screen_buffer`, and with `set_palette` the 256 colours in `palette_buffer`. The entry's
bytes are freed.

## Edge cases

- An odd width draws one more column, taken from the stored row padding (FND-MEDIA-006).
- A run that ends past the row's width writes past it and the next row starts after it; no shipped
  picture has one.
- The encoding, plane count and bytes per line are not checked, and neither is the image size
  against the screen.
- The trailer is looked for at a fixed distance from the end; the rows are not checked to end there.

## What the sources say

The executable (FND-MEDIA-005); the shipped pictures (FND-MEDIA-006).

## Differences between builds

None known.

## Open questions

- How `palette_buffer` reaches the display, and whether its 8-bit values are scaled there.
- What `fn_0006DF90` does with the rectangle after it is recorded.
