---
id: FMT-SAVE-007
title: Tournament, ~~5.SAV
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 24
text: false
definition: fmt_save_007.ksy
evidence: [FND-SAVE-004, FND-TOURNEY-001, FND-TOURNEY-003]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003]
---

## Layout

No file is written while the pointer at `0x0009DC30` is null.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `tournament_site` | The site of this month's tournament. | supported | FND-TOURNEY-001 |
| `0x04` | 4 | `INT32LE` | `tournament_place` | Its place. | supported | FND-TOURNEY-001 |
| `0x08` | 4 | `INT32LE` | `tournament_arrival_day` | The day the player arrived. | supported | FND-TOURNEY-001 |
| `0x0C` | 4 | `INT32LE` | `jousts_today` | The global of that name. | supported | FND-TOURNEY-003 |
| `0x10` | 4 | `INT32LE` | `melees_today` | The global of that name. | supported | FND-TOURNEY-003 |
| `0x14` | 4 | `INT32LE` | `tournament_wins` | The global of that name. | supported | FND-TOURNEY-003 |
| `0x18` | | | | Total size 24 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked against a save from the original; the layout comes from the code that writes and reads it.

## Open questions

None.
