---
id: FMT-UI-001
title: Screen layout, a HAT file
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_ui_001.ksy
evidence: [FND-UI-001, FND-UI-002]
conflicting: []
split_with: []
related: []
---

## Layout

A `.HAT` entry of `C1086.GOB`: a 40-byte header and the region records. The loader takes the number
of records from the file size, `(size - 0x28) / 24` rounded down, and uses `region_count` only to
size its array of region pointers.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `screen` | The screen number; equal to the number the file is registered under. | supported | FND-UI-001, FND-UI-002 |
| `0x04` | 4 | `INT32LE` | `origin_x` | Screen x; 0 in every file. | supported | FND-UI-002 |
| `0x08` | 4 | `INT32LE` | `origin_y` | Screen y; 0 in every file. | supported | FND-UI-002 |
| `0x0C` | 4 | `INT32LE` | `width` | Screen width; 640 in every file. | supported | FND-UI-002 |
| `0x10` | 4 | `INT32LE` | `height` | Screen height; 480 in every file. | supported | FND-UI-002 |
| `0x14` | 4 | `INT32LE` | `region_count` | Number of region records; sizes the pointer array. | supported | FND-UI-002 |
| `0x18` | 13 | `char[13]` | `background` | Name of the background picture, NUL-terminated; the loader copies 13 bytes. | supported | FND-UI-002 |
| `0x25` | 3 | `BYTE[3]` | `unk_25` | Never read; `C0 45 00` in every file. | supported | FND-UI-002 |
| `0x28` | `region_count * 24` | `FMT-UI-002[region_count]` | `regions` | The regions in file order; a callback is bound to a region by this position. | supported | FND-UI-002 |
| | | | | Total size `0x28 + region_count * 24` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 27 HAT entries of the GOG archive: in each, `region_count` equals the count the file size gives.
`TOPTS.HAT` has three bytes, `0D 0A 1A`, after its last record, which the rounding down skips
[FND-UI-002].

## Open questions

- The purpose of `unk_25`, which the game never reads.
- Whether anything reads `origin_x`, `origin_y`, `width` and `height` after the loader stores them.
