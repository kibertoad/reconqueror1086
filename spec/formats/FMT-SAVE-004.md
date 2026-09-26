---
id: FMT-SAVE-004
title: Calendar block, ~~2.SAV
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 64
text: false
definition: fmt_save_004.ksy
evidence: [FND-SAVE-004, FND-UI-004, FND-STRATEGY-001]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003]
---

## Layout

The 64-byte block the pointer at `0x0009AE00` points to, written as it is. No file is written while
the pointer is null.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 24 | `BYTE[24]` | `unk_00` | Purpose unknown. | supported | FND-SAVE-004 |
| `0x18` | 4 | `INT32LE` | `month` | The zero-based month `current_month` returns. | supported | FND-STRATEGY-001 |
| `0x1C` | 4 | `INT32LE` | `day` | The calendar field `fn_00038678` returns. | supported | FND-UI-004 |
| `0x20` | 32 | `BYTE[32]` | `unk_20` | Purpose unknown. | supported | FND-SAVE-004 |
| `0x40` | | | | Total size 64 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked against a save from the original; the layout comes from the code that writes and reads it.

## Open questions

- What the rest of the block holds, the year included.
