---
id: FMT-VIEW-005
title: Backdrop descriptor, the Backdrop resource
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: 24
text: false
definition: fmt_view_005.ksy
evidence: [FND-VIEW-014, FND-VIEW-015]
conflicting: []
split_with: []
related: []
---

## Layout

The `Backdrop` resource of a scene archive. It describes the panorama in the same archive's `BackImage` (FMT-VIEW-006).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `kind` | 1 in every archive; other values are not described. | supported | FND-VIEW-014 |
| `0x04` | 4 | `BYTE[4]` | `unk_04` | Differs between archives; purpose unknown. | supported | FND-VIEW-014 |
| `0x08` | 4 | `INT32LE` | `width` | Panorama width in pixels. | supported | FND-VIEW-014, FND-VIEW-015 |
| `0x0C` | 4 | `INT32LE` | `height` | Panorama height in pixels. | supported | FND-VIEW-014, FND-VIEW-015 |
| `0x10` | 4 | `INT32LE` | `horizon` | Image row that meets the view horizon. | supported | FND-VIEW-014, FND-VIEW-015 |
| `0x14` | 4 | `INT32LE` | `scale` | Image columns per heading unit. | supported | FND-VIEW-014, FND-VIEW-015 |
| `0x18` | | | | Total size 24 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against the `Backdrop` resource of the 42 `MELEE*` and `DEFEND*` archives: every one is 24 bytes with kind 1, width 1,088, height 200, horizon 199 and scale 3.

## Open questions

- The purpose of `unk_04`, and what other `kind` values would do, is unknown.
