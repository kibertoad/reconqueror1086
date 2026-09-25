---
id: FMT-RES-002
title: Container directory record
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB", "CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: 52
text: false
definition: fmt_res_002.ksy
evidence: [FND-RES-001, FND-RES-002, FND-RES-003, FND-RES-004, FND-RES-005]
conflicting: []
split_with: []
related: [RULE-RES-001]
---

## Layout

One record of the directory of FMT-RES-001. The game keeps the records in memory in this layout.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 32 | `char[32]` | `name` | Entry name, ASCII, NUL-terminated and padded with NUL; the writer keeps at most 31 characters. | supported | FND-RES-001, FND-RES-002, FND-RES-005 |
| `0x20` | 4 | `UINT32LE` | `kind` | How the entry is stored, see below. | supported | FND-RES-001, FND-RES-002 |
| `0x24` | 4 | `UINT32LE` | `unk_24` | Written from the writer's third argument; never read. 0 in every record. | supported | FND-RES-001, FND-RES-005 |
| `0x28` | 4 | `UINT32LE` | `stored_size` | Number of bytes the entry takes in `data`. | supported | FND-RES-001, FND-RES-002 |
| `0x2C` | 4 | `UINT32LE` | `expanded_size` | Number of bytes the entry decodes to; the reader returns it. | supported | FND-RES-001, FND-RES-002 |
| `0x30` | 4 | `UINT32LE` | `offset` | File offset of the entry's stored bytes. | supported | FND-RES-001, FND-RES-002 |
| `0x34` | | | | Total size 52 | | |

## Enumerations and flags

### kind

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0 | `STORAGE_PLAIN` | The stored bytes are the entry. | supported | FND-RES-002 |
| 1 | `STORAGE_BLOCKS` | A kind-1 stream, FMT-RES-003. | supported | FND-RES-002, FND-RES-003 |
| 2 | `STORAGE_LZW` | A kind-2 stream, FMT-RES-004. | supported | FND-RES-002, FND-RES-004 |
| 3 | `STORAGE_KIND_3` | A third codec the game can read and write; no shipped entry uses it. | supported | FND-RES-002, FND-RES-005 |

## Differences between builds

None known.

## Coverage

All 26,762 records of the release [FND-RES-001]: 3,556 have kind 0, 23,201 kind 1 and 5 kind 2.
Kind 0 records have equal sizes. The two `Pal102` records of `BAR0` have kind 1 and equal sizes, so
equal sizes do not mean kind 0.

## Open questions

- What `unk_24` was meant to hold.
- The format of kind 3.
