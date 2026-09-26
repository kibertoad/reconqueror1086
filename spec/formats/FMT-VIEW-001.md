---
id: FMT-VIEW-001
title: Scene block definition, one record of the Blocks resource
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: 96
text: false
definition: fmt_view_001.ksy
evidence: [FND-VIEW-001, FND-VIEW-006, FND-VIEW-007, FND-VIEW-008, FND-VIEW-009, FND-VIEW-010, FND-VIEW-011, FND-ASSAULT-001, FND-ASSAULT-004, FND-ASSAULT-022, FND-ASSAULT-029, FND-ASSAULT-034, FND-ASSAULT-035, FND-ASSAULT-036, FND-ASSAULT-037, FND-ASSAULT-038, FND-ASSAULT-039, FND-ASSAULT-002, FND-ASSAULT-003, FND-ASSAULT-008, FND-ASSAULT-027, FND-ASSAULT-044, FND-VIEW-019, FND-VIEW-021, FND-ASSAULT-047]
conflicting: []
split_with: []
related: [RULE-VIEW-003, RULE-ASSAULT-021]
---

## Layout

The `Blocks` resource of a scene archive holds as many of these records as the `Scenario` block
count gives, and the game keeps them in memory in the same layout. Live actors and objects also
use this layout for the blocks the game copies and changes while the scene runs.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `kind` | Shape of the block, see below. | supported | FND-VIEW-001, FND-VIEW-008 |
| `0x04` | 1 | `UINT8` | `behaviour` | Behaviour bits. | supported | FND-VIEW-001, FND-ASSAULT-001 |
| `0x04 bits 0..1` | | `bits[1]` | `see_through` | Set: a ray that hits the block goes on (RULE-VIEW-003). | supported | FND-VIEW-007 |
| `0x04 bits 1..2` | | `bits[1]` | `blocks_movement` | Set: movement cannot enter the cell. | supported | FND-ASSAULT-003, FND-ASSAULT-029 |
| `0x04 bits 2..3` | | `bits[1]` | `mirrored` | Set on a kind-4 block: the far half of its angles reuse the near half mirrored. | supported | FND-VIEW-009 |
| `0x04 bits 3..4` | | `bits[1]` | `chained` | Set with `see_through`: a ray follows `state_target` in the same cell. | supported | FND-VIEW-007 |
| `0x04 bits 4..5` | | `bits[1]` | `actionable` | Set: the player can act on the block (a door or a pickup). | supported | FND-ASSAULT-035, FND-ASSAULT-038 |
| `0x04 bits 5..6` | | `bits[1]` | `weapon_contact` | Set: the player's weapon replaces the block by its `state_target` when it reaches it. | supported | FND-ASSAULT-044 |
| `0x04 bits 6..7` | | `bits[1]` | `opens_on_contact` | Set: when the player walks into the cell, the block is used as if acted on (RULE-ASSAULT-021). Only the exit and gate blocks of the melee scenes have it. | supported | FND-ASSAULT-047 |
| `0x04 bits 7..8` | | `bits[1]` | `actor` | Set, with `interaction` 1: the block is a combatant. | supported | FND-ASSAULT-001 |
| `0x05` | 1 | `UINT8` | `marks` | Marks set while the scene runs. | supported | FND-ASSAULT-008 |
| `0x05 bits 0..1` | | `bits[1]` | `selected` | Set: the combatant is selected for orders. | supported | FND-ASSAULT-008, FND-ASSAULT-035 |
| `0x05 bits 1..8` | | `bits[7]` | `unk_05_1` | Purpose unknown. | supported | FND-VIEW-001 |
| `0x06` | 2 | `BYTE[2]` | `unk_06` | Purpose unknown. | supported | FND-VIEW-001 |
| `0x08` | 2 | `INT16LE` | `color_family` | First of the 32 colour maps used for the block: 0, 32, 64 or 96 on actors. | supported | FND-ASSAULT-002, FND-ASSAULT-039 |
| `0x0A` | 2 | `BYTE[2]` | `unk_0A` | Purpose unknown. | supported | FND-VIEW-001 |
| `0x0C` | 4 | `INT32LE` | `color_offset` | Subtracted from the distance colour map index. | supported | FND-VIEW-011 |
| `0x10` | 4 | `INT32LE` | `texture_width` | Texture width in pixels. | supported | FND-VIEW-001, FND-VIEW-010 |
| `0x14` | 4 | `INT32LE` | `width_shift` | Base-2 logarithm of `texture_width`. | supported | FND-VIEW-001, FND-VIEW-010 |
| `0x18` | 4 | `INT32LE` | `texture_height` | Texture height in pixels. | supported | FND-VIEW-001, FND-VIEW-010 |
| `0x1C` | 4 | `INT32LE` | `lower` | Elevation of the bottom of the block; 256 is one block height. | supported | FND-VIEW-001, FND-VIEW-006 |
| `0x20` | 4 | `INT32LE` | `upper` | Elevation of the top of the block. | supported | FND-VIEW-001, FND-VIEW-006 |
| `0x24` | 4 | `INT32LE` | `offset_x` | Signed 8.8 offset of the block within its cell along x; 0 in every shipped record. | supported | FND-ASSAULT-004, FND-ASSAULT-029, FND-VIEW-009 |
| `0x28` | 4 | `INT32LE` | `offset_y` | Signed 8.8 offset along y. | supported | FND-ASSAULT-004, FND-ASSAULT-029, FND-VIEW-009 |
| `0x2C` | 2 | `INT16LE` | `surface0` | North face texture, or -1; on kind 4, the first texture of the current state. | supported | FND-VIEW-001, FND-VIEW-006, FND-VIEW-009, FND-ASSAULT-034 |
| `0x2E` | 2 | `INT16LE` | `effect` | Effect descriptor started when the block's state begins (FMT-ASSAULT-003); -1 with a -1 north face. | supported | FND-ASSAULT-004, FND-ASSAULT-027, FND-ASSAULT-034 |
| `0x30` | 4 | `INT32LE` | `surface1` | East face texture, or -1. | supported | FND-VIEW-001, FND-VIEW-006 |
| `0x34` | 4 | `INT32LE` | `surface2` | South face texture, or -1; on kind 4, the number of angles. | supported | FND-VIEW-001, FND-VIEW-006, FND-VIEW-009, FND-VIEW-019, FND-VIEW-021 |
| `0x38` | 4 | `INT32LE` | `surface3` | West face texture, or -1; on kind 4, the heading the sprite faces. | supported | FND-VIEW-001, FND-VIEW-006, FND-VIEW-009, FND-VIEW-019 |
| `0x3C` | 4 | `BYTE[4]` | `unk_3C` | Purpose unknown. | supported | FND-VIEW-001 |
| `0x40` | 4 | `INT32LE` | `state_target` | Block that replaces this one when it is used, opened or left, and that a chained ray follows. | supported | FND-VIEW-007, FND-ASSAULT-038 |
| `0x44` | 2 | `BYTE[2]` | `unk_44` | Purpose unknown. | supported | FND-VIEW-001 |
| `0x46` | 2 | `INT16LE` | `movement` | Effect descriptor used when the actor moves. | supported | FND-ASSAULT-004, FND-ASSAULT-022 |
| `0x48` | 2 | `INT16LE` | `interaction` | What using the block does (RULE-ASSAULT-021); 1 on actors. | supported | FND-VIEW-001, FND-ASSAULT-001, FND-ASSAULT-036 |
| `0x4A` | 2 | `INT16LE` | `argument1` | First argument of `interaction`; on an actor, its combatant template. | supported | FND-ASSAULT-001, FND-ASSAULT-037 |
| `0x4C` | 2 | `INT16LE` | `argument2` | Second argument of `interaction`; on an actor, its combat row. | supported | FND-ASSAULT-001, FND-ASSAULT-037 |
| `0x4E` | 16 | `char[16]` | `label` | Name of the block for the scene's authors, ASCII, NUL-terminated, rest of the field unspecified. | supported | FND-VIEW-001 |
| `0x5E` | 2 | `BYTE[2]` | `end` | Always `0xCC 0xCC`. | supported | FND-VIEW-001 |
| `0x60` | | | | Total size 96 | | |

## Enumerations and flags

### `kind`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 0 | `BLOCK_EMPTY` | Nothing to hit. | supported | FND-VIEW-008 |
| 1 | `BLOCK_SOLID` | A box filling the cell. | supported | FND-VIEW-008 |
| 2 | `BLOCK_WALL_X` | A thin wall through the middle of the cell, running along x. | supported | FND-VIEW-008 |
| 3 | `BLOCK_WALL_Y` | A thin wall through the middle of the cell, running along y. | supported | FND-VIEW-008 |
| 4 | `BLOCK_SPRITE` | A sprite that faces the viewer: actors and objects. | supported | FND-VIEW-008, FND-VIEW-009 |
| 5 | `BLOCK_DIAGONAL_A` | A wall along one diagonal of the cell. | supported | FND-VIEW-008 |
| 6 | `BLOCK_DIAGONAL_B` | A wall along the other diagonal. | supported | FND-VIEW-008 |

## Differences between builds

None known.

## Coverage

Checked against all 12,784 records in the 42 `MELEE*` and `DEFEND*` archives, `.RES` and `.LOW`:
sizes, sentinels, labels, kinds and texture references. The other scene archives (`BAR*` and
the rest of the 99 `.RES` and `.LOW` files) decode with the same record size and have not been
checked field by field.

## Open questions

- The purpose of `unk_05_1`, `unk_06`, `unk_0A`, `unk_3C` and `unk_44` is unknown.
- Which diagonal `BLOCK_DIAGONAL_A` and `BLOCK_DIAGONAL_B` run along is not recorded.
- `interaction` values other than 1, 5, 7, 9 and 10, and the 20 cases of the dispatcher that
  they select, are not described.
