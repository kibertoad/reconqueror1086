---
id: FMT-BATTLE-001
title: Field battle unit, one troop in an interactive field battle
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 52
text: false
definition: fmt_battle_001.ksy
evidence: [FND-BATTLE-004, FND-BATTLE-005, FND-BATTLE-007, FND-BATTLE-008, FND-BATTLE-012, FND-BATTLE-013, FND-BATTLE-014, FND-BATTLE-015, FND-BATTLE-019]
conflicting: []
split_with: []
related: []
---

## Layout

A structure the game keeps only in memory, the list `battle_units`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `category` | 0 for halberdiers, `0x78` for swordsmen, `0xF0` for knights; a knight that dies becomes `0x78`. Also the category's offset among the frames of `men8_image`. | supported | FND-BATTLE-004, FND-BATTLE-007, FND-BATTLE-012 |
| `0x04` | 4 | `INT32LE` | `x` | Position on the field in pixels. | supported | FND-BATTLE-005, FND-BATTLE-008, FND-BATTLE-014 |
| `0x08` | 4 | `INT32LE` | `y` | Position on the field in pixels. | supported | FND-BATTLE-005, FND-BATTLE-008, FND-BATTLE-014 |
| `0x0C` | 4 | `INT32LE` | `dest_x` | Destination x, or -1 for none. | supported | FND-BATTLE-004, FND-BATTLE-014, FND-BATTLE-015 |
| `0x10` | 4 | `INT32LE` | `dest_y` | Destination y, or -1 for none. | supported | FND-BATTLE-004, FND-BATTLE-014, FND-BATTLE-015 |
| `0x14` | 4 | `INT32LE` | `heading` | Heading from 0 to 7; 3 for the player's units and 7 for the foe's at the start. | supported | FND-BATTLE-004, FND-BATTLE-013 |
| `0x18` | 4 | `INT32LE` | `phase` | Step of the current animation, 0 to 4. | supported | FND-BATTLE-004, FND-BATTLE-012, FND-BATTLE-019 |
| `0x1C` | 4 | `INT32LE` | `state` | 0 idle or walking, `0x28` fighting, `0x50` dying. Also the state's offset among the frames. | supported | FND-BATTLE-004, FND-BATTLE-012, FND-BATTLE-013 |
| `0x20` | 4 | `INT32LE` | `strength` | Starts at 100; the unit is dead at 0 or below. | supported | FND-BATTLE-004, FND-BATTLE-012 |
| `0x24` | 4 | `INT32LE` | `value` | 10, 20 or 40 by category. No reader was traced. | supported | FND-BATTLE-004 |
| `0x28` | 4 | `INT32LE` | `control` | 1 when the unit acts on its own, 0 while it follows the player's order; 0 for the player's units and 1 for the foe's at the start. | supported | FND-BATTLE-004, FND-BATTLE-014, FND-BATTLE-015 |
| `0x2C` | 4 | `INT32LE` | `lane` | 0 for the player's units and `0x168` for the foe's. Also the side's offset among the frames. | supported | FND-BATTLE-004, FND-BATTLE-019 |
| `0x30` | 4 | `INT32LE` | `target` | Index in `battle_units` of the unit fought, or -1. | supported | FND-BATTLE-004, FND-BATTLE-012, FND-BATTLE-014 |
| `0x34` | | | | Total size 52 | | |

## Enumerations and flags

### Values of `state`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0x00` | `UNIT_IDLE` | Looking for a foe, turning or walking. | supported | FND-BATTLE-014 |
| `0x28` | `UNIT_FIGHTING` | In contact with its target. | supported | FND-BATTLE-012, FND-BATTLE-013 |
| `0x50` | `UNIT_DYING` | Dying; the death completes at phase 4. | supported | FND-BATTLE-012 |

## Differences between builds

None known.

## Coverage

Read against the constructor, the unit pass, the pointer and key handling, and the renderer.

## Open questions

- What reads `value`, if anything.
