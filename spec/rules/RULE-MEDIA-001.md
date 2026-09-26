---
id: RULE-MEDIA-001
title: Loading a CSF sprite file
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-MEDIA-001, FND-MEDIA-002]
conflicting: []
split_with: []
related: [FMT-MEDIA-001, FMT-MEDIA-002, RULE-RES-001, RULE-MEDIA-002]
---

## Summary

A CSF file is read whole, from a disk file or the open archive, its `2J` tag is checked, and the game
keeps the bytes with a pointer to each frame and, for each frame, a pointer to each row.

## When it runs

Whenever a screen loads a sprite file, 24 places in all, and at start-up for `CONFONT.CSF`
(`load_sprites("CONFONT.CSF", 1)`, kept in `confont`).

## Parameters

- `name`: the file or entry name.
- `from_archive`: 0 to read a disk file, otherwise the open archive.
- `index`: a directory index, for `load_sprites_at`.

## Inputs

The open archive (RULE-RES-001).

## Procedure

```text
define load_sprites(name: char[], from_archive: INT32) -> sprite_set:
    let b: UINT8[] = []
    if from_archive == 0:
        # a missing file stops the game with "CSF files does not exist"
        b = read_file(name, FMT-MEDIA-001)
    else:
        let e = find_entry(name)
        # a missing entry stops the game with "CSF files does not exist in GOB"
        read_entry(e, b)
    return make_sprite_set(b)

define load_sprites_at(index: UINT32) -> sprite_set:
    let b: UINT8[] = []
    read_entry(entry_at(index), b)
    return make_sprite_set(b)

define make_sprite_set(b: UINT8[]) -> sprite_set:
    let f: FMT-MEDIA-001 = b
    if f.version != "2J":
        # the game stops with "CSF is incorrect version"
        return 0
    let s: sprite_set
    s.count = f.count
    s.data = b
    let p = 6 + 4 * f.count
    for i in 0..f.count:
        append(s.frames, p)
        p = p + f.sizes[i]
    for i in 0..f.count:
        append(s.rows, build_row_table(b, s.frames[i]))
    return s

define build_row_table(b: UINT8[], start: UINT32) -> UINT32[]:
    let height = b[start + 2] + 256 * b[start + 3]
    let rows: UINT32[] = []
    let p = start + 4
    for r in 0..height:
        append(rows, p)
        let n = b[p]
        p = p + 1
        for k in 0..n:
            let op = b[p]
            # the length is an INT16LE
            let length = b[p + 1] + 256 * b[p + 2]
            p = p + 3
            if op == 0:
                p = p + length
            else if op == 2:
                p = p + 1
    return rows
```

## Outputs

The `sprite_set` record. Every allocation failure stops the game with an error message.

## Edge cases

- Frame offsets come from the size table only; a size that does not match the frame's rows goes
  unnoticed until the frame is drawn.
- No shipped CSF has a row with 0 segments.

## What the sources say

The executable (FND-MEDIA-001); the shipped files (FND-MEDIA-002).

## Differences between builds

None known.

## Open questions

None.
