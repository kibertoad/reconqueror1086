---
id: FMT-ASSAULT-001
title: Combatant record, one of the actors of a first-person assault
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 68
text: false
definition: fmt_assault_001.ksy
evidence: [FND-ASSAULT-001, FND-ASSAULT-008, FND-ASSAULT-010, FND-ASSAULT-018, FND-ASSAULT-021, FND-ASSAULT-023, FND-ASSAULT-026, FND-ASSAULT-027, FND-ASSAULT-028, FND-ASSAULT-031, FND-ASSAULT-033, FND-ASSAULT-035, FND-ASSAULT-014, FND-ASSAULT-017, FND-ASSAULT-019, FND-ASSAULT-020, FND-ASSAULT-022, FND-ASSAULT-024, FND-ASSAULT-025, FND-ASSAULT-046]
conflicting: []
split_with: []
related: []
---

## Layout

A structure the game keeps only in memory. The combatant records form one list: records 0 to 9
are the ten templates and each placed actor adds one after them (`combatants`).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 8 | `BYTE[8]` | `unk_00` | Purpose unknown. | supported | FND-ASSAULT-001 |
| `0x08` | 4 | `INT32LE` | `effect` | The live effect the combatant is running, or -1. | supported | FND-ASSAULT-028 |
| `0x0C` | 4 | `INT32LE` | `x` | Position along x in 8.8 map units; starts at the centre of the cell. | supported | FND-ASSAULT-001, FND-ASSAULT-018 |
| `0x10` | 4 | `INT32LE` | `y` | Position along y in 8.8 map units. | supported | FND-ASSAULT-001, FND-ASSAULT-018 |
| `0x14` | 4 | `BYTE[4]` | `unk_14` | Purpose unknown. | supported | FND-ASSAULT-001 |
| `0x18` | 4 | `INT32LE` | `mode` | Current mode. | supported | FND-ASSAULT-010, FND-ASSAULT-028 |
| `0x1C` | 4 | `INT32LE` | `next_mode` | Mode taken when the current mode's test succeeds; a retainer order writes the requested mode here. | supported | FND-ASSAULT-008, FND-ASSAULT-010 |
| `0x20` | 4 | `INT32LE` | `fallback_mode` | Mode taken when the test fails. | supported | FND-ASSAULT-010 |
| `0x24` | 4 | `INT32LE` | `target` | Index in `combatants` of the combatant it pursues, strikes or follows. | supported | FND-ASSAULT-008, FND-ASSAULT-018, FND-ASSAULT-027 |
| `0x28` | 4 | `INT32LE` | `dest_x` | Destination cell along x. | supported | FND-ASSAULT-021, FND-ASSAULT-023, FND-ASSAULT-035 |
| `0x2C` | 4 | `INT32LE` | `dest_y` | Destination cell along y. | supported | FND-ASSAULT-021, FND-ASSAULT-023, FND-ASSAULT-035 |
| `0x30` | 4 | `INT32LE` | `side` | 0 for the player's side, otherwise hostile. | supported | FND-ASSAULT-001, FND-ASSAULT-026 |
| `0x34` | 4 | `INT32LE` | `skill` | Fighting skill used by the hit chance. | supported | FND-ASSAULT-031, FND-ASSAULT-033 |
| `0x38` | 4 | `BYTE[4]` | `unk_38` | Purpose unknown. | supported | FND-ASSAULT-001 |
| `0x3C` | 4 | `INT32LE` | `armor` | Armour; the attacker's combat-row penetration is taken from it before the remainder reduces the damage. | supported | FND-ASSAULT-046 |
| `0x40` | 4 | `INT32LE` | `health` | Health; the combatant dies when it reaches 0. | supported | FND-ASSAULT-026, FND-ASSAULT-028, FND-ASSAULT-033 |
| `0x44` | | | | Total size 68 | | |

## Enumerations and flags

### `mode`, `next_mode`, `fallback_mode`

The names follow what each mode's test and handler do (RULE-ASSAULT-008).

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `MODE_FORMATION` | Checks for a friend in the 3 by 3 cells around it; no movement. | supported | FND-ASSAULT-014, FND-ASSAULT-017 |
| 2 | `MODE_DEFEND` | Checks for an enemy in the 3 by 3 cells around it; no movement. | supported | FND-ASSAULT-014, FND-ASSAULT-017 |
| 3 | `MODE_SEEK_FRIEND` | Looks for a visible friend; no movement. | supported | FND-ASSAULT-014, FND-ASSAULT-019 |
| 4 | `MODE_SEEK_ENEMY` | Looks for a visible enemy; no movement. | supported | FND-ASSAULT-014, FND-ASSAULT-018 |
| 5 | `MODE_RETREAT` | Walks on in its heading, turning left at obstacles, looking for a visible friend. | supported | FND-ASSAULT-014, FND-ASSAULT-022 |
| 6 | `MODE_ATTACK` | Walks on in its heading, turning left at obstacles, looking for a visible enemy. | supported | FND-ASSAULT-014, FND-ASSAULT-022 |
| 7 | `MODE_REGROUP` | Walks towards its target friend until any friend is close in front. | supported | FND-ASSAULT-014, FND-ASSAULT-020 |
| 8 | `MODE_PURSUE` | Walks towards its target enemy until an enemy is within reach. | supported | FND-ASSAULT-014, FND-ASSAULT-020 |
| 9 | `MODE_FLEE_FRIEND` | Walks away from its target, looking for a visible friend. | supported | FND-ASSAULT-014, FND-ASSAULT-025 |
| 10 | `MODE_FLEE` | Walks away from its target, looking for a visible enemy; the Retreat order requests it. | supported | FND-ASSAULT-008, FND-ASSAULT-014, FND-ASSAULT-025 |
| 11 | `MODE_STRIKE` | Strikes its target when it is within reach. | supported | FND-ASSAULT-014, FND-ASSAULT-027 |
| 12 | `MODE_GO_TO` | Walks to the destination cell. | supported | FND-ASSAULT-014, FND-ASSAULT-021, FND-ASSAULT-023 |
| 13 | `MODE_RALLY` | Checks whether its health is at least 6; no movement. | supported | FND-ASSAULT-014, FND-ASSAULT-021 |
| 14 | `MODE_HIT` | Shows being hit. | supported | FND-ASSAULT-014, FND-ASSAULT-024 |
| 15 | `MODE_DYING` | Shows dying. | supported | FND-ASSAULT-014, FND-ASSAULT-024 |
| 16 | `MODE_FOLLOW` | Walks towards the player. | supported | FND-ASSAULT-008, FND-ASSAULT-014, FND-ASSAULT-021 |
| 17 | `MODE_FOLLOW_SEARCH` | Walks on in its heading, turning left at obstacles, until the player is in sight. | supported | FND-ASSAULT-014, FND-ASSAULT-021, FND-ASSAULT-022 |

## Differences between builds

None known.

## Coverage

A memory structure: checked against the code that reads and writes it, listed in the evidence.

## Open questions

- The record also keeps the combatant's actor kind, template, combat row, whether it is live,
  which map block is its own, and its heading. Where it keeps them is not recorded; they
  lie in `unk_00`, `unk_14` or `unk_38`, or in the combatant's block. The rules reach them
  through the functions of RULE-ASSAULT-027.
- The object action dispatcher tests the player's dword at offset 4 inside its cases
  (FND-ASSAULT-036); what it holds is not recorded.
- The purpose of `unk_00`, `unk_14` and `unk_38` is otherwise unknown.
