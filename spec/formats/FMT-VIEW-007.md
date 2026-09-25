---
id: FMT-VIEW-007
title: Colour map, one of the Pal0 to Pal127 resources
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: 256
text: false
definition: fmt_view_007.ksy
evidence: [FND-VIEW-016, FND-VIEW-017]
conflicting: []
split_with: []
related: []
---

## Layout

The resources `Pal0` to `Pal127` of a scene archive. Map `n` is the resource named `Pal` and `n` in decimal.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 256 | `UINT8[256]` | `index` | Palette index drawn for each source palette index. | supported | FND-VIEW-016, FND-VIEW-017 |
| `0x100` | | | | Total size 256 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against all 128 maps of the 42 `MELEE*` and `DEFEND*` archives, and maps 0 to 31 of all 90 archives with a `Scenario` against RULE-VIEW-006.

## Open questions

- Whether the game loads the stored maps or builds them with RULE-VIEW-006 when a scene starts is not recorded; the two agree for maps 0 to 31.
