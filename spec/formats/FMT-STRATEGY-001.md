---
id: FMT-STRATEGY-001
title: Strategic movement record, one force on the strategic map
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 280
text: false
definition: fmt_strategy_001.ksy
evidence: [FND-STRATEGY-003, FND-STRATEGY-006, FND-STRATEGY-007, FND-STRATEGY-008, FND-STRATEGY-009, FND-STRATEGY-010, FND-STRATEGY-011, FND-STRATEGY-019, FND-STRATEGY-020, FND-STRATEGY-021, FND-STRATEGY-023, FND-STRATEGY-024, FND-STRATEGY-025, FND-STRATEGY-026, FND-STRATEGY-029, FND-STRATEGY-030, FND-STRATEGY-032, FND-STRATEGY-033, FND-STRATEGY-034]
conflicting: []
split_with: []
related: []
---

## Layout

A structure the game keeps only in memory, in three lists: the six `player_forces`, the five
`hostile_forces` and the three `brigand_forces`. Each list uses the fields its rules name.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `active` | 1 while the force is on the map. | supported | FND-STRATEGY-003, FND-STRATEGY-020 |
| `0x04` | 4 | `INT32LE` | `selected` | 1 for the selected player record. | supported | FND-STRATEGY-019, FND-STRATEGY-026 |
| `0x08` | 4 | `INT32LE` | `unk_08` | Purpose unknown; placing and removing a field army clear it. | supported | FND-STRATEGY-025 |
| `0x0C` | 4 | `INT32LE` | `complete` | 1 when the force has no route left to follow. | supported | FND-STRATEGY-011, FND-STRATEGY-020 |
| `0x10` | 4 | `INT32LE` | `reversed` | 1 when the route file was loaded in reverse order. | supported | FND-STRATEGY-007 |
| `0x14` | 4 | `INT32LE` | `target` | Hostile force: the player record a pursuit hunts. Player record: 0, or a hostile force index with `0x1000` or a brigand force index with `0x10000`. | supported | FND-STRATEGY-006, FND-STRATEGY-020, FND-STRATEGY-026 |
| `0x18` | 4 | `INT32LE` | `count` | Number of route points. | supported | FND-STRATEGY-007, FND-STRATEGY-026 |
| `0x1C` | 4 | `INT32LE` | `swordsmen` | Swordsmen of a hostile or brigand force. | supported | FND-STRATEGY-008, FND-STRATEGY-032 |
| `0x20` | 4 | `INT32LE` | `halberdiers` | Halberdiers. | supported | FND-STRATEGY-008, FND-STRATEGY-032 |
| `0x24` | 4 | `INT32LE` | `knights` | Knights. | supported | FND-STRATEGY-008, FND-STRATEGY-032 |
| `0x28` | 4 | `INT32LE` | `origin` | Index in `properties` of the property the force came from. | supported | FND-STRATEGY-006, FND-STRATEGY-007 |
| `0x2C` | 4 | `INT32LE` | `lord` | Index in `persons` of the origin's lord. | supported | FND-STRATEGY-006, FND-STRATEGY-021 |
| `0x30` | 4 | `INT32LE` | `cursor` | Index of the route point being walked to. | supported | FND-STRATEGY-011 |
| `0x34` | 4 | `INT32LE` | `mode` | How a hostile force moves, see below. | supported | FND-STRATEGY-003 |
| `0x38` | 4 | `INT32LE` | `frame` | Marker frame. | supported | FND-STRATEGY-029, FND-STRATEGY-030 |
| `0x3C` | 4 | `INT32LE` | `dest_x` | Destination x in route units. | supported | FND-STRATEGY-009, FND-STRATEGY-011 |
| `0x40` | 4 | `INT32LE` | `dest_y` | Destination y in route units. | supported | FND-STRATEGY-009, FND-STRATEGY-011 |
| `0x44` | 4 | `INT32LE` | `cell_row` | Row of the terrain cell the force is on. | supported | FND-STRATEGY-003, FND-STRATEGY-009 |
| `0x48` | 4 | `INT32LE` | `cell_col` | Column of that cell. | supported | FND-STRATEGY-003, FND-STRATEGY-009 |
| `0x4C` | 8 | `BYTE[8]` | `unk_4C` | Purpose unknown. | supported | FND-STRATEGY-020 |
| `0x54` | 4 | `INT32LE` | `cooldown` | Passes before a player record can fight or be warned again. | supported | FND-STRATEGY-019, FND-STRATEGY-021 |
| `0x58` | 4 | `INT32LE` | `terrain_kind` | Kind of the cell a player record last moved onto. | supported | FND-STRATEGY-020 |
| `0x5C` | 4 | `FLOAT32LE` | `x` | Position x in route units. | supported | FND-STRATEGY-009, FND-STRATEGY-020 |
| `0x60` | 4 | `FLOAT32LE` | `y` | Position y in route units. | supported | FND-STRATEGY-009, FND-STRATEGY-020 |
| `0x64` | 4 | `FLOAT32LE` | `dir_x` | Direction or last step along x. | supported | FND-STRATEGY-009, FND-STRATEGY-011 |
| `0x68` | 4 | `FLOAT32LE` | `dir_y` | Direction or last step along y. | supported | FND-STRATEGY-009, FND-STRATEGY-011 |
| `0x6C` | 4 | `PTR32<INT32LE>` | `route` | The route points of a hostile or brigand force, `count` pairs of x and y, or null. | supported | FND-STRATEGY-007, FND-STRATEGY-032 |
| `0x70` | 160 | `INT32LE[40]` | `points` | A player record's own route, pairs of x and y. | supported | FND-STRATEGY-020, FND-STRATEGY-026 |
| `0x110` | 4 | `UINT32LE` | `image` | Handle of the marker image. | supported | FND-STRATEGY-029, FND-STRATEGY-030 |
| `0x114` | 4 | `BYTE[4]` | `unk_114` | Purpose unknown. | supported | FND-STRATEGY-020 |
| `0x118` | | | | Total size 280 | | |

## Enumerations and flags

### `mode`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `MOVE_DIRECT` | Walks straight to the destination. | supported | FND-STRATEGY-003, FND-STRATEGY-009 |
| 2 | `MOVE_ROUTED` | Follows its route points. | supported | FND-STRATEGY-003, FND-STRATEGY-011 |
| 3 | `MOVE_PURSUIT` | Chases the player record `target`. | supported | FND-STRATEGY-003, FND-STRATEGY-010 |

## Differences between builds

None known.

## Coverage

Read against the constructors, the three movement handlers, the player routines and the brigand routines.

## Open questions

- The purpose of `unk_08`, `unk_4C` and `unk_114` is unknown.
- `points` has room for 20 pairs; what the player routines read past the 20th is not recorded.
