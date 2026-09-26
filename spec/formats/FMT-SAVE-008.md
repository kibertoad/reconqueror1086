---
id: FMT-SAVE-008
title: Starting persons and properties, default.dat
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 1618
text: false
definition: fmt_save_008.ksy
evidence: [FND-SAVE-005]
conflicting: []
split_with: []
related: [RULE-SAVE-004]
---

## Layout

Written to `default.dat` in the current directory at every start and read when a new game starts.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 1408 | `BYTE[8][176]` | `person_bytes` | For each person record: its bytes `+0x06` to `+0x09`, then `+0x07` to `+0x0A`. | supported | FND-SAVE-005 |
| `0x580` | 210 | `FMT-STRATEGY-002[14]` | `properties` | The 14 property records whole. | supported | FND-SAVE-005 |
| `0x652` | | | | Total size 1618 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked against a save from the original; the layout comes from the code that writes and reads it.

## Open questions

None.
