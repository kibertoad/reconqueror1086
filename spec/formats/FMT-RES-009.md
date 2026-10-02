---
id: FMT-RES-009
title: Owned autoplay indexed bitmap layout
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:AUTOPLAY.BMP"]
byte_order: little
size: 308278
text: false
definition: fmt_res_009.ksy
evidence: [FND-RES-015]
conflicting: []
split_with: []
related: []
---

## Layout

The listed owned file has a 14-byte bitmap file header, a 40-byte information
header, an indexed palette and an uncompressed row region. Field roles follow
SRC-BMP-REFERENCE and the complete stored values and boundaries are FND-RES-015.
This is the observed file case, not the consumer's accepted bitmap language.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| 0 | 2 | BYTE[2] | bmp_signature | BM file marker. | supported | FND-RES-015 |
| 2 | 4 | UINT32 | bmp_file_size | Total stored file size, 308278. | supported | FND-RES-015 |
| 6 | 2 | UINT16 | bmp_reserved_1 | Reserved word, zero. | supported | FND-RES-015 |
| 8 | 2 | UINT16 | bmp_reserved_2 | Reserved word, zero. | supported | FND-RES-015 |
| 10 | 4 | UINT32 | bmp_pixel_offset | Pixel-region file offset, 1078. | supported | FND-RES-015 |
| 14 | 4 | UINT32 | bmp_info_size | Information-header byte count, 40. | supported | FND-RES-015 |
| 18 | 4 | INT32 | bmp_width | Width in pixels, 640. | supported | FND-RES-015 |
| 22 | 4 | INT32 | bmp_height | Positive height, 480; documented bottom-up storage. | supported | FND-RES-015 |
| 26 | 2 | UINT16 | bmp_planes | Plane count, 1. | supported | FND-RES-015 |
| 28 | 2 | UINT16 | bmp_bit_count | Bits per pixel, 8. | supported | FND-RES-015 |
| 30 | 4 | UINT32 | bmp_compression | Stored compression value, zero. | supported | FND-RES-015 |
| 34 | 4 | UINT32 | bmp_image_size | Stored pixel-region byte count, 307200. | supported | FND-RES-015 |
| 38 | 4 | INT32 | bmp_x_resolution | Horizontal resolution field, zero. | supported | FND-RES-015 |
| 42 | 4 | INT32 | bmp_y_resolution | Vertical resolution field, zero. | supported | FND-RES-015 |
| 46 | 4 | UINT32 | bmp_palette_count | Palette entry count, 256. | supported | FND-RES-015 |
| 50 | 4 | UINT32 | bmp_important_count | Important-color count field, zero. | supported | FND-RES-015 |
| 54 | 1024 | BYTE[1024] | bmp_palette | 256 four-byte blue/green/red/reserved entries; reserved bytes zero. | supported | FND-RES-015 |
| 1078 | 307200 | BYTE[307200] | bmp_pixels | 480 stored rows of 640 indexed bytes, no padding at this width. | supported | FND-RES-015 |

All multibyte fields are little-endian. The palette's byte order is blue, green,
red, reserved. Pixel bytes select entries in that palette. Positive height
encodes bottom-up row order under SRC-BMP-REFERENCE. The aligned stride is
640 for this width and bit depth, so there is no stored row padding. Wider
bitmap variants, zero image-length conventions and alternative dimensions are
outside this entry's direct file-data coverage.

## Enumerations and flags

Compression zero denotes the uncompressed case in SRC-BMP-REFERENCE. No other
compression mode occurs in the owned file, and no consumer branch is claimed.

## Differences between builds

None known.

## Coverage

FND-RES-015 checks the whole identified file, every palette entry and every row,
including exact final consumption and byte reconstruction. Palette values and
artwork are not retained in the spec. No shipped bitmap reader, image placement
or original executable behavior has been completely read or run.

## Open questions

- Which shipped consumer reads this file, and is that path reachable?
  File recognition alone does not settle runtime use. (Q-RES-135)
- Which header and extent constraints does that consumer actually check, and
  what happens on malformed input? File bounds do not prove its rejection policy.
  (Q-RES-136)
- How does that consumer position and orient the decoded image on screen?
  The declared storage convention does not establish visible placement.
  (Q-RES-137)
