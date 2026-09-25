---
id: FMT-STRATEGY-002
title: Property record, one castle of the strategic map
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER.EXE"]
byte_order: little
size: 15
text: false
definition: fmt_strategy_002.ksy
evidence: [FND-STRATEGY-003, FND-STRATEGY-005, FND-STRATEGY-015, FND-STRATEGY-016]
conflicting: []
split_with: []
related: []
---

## Layout

The executable's data holds the 14 records of `properties` with their starting values, and the game
changes them in place.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `state` | Matches the `assignment` of the persons that belong to the property; a property whose state is 0 is skipped by the hostile routines. | supported | FND-STRATEGY-003, FND-STRATEGY-015 |
| `0x01` | 2 | `UINT16LE` | `cell_row` | Row of the property's terrain cell. | supported | FND-STRATEGY-015 |
| `0x03` | 2 | `UINT16LE` | `cell_col` | Column of that cell. | supported | FND-STRATEGY-015 |
| `0x05` | 2 | `UINT16LE` | `map_x` | Position x in route units. | supported | FND-STRATEGY-005, FND-STRATEGY-015 |
| `0x07` | 2 | `UINT16LE` | `map_y` | Position y in route units. | supported | FND-STRATEGY-005, FND-STRATEGY-015 |
| `0x09` | 1 | `UINT8` | `lord` | Index in `persons` of the property's lord. | supported | FND-STRATEGY-015 |
| `0x0A` | 2 | `UINT16LE` | `next` | Link of the property list, `0xFF` at its end. | supported | FND-STRATEGY-015 |
| `0x0C` | 1 | `UINT8` | `garrison` | Troops held at the property. | supported | FND-STRATEGY-005, FND-STRATEGY-015 |
| `0x0D` | 1 | `UINT8` | `alerted` | 1 once a player army has come near. | supported | FND-STRATEGY-005 |
| `0x0E` | 1 | `UINT8` | `approached` | 1 once the approach warning has been shown. | supported | FND-STRATEGY-005 |
| `0x0F` | | | | Total size 15 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read against the helpers that read and write each field and the save routine.

## Open questions

- What `state` values other than 0 stand for is not recorded.
