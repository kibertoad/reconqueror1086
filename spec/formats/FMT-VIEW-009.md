---
id: FMT-VIEW-009
title: Combat palette, 256 colours of three bytes
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/SKIRMISH.RES"]
byte_order: little
size: 768
text: false
definition: fmt_view_009.ksy
evidence: [FND-VIEW-017]
conflicting: []
split_with: []
related: []
---

## Layout

The `SKIRMISH.PAL` entry of `CD:CONQUER/SKIRMISH.RES`: 256 colours in palette order.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 768 | `UINT8[768]` | `rgb` | Red, green and blue of each colour, colour `c` at `3 * c`. | supported | FND-VIEW-017 |
| `0x300` | | | | Total size 768 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against `SKIRMISH.PAL` through RULE-VIEW-006, which reproduces the stored maps with it.

## Open questions

- The range of the channel values the game sends to the display is not recorded.
