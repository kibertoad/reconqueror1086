---
id: FMT-VIEW-003
title: Scene start position, the Viewer resource
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: 108
text: false
definition: fmt_view_003.ksy
evidence: [FND-VIEW-012]
conflicting: []
split_with: []
related: []
---

## Layout

The `Viewer` resource of a scene archive.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `x` | Start x in 8.8 map units. | supported | FND-VIEW-012 |
| `0x04` | 4 | `INT32LE` | `y` | Start y in 8.8 map units. | supported | FND-VIEW-012 |
| `0x08` | 4 | `INT32LE` | `elevation` | Start eye height. | supported | FND-VIEW-012 |
| `0x0C` | 4 | `INT32LE` | `heading` | Start heading; one turn is `0x10000`. | supported | FND-VIEW-012 |
| `0x10` | 92 | `BYTE[92]` | `unk_10` | Purpose unknown. | supported | FND-VIEW-012 |
| `0x6C` | | | | Total size 108 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against the `Viewer` resource of the 42 `MELEE*` and `DEFEND*` archives: size, and x and y on a floor cell.

## Open questions

- How the 16-bit `heading` becomes the byte heading the view uses is not recorded.
- The purpose of `unk_10` is unknown.
