---
id: FMT-RES-003
title: Kind-1 stream, the stored bytes of a kind-1 entry
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB", "CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: null
text: false
definition: fmt_res_003.ksy
evidence: [FND-RES-003, FND-RES-008]
conflicting: []
split_with: []
related: [RULE-RES-002]
---

## Layout

The stored bytes of an entry of kind 1 are blocks, one after another, until the stored size is
used up. RULE-RES-002 decodes them.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `length` | Number of bytes after this field that belong to the block. | supported | FND-RES-003 |
| `0x02` | 1 | `UINT8` | `marker` | `0x80` for a stored block; any other value, `0x40` in every shipped block, for a compressed block. | supported | FND-RES-003 |
| `0x03` | `length - 1` | `BYTE[length - 1]` | `body` | The block's output for a stored block; otherwise the compressed block below. | supported | FND-RES-003 |
| | | | | Total size `2 + length` | | |

A compressed block's `body`:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `unk_00` | Never read; 0 in 23,222 of the 32,060 compressed blocks. | supported | FND-RES-003 |
| `0x01` | 2 | `UINT16BE` | `control` | The first control word: bit 15 is the first token's flag. | supported | FND-RES-003 |
| `0x03` | `length - 4` | `BYTE[length - 4]` | `tokens` | Tokens, with a big-endian control word before each further group of 16. | supported | FND-RES-003 |
| | | | | Total size `length - 1` | | |

A token whose control bit is 0 is one literal byte. A token whose bit is 1 starts with two bytes `a`
and `b`. With `(a << 4) | (b >> 4)` above 0 it is a copy of `(b & 0x0F) + 3` bytes from that
distance back in the block's output, 2 bytes long. With the distance 0 it has two more bytes `c`
and `v` and writes `v` `(b << 8) + c + 16` times, 4 bytes long.

## Enumerations and flags

### marker

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0x40` | `BLOCK_COMPRESSED` | Compressed; the game takes every value but `0x80` this way. | supported | FND-RES-003 |
| `0x80` | `BLOCK_STORED` | The body is the output. | supported | FND-RES-003 |

## Differences between builds

None known.

## Coverage

All 23,201 kind-1 entries of the release, 32,091 blocks [FND-RES-003]. In every entry each block
but the last expands to 16,384 bytes, no copy reaches before its block, and no compressed block has
bytes after its last token. The game does not depend on those three properties.

## Open questions

None known.
