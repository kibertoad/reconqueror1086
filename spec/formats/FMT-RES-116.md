---
id: FMT-RES-116
title: Leading raw-track record in the owned disc carrier
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["game.gog"]
byte_order: little
size: 2352
text: false
definition: fmt_res_116.ksy
evidence: [FND-RES-012]
conflicting: []
split_with: []
related: []
---

## Layout

Each leading record has the following stored regions. Mode-1-shaped records and
entirely-zero records both fit these widths; the latter are an observed padding
run, not an ordinary Mode 1 header. The three address bytes are retained as
bytes rather than used to find physical file position. All fields are byte
vectors or single-byte values, so this layout makes no multibyte endian claim.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0` | 12 | `BYTE[12]` | `raw_sync` | Stored prefix: zero, ten FF bytes, zero in framed records; twelve zero bytes in the complete-zero run. | supported | FND-RES-012 |
| `12` | 3 | `BYTE[3]` | `raw_position` | Stored packed-decimal address components in framed records; all zero in the complete-zero run. The duplicate run repeats earlier addresses. | supported | FND-RES-012 |
| `15` | 1 | `BYTE` | `raw_mode` | Stored mode byte, 1 in framed records and 0 in the complete-zero run. | supported | FND-RES-012 |
| `16` | 2048 | `BYTE[2048]` | `raw_payload` | Region used by physical-indexed ISO logical reads; the post-volume records have zero bytes here. | supported | FND-RES-012 |
| `2064` | 288 | `BYTE[288]` | `raw_trailer` | Remaining stored bytes of the record; validation roles are unread. Bytes at record offsets 2068 through 2075 are zero in the owned leading span. | supported | FND-RES-012 |
| | | | | Total size 2352 | | |

## Enumerations and flags

### raw_mode

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0 | RAW_BLANK | Observed mode byte of the entirely-zero padding records; no general Mode 0 interpretation is asserted. | supported | FND-RES-012 |
| 1 | RAW_MODE_1 | Observed mode byte of the framed leading records. | supported | FND-RES-012 |

## Differences between builds

None known.

## Coverage

All 244,445 records before the first audio track of BLD-GOG-EN were inspected.
The scan included the complete-zero run and address-repeating duplicate block
[FND-RES-012]. It does not describe records in the trailing audio span or imply
that the wrapper validates every stored byte.

## Open questions

- Which parts of the stored trailer does the shipped wrapper validate?
  (Q-RES-122)
- Does the shipped wrapper use stored address components as physical locators
  rather than selecting records by file position? (Q-RES-123)
