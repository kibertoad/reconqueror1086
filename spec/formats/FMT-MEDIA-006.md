---
id: FMT-MEDIA-006
title: Smacker 2 movie header
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.SMK"]
byte_order: little
size: null
text: false
definition: fmt_media_006.ksy
evidence: [FND-MEDIA-008, FND-MEDIA-009]
conflicting: []
split_with: []
related: [RULE-MEDIA-005]
---

## Layout

The start of a `.SMK` file, the published Smacker layout. The game reads it through the linked
Smacker library, whose movie object keeps the frame count at `+0x0C` and the current frame at
`+0x678` [FND-MEDIA-009].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `signature` | `SMK2` in every file. | supported | FND-MEDIA-008 |
| `0x04` | 4 | `UINT32LE` | `width` | Width in pixels. | supported | FND-MEDIA-008, FND-MEDIA-009 |
| `0x08` | 4 | `UINT32LE` | `height` | Height in pixels. | supported | FND-MEDIA-008, FND-MEDIA-009 |
| `0x0C` | 4 | `UINT32LE` | `frame_count` | Number of frames. | supported | FND-MEDIA-008, FND-MEDIA-009 |
| `0x10` | 4 | `INT32LE` | `frame_rate` | Above 0: milliseconds per frame; below 0: tens of microseconds, negated; 0: 100 ms. | supported | FND-MEDIA-008 |
| `0x14` | 4 | `UINT32LE` | `flags` | Bit 0 adds a ring frame. | supported | FND-MEDIA-008 |
| `0x18` | 28 | `UINT32LE[7]` | `audio_size` | Largest decoded audio size of each of seven tracks. | supported | FND-MEDIA-008 |
| `0x34` | 4 | `UINT32LE` | `trees_size` | Bytes of tree data. | supported | FND-MEDIA-008 |
| `0x38` | 16 | `UINT32LE[4]` | `tree_sizes` | Decoded sizes of the four video trees. | supported | FND-MEDIA-008 |
| `0x48` | 28 | `UINT32LE[7]` | `audio_rate` | Sample rate in the low 24 bits and track flags in the top byte, per track. | supported | FND-MEDIA-008 |
| `0x64` | 4 | `UINT32LE` | `reserved` | Not used. | supported | FND-MEDIA-008 |
| `0x68` | `frame_count * 4` | `UINT32LE[frame_count]` | `frame_sizes` | Size of each frame; the low two bits are flags. | supported | FND-MEDIA-008 |
| after `frame_sizes` | `frame_count` | `UINT8[frame_count]` | `frame_types` | Bit 0: the frame starts with a palette; bits 1 to 7: it has audio for tracks 0 to 6. | supported | FND-MEDIA-008 |
| after `frame_types` | `trees_size` | `BYTE[]` | `trees` | Packed Huffman trees. | supported | FND-MEDIA-008 |
| after `trees` | sum of frame sizes | `BYTE[]` | `frames` | Frame data, ending at the end of the file. | supported | FND-MEDIA-008 |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 2,131 movies of the disc image [FND-MEDIA-008]: every one is `SMK2`, and its tables and frames
end at the end of the file.

## Open questions

None.
