---
id: FMT-STRATEGY-005
title: Strategic terrain grid, icon.jp
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: 16 + rows * cols * 4
text: false
definition: fmt_strategy_005.ksy
evidence: [FND-STRATEGY-003, FND-STRATEGY-013, FND-STRATEGY-017]
conflicting: []
split_with: []
related: []
---

## Layout

Entry 292 of `C1086.GOB`, and the scratch file `temp.jap` the game writes in the same layout.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `cell_width` | 80. | supported | FND-STRATEGY-017 |
| `0x04` | 4 | `INT32LE` | `cell_height` | 80. | supported | FND-STRATEGY-017 |
| `0x08` | 4 | `INT32LE` | `rows` | 200. | supported | FND-STRATEGY-017 |
| `0x0C` | 4 | `INT32LE` | `cols` | 400. | supported | FND-STRATEGY-017 |
| `0x10` | `rows * cols * 4` | `UINT32LE[]` | `cells` | The cells, column by column: all rows of column 0, then of column 1. | supported | FND-STRATEGY-017 |
| `0x10 bits 0..16` | | `bits[16]` | `tile` | In each cell: the tile drawn there, which also gives its terrain kind. | supported | FND-STRATEGY-013, FND-STRATEGY-017 |
| `0x10 bits 16..24` | | `bits[8]` | `person` | In each cell: the index in `persons` of the person whose cell it is, or 0. | supported | FND-STRATEGY-003, FND-STRATEGY-017 |
| `0x10 bits 24..32` | | `bits[8]` | `unk_bits_24` | In each cell: purpose unknown. | supported | FND-STRATEGY-017 |
| | | | | Total size `16 + rows * cols * 4` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The GOG entry, 320,016 bytes.

## Open questions

- The purpose of the top byte of a cell is unknown.
