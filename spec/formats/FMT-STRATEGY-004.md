---
id: FMT-STRATEGY-004
title: Route file, a list of route points
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: 4 + point_count * 8
text: false
definition: fmt_strategy_004.ksy
evidence: [FND-STRATEGY-007, FND-STRATEGY-016, FND-STRATEGY-032]
conflicting: []
split_with: []
related: []
---

## Layout

The `rt_*.rat`, `sc_*.rat`, `br_*.rat`, `scot.rat` and `wales.rat` entries of `C1086.GOB`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `point_count` | Number of points. | supported | FND-STRATEGY-016 |
| `0x04` | `point_count * 8` | `INT32LE[]` | `points` | The points in walking order, each an x and a y in route units. | supported | FND-STRATEGY-016 |
| | | | | Total size `4 + point_count * 8` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 116 route entries of the GOG archive: each is exactly `4 + 8 * point_count` bytes.

## Open questions

None known.
