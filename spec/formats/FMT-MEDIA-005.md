---
id: FMT-MEDIA-005
title: Cached skirmish screen, a raw picture entry
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/SKIRMISH.RES"]
byte_order: little
size: 64000
text: false
definition: fmt_media_005.ksy
evidence: [FND-MEDIA-007]
conflicting: []
split_with: []
related: [RULE-MEDIA-004]
---

## Layout

A `.PCX` entry of `SKIRMISH.RES` other than `FNT6.PCX`: the pixels RULE-MEDIA-004 decoded from a PCX
file, with no header.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 64,000 | `UINT8[64000]` | `pixels` | Palette indices, 320 per row, 200 rows, top row first. | supported | FND-MEDIA-007 |
| `0xFA00` | | | | Total size 64,000 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The eight entries `MELEE2`, `SK_START`, `BRAWL`, `SKIRMCUR`, `SKIRMISH`, `HELP`, `EXIT` and
`STAIRS`, kind 1, each decoding to 64,000 bytes [FND-MEDIA-007]. `SKIRMISH.PCX` drawn with
`SKIRMISH.PAL` gives a coherent 320 by 200 screen.

## Open questions

- The layout of the 9,216-byte `FNT6.PCX`.
