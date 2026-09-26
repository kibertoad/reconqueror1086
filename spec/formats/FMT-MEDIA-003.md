---
id: FMT-MEDIA-003
title: PCX picture as the game reads it
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_media_003.ksy
evidence: [FND-MEDIA-005, FND-MEDIA-006]
conflicting: []
split_with: []
related: [RULE-MEDIA-003]
---

## Layout

A decoded `.PCX` or `.PCC` entry of `C1086.GOB`. The header is the 128-byte header of ZSoft PCX;
the game reads only the fields marked as read.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `manufacturer` | `0x0A`; read and required. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| `0x01` | 1 | `UINT8` | `version` | 5; read and required. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| `0x02` | 1 | `UINT8` | `encoding` | 1 in every entry; not read. | supported | FND-MEDIA-006 |
| `0x03` | 1 | `UINT8` | `bits` | 8; read and required. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| `0x04` | 2 | `UINT16LE` | `xmin` | 0 in every entry; read. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| `0x06` | 2 | `UINT16LE` | `ymin` | 0 in every entry; read. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| `0x08` | 2 | `UINT16LE` | `xmax` | `width - 1`; read. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| `0x0A` | 2 | `UINT16LE` | `ymax` | `height - 1`; read. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| `0x0C` | 4 | `UINT16LE[2]` | `resolution` | Mostly 640 and 480; not read. | supported | FND-MEDIA-006 |
| `0x10` | 48 | `UINT8[48]` | `ega_palette` | Not read. | supported | FND-MEDIA-005 |
| `0x40` | 1 | `UINT8` | `reserved` | Not read. | supported | FND-MEDIA-005 |
| `0x41` | 1 | `UINT8` | `planes` | 1 in every entry; not read. | supported | FND-MEDIA-006 |
| `0x42` | 2 | `UINT16LE` | `bytes_per_line` | The width rounded up to even in every entry; not read. | supported | FND-MEDIA-006 |
| `0x44` | 2 | `UINT16LE` | `palette_type` | 0 or 1; not read. | supported | FND-MEDIA-006 |
| `0x46` | 58 | `UINT8[58]` | `filler` | Not read. | supported | FND-MEDIA-005 |
| `0x80` | variable | `BYTE[]` | `rows` | `ymax - ymin + 1` rows of RLE data, each `xmax - xmin + 1` pixels rounded up to even. A byte `b` with `b & 0xC0 == 0xC0` repeats the next byte `b & 0x3F` times; any other byte is one pixel. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| size - 769 | 1 | `UINT8` | `palette_marker` | `0x0C`; the palette is loaded only when it is. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| size - 768 | 768 | `UINT8[768]` | `palette` | 256 colours of red, green and blue. | supported | FND-MEDIA-005, FND-MEDIA-006 |
| | | | | Total size: the entry's expanded size | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 143 `.PCX` and 49 `.PCC` entries of `C1086.GOB` [FND-MEDIA-006]: their rows end exactly at the
palette marker, no run has a count of 0 or crosses a row end, and every odd-width picture stores one
padding byte per row, which the game draws.

## Open questions

- Whether the palette values are sent to the display as they are or scaled; the path from the
  palette buffer to the hardware was not traced.
