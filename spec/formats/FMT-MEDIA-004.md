---
id: FMT-MEDIA-004
title: Stored palette, a PAL entry
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/SKIRMISH.RES"]
byte_order: little
size: 768
text: false
definition: fmt_media_004.ksy
evidence: [FND-MEDIA-007]
conflicting: []
split_with: []
related: [RULE-MEDIA-004]
---

## Layout

A `.PAL` entry of `SKIRMISH.RES`, written and read by RULE-MEDIA-004 next to the picture of the same
name. FMT-VIEW-009 is the same layout for `SKIRMISH.PAL`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 768 | `UINT8[768]` | `rgb` | Red, green and blue of colour `c` at `3 * c`, the last 768 bytes of the source PCX. | supported | FND-MEDIA-007 |
| `0x300` | | | | Total size 768 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The five `.PAL` entries of `SKIRMISH.RES`: `MELEE2`, `SK_START`, `BRAWL`, `SKIRMISH` and `HELP`,
kind 0, 768 bytes each, with largest component values of 252 to 255 [FND-MEDIA-007].

## Open questions

None.
