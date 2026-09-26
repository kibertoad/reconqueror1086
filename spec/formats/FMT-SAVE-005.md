---
id: FMT-SAVE-005
title: Home fief, ~~3.SAV
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: null
text: false
definition: fmt_save_005.ksy
evidence: [FND-SAVE-004, FND-SAVE-005, FND-ESTATE-003]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003]
---

## Layout

The estate state reached through the pointer at `0x0009ABEC`. No file is written while the pointer
is null. Only the first fief of the list is written.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `unk_ABE8` | The global at `0x0009ABE8`; a new game sets it to 0. | supported | FND-SAVE-004, FND-SAVE-005 |
| `0x04` | 12 | `BYTE[12]` | `estate_block` | The block the pointer points to; its first dword sizes the fief pointer list on load, and `+0x08` is that list's address. | supported | FND-SAVE-004 |
| `0x10` | 56 | `BYTE[56]` | `home_fief` | The first fief record (`fiefs`), with stale pointers in `list_28` to `list_34`. | supported | FND-SAVE-004, FND-ESTATE-003 |
| `0x48` | 628 | `BYTE[628]` | `list_28` | 19 rows of 8 `INT32LE` and 20 more bytes. | supported | FND-SAVE-004 |
| `0x2BC` | 688 | `BYTE[688]` | `list_2C` | 15 rows of 11 `INT32LE` and 28 more bytes. | supported | FND-SAVE-004 |
| `0x56C` | 416 | `BYTE[416]` | `list_34` | 10 rows of 10 `INT32LE` and 16 more bytes. | supported | FND-SAVE-004 |
| `0x70C` | 296 | `BYTE[296]` | `list_30` | 9 rows of 8 `INT32LE` and 8 more bytes. | supported | FND-SAVE-004 |
| `0x834` | variable | `BYTE[12][]` | `chains` | For each row of `list_28`, `list_2C`, `list_34` and `list_30` in that order, the row's chain of 12-byte nodes, as many as the row's length field: `+0x1C` in `list_28`, `+0x24` in `list_2C`, `+0x18` in `list_34`, `+0x10` in `list_30`. | supported | FND-SAVE-004 |
| | | | | Total size variable | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked against a save from the original; the layout comes from the code that writes and reads it.

## Open questions

- What the nodes of each chain are.
