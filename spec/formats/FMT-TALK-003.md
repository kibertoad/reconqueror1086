---
id: FMT-TALK-003
title: Action index, ALL.TMI
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_talk_003.ksy
evidence: [FND-TALK-004]
conflicting: []
split_with: []
related: [RULE-TALK-002]
---

## Layout

The `ALL.TMI` entry of `C1086.GOB`: a dword, then 8-byte records to the end of the file, searched from the first in file order.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `unk_00` | Skipped by the lookup. | supported | FND-TALK-004 |
| `0x04` | 8 each | | `records` | To the end of the file. | supported | FND-TALK-004 |
| `+0x00` | 4 | `INT32LE` | `records[i].action` | The action number a node names. | supported | FND-TALK-004 |
| `+0x04` | 4 | `INT32LE` | `records[i].offset` | Offset in `ALL.TMB` of the action's FMT-TALK-004 group. | supported | FND-TALK-004 |
| | | | | Total size `4 + 8 * count` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read against the lookup at `0x0006A950`; the GOG archive holds 689 records.

## Open questions

- What `unk_00` holds.
