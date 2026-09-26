---
id: FMT-SAVE-002
title: Strategic map state, TROOPS.SAV
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: null
text: false
definition: fmt_save_002.ksy
evidence: [FND-SAVE-004]
conflicting: []
split_with: []
related: [RULE-SAVE-002, RULE-SAVE-003]
---

## Layout

Written to `troops.sav` in the current directory and stored in FMT-SAVE-001. A route block is a
`UINT32LE` marker: `0x1111` followed by the record's `count` pairs of `INT32LE` x and y when its
`complete` is 0 and it has a route, `0xFFFF` and nothing more otherwise.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `planting_notice` | The global of that name. | supported | FND-SAVE-004 |
| `0x04` | 32 | `BYTE[32]` | `king_order` | The 32 bytes at `0x000AA498`, starting with `king_order_kind`. | supported | FND-SAVE-004 |
| `0x24` | 4 | `INT32LE` | `strategic_speed` | The global of that name. | supported | FND-SAVE-004 |
| `0x28` | 4 | `INT32LE` | `next_king_order_year` | The global of that name. | supported | FND-SAVE-004 |
| `0x2C` | 4 | `INT32LE` | `next_brigand_year` | The global of that name. | supported | FND-SAVE-004 |
| `0x30` | 4 | `INT32LE` | `unk_AB0C8` | The global at `0x000AB0C8`; purpose unknown. | supported | FND-SAVE-004 |
| `0x34` | 4 | `INT32LE` | `unk_AB0C0` | The global at `0x000AB0C0`; purpose unknown. | supported | FND-SAVE-004 |
| `0x38` | 4 | `INT32LE` | `terrain_profile` | The global of that name. | supported | FND-SAVE-004 |
| `0x3C` | 4 | `INT32LE` | `field_army_count` | The global of that name. | supported | FND-SAVE-004 |
| `0x40` | 4 | `INT32LE` | `hostile_count` | The global of that name. | supported | FND-SAVE-004 |
| `0x44` | 4 | `INT32LE` | `selected_force` | The global of that name. | supported | FND-SAVE-004 |
| `0x48` | 4 | `INT32LE` | `spy_out` | The global of that name. | supported | FND-SAVE-004 |
| `0x4C` | 4 | `INT32LE` | `route_drawing` | The global of that name. | supported | FND-SAVE-004 |
| `0x50` | 4 | `INT32LE` | `fallback_person` | The global of that name. | supported | FND-SAVE-004 |
| `0x54` | 4 | `INT32LE` | `fallback_origin` | The global of that name. | supported | FND-SAVE-004 |
| `0x58` | 4 | `INT32LE` | `home_row` | The global of that name. | supported | FND-SAVE-004 |
| `0x5C` | 4 | `INT32LE` | `home_col` | The global of that name. | supported | FND-SAVE-004 |
| `0x60` | 4 | `INT32LE` | `ridden_force` | The global of that name. | supported | FND-SAVE-004 |
| `0x64` | 4 | `INT32LE` | `unk_A7540` | The global at `0x000A7540`; purpose unknown. | supported | FND-SAVE-004 |
| `0x68` | 1680 | `FMT-STRATEGY-001[6]` | `player_forces` | The player's six records; their `route` pointers are written as they are. | supported | FND-SAVE-004 |
| `0x6F8` | variable | | `brigands` | Three times: the brigand's FMT-STRATEGY-006 order (32 bytes), its FMT-STRATEGY-001 record (280 bytes) and its route block. | supported | FND-SAVE-004 |
| | variable | | `hostiles` | Five times: the hostile FMT-STRATEGY-001 record and its route block. | supported | FND-SAVE-004 |
| | 4028 | `BYTE[4028]` | `unk_AB170` | The 4,028 bytes at `0x000AB170`; purpose unknown. | supported | FND-SAVE-004 |
| | | | | Total size variable | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked against a save from the original; the layout comes from the code that writes and reads it.

## Open questions

- What `unk_AB0C8`, `unk_AB0C0`, `unk_A7540` and `unk_AB170` hold.
