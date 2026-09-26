---
id: FMT-SAVE-006
title: Armies, ~~4.SAV
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 3200
text: false
definition: fmt_save_006.ksy
evidence: [FND-SAVE-004, FND-ESTATE-002]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003]
---

## Layout

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 3200 | `BYTE[640][5]` | `armies` | The five army records of `army_records`, whole. | supported | FND-SAVE-004, FND-ESTATE-002 |
| `0xC80` | | | | Total size 3200 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked against a save from the original; the layout comes from the code that writes and reads it.

## Open questions

None.
