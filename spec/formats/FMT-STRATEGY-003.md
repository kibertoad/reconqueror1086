---
id: FMT-STRATEGY-003
title: Person record, one character of the strategic map
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER.EXE"]
byte_order: little
size: 18
text: false
definition: fmt_strategy_003.ksy
evidence: [FND-STRATEGY-003, FND-STRATEGY-007, FND-STRATEGY-008, FND-STRATEGY-015, FND-STRATEGY-021, FND-STRATEGY-023]
conflicting: []
split_with: []
related: []
---

## Layout

The executable's data holds the 176 records of `persons` with their starting values, and the game
changes them in place. Record 0 is never used.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `PTR32<BYTE>` | `name` | The person's name, a string. | supported | FND-STRATEGY-015 |
| `0x04` | 1 | `UINT8` | `group` | Index in `properties` of the property the person belongs to. | supported | FND-STRATEGY-007, FND-STRATEGY-015 |
| `0x05` | 1 | `UINT8` | `county` | Index of the county name. | supported | FND-STRATEGY-015 |
| `0x06` | 1 | `UINT8` | `flags` | Flags. | supported | FND-STRATEGY-015 |
| `0x06 bits 0..1` | | `bits[1]` | `eligible` | Set: the person counts in the household and can be met on the map. | supported | FND-STRATEGY-003, FND-STRATEGY-015 |
| `0x06 bits 1..8` | | `bits[7]` | `unk_06_1` | Purpose unknown. | supported | FND-STRATEGY-015 |
| `0x07` | 1 | `UINT8` | `assignment` | Compared with a property's `state`; 0 for a person with no seat. | supported | FND-STRATEGY-003, FND-STRATEGY-023 |
| `0x08` | 2 | `UINT16LE` | `cell_row` | Row of the person's terrain cell. | supported | FND-STRATEGY-007, FND-STRATEGY-023 |
| `0x0A` | 2 | `UINT16LE` | `cell_col` | Column of that cell. | supported | FND-STRATEGY-007, FND-STRATEGY-023 |
| `0x0C` | 1 | `UINT8` | `rating` | The lord rating that sizes the property's forces. | supported | FND-STRATEGY-008, FND-STRATEGY-021 |
| `0x0D` | 1 | `UINT8` | `next` | Link of the person list, `0xFF` at its end. | supported | FND-STRATEGY-008, FND-STRATEGY-015 |
| `0x0E` | 4 | `BYTE[4]` | `unk_0E` | Purpose unknown. | supported | FND-STRATEGY-015 |
| `0x12` | | | | Total size 18 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read against the person helpers and the save routine.

## Open questions

- The purpose of bit 1 of `flags` and of `unk_0E` is unknown.
- How `rating` changes apart from the two writes in the new-game setup and the encounter is not
  recorded.
