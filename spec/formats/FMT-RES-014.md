---
id: FMT-RES-014
title: Owned disc-root indexed icon container layout
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:AUTOPLAY.ICO", "CD:CONQCFG.ICO", "CD:CONQUER.ICO"]
byte_order: little
size: null
text: false
definition: fmt_res_014.ksy
evidence: [FND-RES-020]
conflicting: []
split_with: []
related: []
---

## Layout

This describes the owned files, not the reader's accepted language. Field roles
follow SRC-ICO-REFERENCE and SRC-BMP-REFERENCE; complete values and extents are
FND-RES-020. No pixels or palette colors are reproduced.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| 0 | 2 | UINT16 | ico_reserved | Zero. | supported | FND-RES-020 |
| 2 | 2 | UINT16 | ico_type | Icon resource type, 1. | supported | FND-RES-020 |
| 4 | 2 | UINT16 | ico_count | Image count: 2 for AUTOPLAY, 1 for the others. | supported | FND-RES-020 |
| 6 | 16 * ico_count | BYTE[16 * ico_count] | ico_directory | Counted records described below. | supported | FND-RES-020 |
|  | file_size - 6 - 16 * ico_count | BYTE[] | ico_payload_storage | Adjacent image payloads through file end. | supported | FND-RES-020 |
| | | | | Total size variable | | |

### Directory record

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| 0 | 1 | BYTE | width | Stored width, 16 or 32. | supported | FND-RES-020 |
| 1 | 1 | BYTE | height | Stored height, 16 or 32. | supported | FND-RES-020 |
| 2 | 1 | BYTE | color_count | 16. | supported | FND-RES-020 |
| 3 | 1 | BYTE | reserved | Zero. | supported | FND-RES-020 |
| 4 | 2 | UINT16 | planes_hint | Zero in every record; actual payload plane count is 1. | supported | FND-RES-020 |
| 6 | 2 | UINT16 | bit_count_hint | Zero in every record; actual payload depth is 4. | supported | FND-RES-020 |
| 8 | 4 | UINT32 | image_length | Stored payload length, 296 or 744. | supported | FND-RES-020 |
| 12 | 4 | UINT32 | image_offset | File-relative payload start, as listed in FND-RES-020. | supported | FND-RES-020 |
| | | | | Total size 16 | | |

### Image payload

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| 0 | 4 | UINT32 | header_size | 40. | supported | FND-RES-020 |
| 4 | 4 | INT32 | width | 16 or 32. | supported | FND-RES-020 |
| 8 | 4 | INT32 | combined_height | 32 or 64, twice the displayed height. | supported | FND-RES-020 |
| 12 | 2 | UINT16 | planes | 1. | supported | FND-RES-020 |
| 14 | 2 | UINT16 | bit_count | 4. | supported | FND-RES-020 |
| 16 | 4 | UINT32 | compression | Zero. | supported | FND-RES-020 |
| 20 | 4 | UINT32 | image_size | Stored value 128, 512 or 640; not a uniform plane-length rule. | supported | FND-RES-020 |
| 24 | 4 | INT32 | x_resolution | Zero. | supported | FND-RES-020 |
| 28 | 4 | INT32 | y_resolution | Zero. | supported | FND-RES-020 |
| 32 | 4 | UINT32 | colors_used | Zero. | supported | FND-RES-020 |
| 36 | 4 | UINT32 | colors_important | Zero. | supported | FND-RES-020 |
| 40 | 64 | BYTE[64] | palette | 16 blue/green/red/reserved entries; reserved bytes zero. | supported | FND-RES-020 |
| 104 | xor_stride * height | BYTE[] | xor_plane | Four-bit indexed rows; high nibble first. | supported | FND-RES-020 |
|  | and_stride * height | BYTE[] | and_plane | One-bit rows; most significant bit first. | supported | FND-RES-020 |
| | | | | Total size 40 + 64 + xor_stride * height + and_stride * height | | |

Here height is combined_height / 2. The aligned strides are
xor_stride = ((width * 4 + 31) / 32 rounded down) * 4 and
and_stride = ((width + 31) / 32 rounded down) * 4. Positive stored height
has the documented bottom-up row interpretation. For 32 pixels, the planes
occupy 512 and 128 bytes; for 16 pixels, they occupy 128 and 64 bytes.
The directory hints remain zero and do not replace the payload fields.

## Enumerations and flags

None beyond the literal stored values above. No accepted variant set is claimed.

## Differences between builds

None known.

## Coverage

Complete traversal and reconstruction cover every listed file and payload
[FND-RES-020]. The definition describes that partition; Kaitai compilation has
not run because the compiler is unavailable. No original reader ran and no
rendering or consumer behavior is established.

## Open questions

- Which shipped consumer opens these files, and are those paths reachable?
  Filename-based shell use and application-directed use remain competing
  readings; the storage observation decides neither. (Q-RES-164)
- Does the consumer ignore image_size, use it as a length, or accommodate its
  variants? FND-RES-020 records differing values; read the actual length checks
  and copy inputs to distinguish these readings. (Q-RES-165)
- How does the consumer select images and interpret zero directory hints?
  Header-derived depth and external display-based selection both remain possible.
  Trace selection inputs and every fallback. (Q-RES-166)
- What does the consumer do with absent, truncated or inconsistent icon files?
  Rejection, fallback and partial acceptance remain possible until its failure
  paths are read. (Q-RES-167)
