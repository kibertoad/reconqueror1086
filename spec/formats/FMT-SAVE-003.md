---
id: FMT-SAVE-003
title: Properties, persons, items and variables, PROPERTY.SAV
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 2638
text: false
definition: fmt_save_003.ksy
evidence: [FND-SAVE-004, FND-SAVE-005]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003]
---

## Layout

Written to `property.sav` in the current directory and stored in FMT-SAVE-001. The conversation
variables go to a separate file, `vtsave.vtb` (FMT-TALK-008).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `fallback_person` | The global of that name. | supported | FND-SAVE-004 |
| `0x04` | 4 | `INT32LE` | `home_row` | The global of that name. | supported | FND-SAVE-004 |
| `0x08` | 4 | `INT32LE` | `home_col` | The global of that name. | supported | FND-SAVE-004 |
| `0x0C` | 4 | `INT32LE` | `unk_B718` | The global at `0x0009B718`; a new game sets it to 0. | supported | FND-SAVE-004, FND-SAVE-005 |
| `0x10` | 4 | `INT32LE` | `unk_B71C` | The global at `0x0009B71C`; purpose unknown. | supported | FND-SAVE-004 |
| `0x14` | 4 | `INT32LE` | `start_home` | The global of that name. | supported | FND-SAVE-004 |
| `0x18` | 4 | `INT32LE` | `place_person` | The global of that name. | supported | FND-SAVE-004 |
| `0x1C` | 4 | `INT32LE` | `property_list_head` | The global of that name. | supported | FND-SAVE-004 |
| `0x20` | 4 | `INT32LE` | `person_list_head` | The global of that name. | supported | FND-SAVE-004 |
| `0x24` | 280 | `INT32LE[70]` | `item_counts` | The count of each item. | supported | FND-SAVE-004 |
| `0x13C` | 210 | `FMT-STRATEGY-002[14]` | `properties` | The 14 property records whole. | supported | FND-SAVE-004 |
| `0x20E` | 2112 | `BYTE[12][176]` | `person_bytes` | For each person record: its bytes `+0x06` to `+0x09`, then `+0x07` to `+0x0A`, then `+0x0D` to `+0x10`. Loaded in the same order, so bytes 6 to 10 and 13 to 16 are restored. | supported | FND-SAVE-004 |
| `0xA4E` | | | | Total size 2638 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked against a save from the original; the layout comes from the code that writes and reads it.

## Open questions

None.
