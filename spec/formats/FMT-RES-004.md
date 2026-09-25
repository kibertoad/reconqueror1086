---
id: FMT-RES-004
title: Kind-2 stream, the stored bytes of a kind-2 entry
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB", "CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: big
size: null
text: false
definition: fmt_res_004.ksy
evidence: [FND-RES-004]
conflicting: []
split_with: []
related: [RULE-RES-003]
---

## Layout

The stored bytes of an entry of kind 2 are one bit stream of LZW codes, read from the most
significant bit of each byte down. Codes start 9 bits wide. RULE-RES-003 gives how the width grows.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `stored_size` | `BYTE[stored_size]` | `codes` | The code stream, ending with code 257 and padding. | supported | FND-RES-004 |
| | | | | Total size `stored_size` | | |

## Enumerations and flags

### codes

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0 to 255 | `LZW_LITERAL` | The byte of that value. | supported | FND-RES-004 |
| 256 | `LZW_CLEAR` | Empties the dictionary and resets the width to 9 bits. | supported | FND-RES-004 |
| 257 | `LZW_END` | Ends the stream. | supported | FND-RES-004 |
| 258 to 16,383 | `LZW_STRING` | A dictionary string. | supported | FND-RES-004 |

## Differences between builds

None known.

## Coverage

The five kind-2 entries of `C1086.GOB` [FND-RES-004]. Each reaches 14-bit codes and has 4 or 5
bytes after its end code.

## Open questions

None known.
