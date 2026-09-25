---
id: FMT-UI-002
title: Screen region, one record of a HAT file
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: 24
text: false
definition: fmt_ui_002.ksy
evidence: [FND-UI-002, FND-UI-003, FND-UI-004]
conflicting: []
split_with: []
related: []
---

## Layout

One region record of FMT-UI-001. The loader keeps x, y, width and height as 16-bit values.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `id` | Region number, kept with the region; the callbacks are bound by position. | supported | FND-UI-002, FND-UI-003 |
| `0x04` | 4 | `INT32LE` | `x` | Left edge in screen pixels. | supported | FND-UI-002, FND-UI-004 |
| `0x08` | 4 | `INT32LE` | `y` | Top edge. | supported | FND-UI-002, FND-UI-004 |
| `0x0C` | 4 | `INT32LE` | `width` | Width; some regions reach past the 640 by 480 screen. | supported | FND-UI-002, FND-UI-004 |
| `0x10` | 4 | `INT32LE` | `height` | Height. | supported | FND-UI-002, FND-UI-004 |
| `0x14` | 4 | `INT32LE` | `enabled` | 1 for an active region, 0 for one the screen turns on later. | supported | FND-UI-002, FND-UI-004 |
| `0x18` | | | | Total size 24 | | |

## Enumerations and flags

### enabled

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0 | `OFF` | Inactive. `0x00059D34` stores this in a region the current state does not use. | supported | FND-UI-002, FND-UI-004 |
| 1 | `ON` | Active. | supported | FND-UI-002, FND-UI-004 |

## Differences between builds

None known.

## Coverage

All 211 records of the 27 HAT entries of the GOG archive. Region 11 of `GAMEOPTS.HAT` and regions 3
to 6 of `VSMITH.HAT` are 0; the rest are 1. The ids of `CHARGEN.HAT` are out of order [FND-UI-002].

## Open questions

- What reads `id` and `enabled` when the pointer moves was not traced.
