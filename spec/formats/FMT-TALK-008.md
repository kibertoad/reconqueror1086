---
id: FMT-TALK-008
title: Conversation variables, ALL.VTB
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_talk_008.ksy
evidence: [FND-TALK-006]
conflicting: []
split_with: []
related: [RULE-TALK-003]
---

## Layout

The `ALL.VTB` entry of `C1086.GOB`, the starting values of the conversation variables.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `count` | Number of variables. | supported | FND-TALK-006 |
| `0x04` | 4 | `INT32LE` | `element_size` | Bytes per variable. | supported | FND-TALK-006 |
| `0x08` | 4 | `INT32LE` | `element_kind` | See `element_kind`. | supported | FND-TALK-006 |
| `0x0C` | `count * element_size` | `BYTE[]` | `values` | The variables, numbered from 0. | supported | FND-TALK-006 |
| | | | | Total size `12 + count * element_size` | | |

## Enumerations and flags

### `element_kind`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0, 1 | `ELEMENT_8` | Reads and writes copy 1 byte. | supported | FND-TALK-006 |
| 2, 3 | `ELEMENT_16` | Reads and writes copy 2 bytes. | supported | FND-TALK-006 |
| 4, 5 | `ELEMENT_32` | Reads and writes copy 4 bytes. | supported | FND-TALK-006 |

## Differences between builds

None known.

## Coverage

Read against the loader `0x00062610`; the GOG archive holds 190 variables of 4 bytes, kind 5 [FND-TALK-006].

## Open questions

- What separates the kinds of each pair; the copy is the same for both.
