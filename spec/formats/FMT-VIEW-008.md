---
id: FMT-VIEW-008
title: Scene texture, a TEX resource
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: null
text: false
definition: fmt_view_008.ksy
evidence: [FND-VIEW-018, FND-VIEW-010]
conflicting: []
split_with: []
related: []
---

## Layout

A resource named `TEXnnn width height`, with `width` and `height` taken from its name (FND-VIEW-018).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `width * height` | `UINT8[]` | `pixels` | Palette index of each pixel; 0 is transparent (FND-VIEW-010). | supported | FND-VIEW-010, FND-VIEW-018 |
| | | | | Total size `width * height` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against the size of all 12,982 `TEX` resources in the 99 archives.

## Open questions

- The order of the pixels, row by row or column by column, is not recorded.
