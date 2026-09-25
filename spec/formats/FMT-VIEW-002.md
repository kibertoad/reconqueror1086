---
id: FMT-VIEW-002
title: Scene map, the Map resource
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: 32768
text: false
definition: fmt_view_002.ksy
evidence: [FND-VIEW-002, FND-VIEW-007, FND-VIEW-020]
conflicting: []
split_with: []
related: []
---

## Layout

The `Map` resource of a scene archive; the game keeps the live map in memory in the same layout
and changes cells as actors move and objects are used.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 32768 | `UINT16LE[16384]` | `cells` | Block number (index into FMT-VIEW-001 records) of each cell, cell `(x, y)` at index `x * 128 + y`. | supported | FND-VIEW-002, FND-VIEW-007 |
| `0x8000` | | | | Total size 32768 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against the `Map` resource of all 42 `MELEE*` and `DEFEND*` archives. The maps also hold cells the `Viewer` cannot reach (FND-VIEW-020).

## Open questions

None known.
