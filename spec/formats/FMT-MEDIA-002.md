---
id: FMT-MEDIA-002
title: CSF frame
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB", "CD:CONQUER/SKIRMISH.RES"]
byte_order: little
size: null
text: false
definition: fmt_media_002.ksy
evidence: [FND-MEDIA-001, FND-MEDIA-002, FND-MEDIA-003, FND-MEDIA-004]
conflicting: []
split_with: []
related: [RULE-MEDIA-001, RULE-MEDIA-002]
---

## Layout

One frame of FMT-MEDIA-001.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `width` | Width in pixels; the text routine reads it as signed to advance x. | supported | FND-MEDIA-002, FND-MEDIA-004 |
| `0x02` | 2 | `UINT16LE` | `height` | Number of rows. | supported | FND-MEDIA-001, FND-MEDIA-002 |
| `0x04` | variable | `row[height]` | `rows` | The rows, top first, each laid out as below. | supported | FND-MEDIA-001, FND-MEDIA-002 |
| row `+0x00` | 1 | `UINT8` | `segment_count` | Number of segments in the row. | supported | FND-MEDIA-001, FND-MEDIA-002 |
| row `+0x01` | variable | `segment[segment_count]` | `segments` | The segments, left first, each laid out as below. | supported | FND-MEDIA-002 |
| segment `+0x00` | 1 | `UINT8` | `op` | What the segment does, see below. | supported | FND-MEDIA-002, FND-MEDIA-003 |
| segment `+0x01` | 2 | `INT16LE` | `length` | Number of pixels the segment covers. | supported | FND-MEDIA-002, FND-MEDIA-003 |
| segment `+0x03` | `length`, 1 or 0 | `UINT8[]` | `pixels` | `length` pixels when `op` is 0, one pixel when `op` is 2, nothing otherwise. | supported | FND-MEDIA-001, FND-MEDIA-002 |
| | | | | Total size: the frame's entry in `sizes` | | |

The segment lengths of a row add up to `width` in every shipped frame; the game does not check this.

## Enumerations and flags

### op

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0 | `SEGMENT_LITERAL` | `length` pixels follow and are drawn as they are, or in the caller's colour by the mask routine. | supported | FND-MEDIA-003 |
| 1 | `SEGMENT_SKIP` | `length` pixels are left as they are. | supported | FND-MEDIA-003 |
| 2 | `SEGMENT_FILL` | One pixel follows and is drawn `length` times, or the caller's colour by the mask routine. | supported | FND-MEDIA-003 |

## Differences between builds

None known.

## Coverage

All 3,262 frames [FND-MEDIA-002]: 267,374 rows with 1 to 31 segments each, 225,692 literal, 415,414
skip and 50,293 fill segments, all with lengths above 0. The game treats any operation but 0 and 2 as
a skip.

## Open questions

None.
