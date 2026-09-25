---
id: FMT-VIEW-006
title: Backdrop pixels, the BackImage resource
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: null
text: false
definition: fmt_view_006.ksy
evidence: [FND-VIEW-014]
conflicting: []
split_with: []
related: []
---

## Layout

The `BackImage` resource of a scene archive: `width * height` palette indices, row by row, with `width` and `height` from the same archive's `Backdrop` (FMT-VIEW-005).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `width * height` | `UINT8[]` | `pixels` | Palette index of each pixel, pixel `(x, y)` at `y * width + x`. | supported | FND-VIEW-014 |
| | | | | Total size `width * height` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against the `BackImage` resource of the 42 `MELEE*` and `DEFEND*` archives: each is `1088 * 200` bytes.

## Open questions

None known.
