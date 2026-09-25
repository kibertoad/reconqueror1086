---
id: FMT-ASSAULT-003
title: Scene effect descriptor, one record of the SFXDEFS resource
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: 64
text: false
definition: fmt_assault_003.ksy
evidence: [FND-ASSAULT-004, FND-ASSAULT-022, FND-ASSAULT-029, FND-ASSAULT-042]
conflicting: []
split_with: []
related: [RULE-ASSAULT-017]
---

## Layout

The `SFXDEFS` resource of a scene archive holds as many of these as the dword at `Scenario`
offset `0x1C` gives.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 12 | `BYTE[12]` | `unk_00` | Purpose unknown. | supported | FND-ASSAULT-042 |
| `0x0C` | 4 | `INT32LE` | `tick_count` | Ticks the effect runs. | supported | FND-ASSAULT-004, FND-ASSAULT-042 |
| `0x10` | 4 | `BYTE[4]` | `unk_10` | Purpose unknown. | supported | FND-ASSAULT-042 |
| `0x14` | 4 | `INT32LE` | `interval` | Milliseconds between ticks. | supported | FND-ASSAULT-004, FND-ASSAULT-042 |
| `0x18` | 4 | `UINT32LE` | `flags` | Scheduler flags. | supported | FND-ASSAULT-042 |
| `0x18 bits 0..4` | | `bits[4]` | `unk_flags_0` | Purpose unknown. | supported | FND-ASSAULT-042 |
| `0x18 bits 4..5` | | `bits[1]` | `stop_when_blocked` | Set: a blocked step ends the effect (RULE-ASSAULT-017). | supported | FND-ASSAULT-022, FND-ASSAULT-029 |
| `0x18 bits 5..6` | | `bits[1]` | `unk_flags_5` | Purpose unknown. | supported | FND-ASSAULT-042 |
| `0x18 bits 6..7` | | `bits[1]` | `turn_when_blocked` | Set: a blocked step turns the mover left and the effect goes on. | supported | FND-ASSAULT-022, FND-ASSAULT-029 |
| `0x18 bits 7..32` | | `bits[25]` | `unk_flags_7` | Purpose unknown. | supported | FND-ASSAULT-042 |
| `0x1C` | 4 | `INT32LE` | `step_x` | Movement per tick, forward component in 8.8 units, turned by the mover's heading. | supported | FND-ASSAULT-004, FND-ASSAULT-029, FND-ASSAULT-042 |
| `0x20` | 4 | `INT32LE` | `step_y` | Movement per tick, sideways component. | supported | FND-ASSAULT-004, FND-ASSAULT-042 |
| `0x24` | 4 | `BYTE[4]` | `unk_24` | Purpose unknown. | supported | FND-ASSAULT-042 |
| `0x28` | 4 | `INT32LE` | `block_selector` | -1, or a scene block the effect applies to. | supported | FND-ASSAULT-042 |
| `0x2C` | 4 | `INT32LE` | `block_step` | Added to the block number each tick. | supported | FND-ASSAULT-004, FND-ASSAULT-042 |
| `0x30` | 4 | `INT32LE` | `loop_block` | Block number the block step returns to. | supported | FND-ASSAULT-042 |
| `0x34` | 4 | `INT32LE` | `surface_step` | Added to the surface number each tick. | supported | FND-ASSAULT-004, FND-ASSAULT-042 |
| `0x38` | 4 | `INT32LE` | `last_surface` | Surface number the surface step stops at. | supported | FND-ASSAULT-042 |
| `0x3C` | 4 | `INT32LE` | `heading_step` | Added to the heading each tick. | supported | FND-ASSAULT-042 |
| `0x40` | | | | Total size 64 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against every `SFXDEFS` resource in the 99 scene archives for count and interval, and
against the actor descriptors of the 42 `MELEE*` and `DEFEND*` archives for the remaining
fields named here.

## Open questions

- The purpose of `unk_00`, `unk_10`, `unk_24` and the flag bits other than `0x10` and `0x40` is
  unknown. The shipped actor descriptors have flags `0x142` (movement) and 5 (state changes).
