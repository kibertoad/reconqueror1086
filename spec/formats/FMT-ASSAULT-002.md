---
id: FMT-ASSAULT-002
title: Combat row, one of the 25 weapon records in the executable
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER.EXE"]
byte_order: little
size: 28
text: false
definition: fmt_assault_002.ksy
evidence: [FND-ASSAULT-020, FND-ASSAULT-027, FND-ASSAULT-031, FND-ASSAULT-032, FND-ASSAULT-040]
conflicting: []
split_with: []
related: []
---

## Layout

Twenty-five records at `0x0009CE14` in `CD:CONQUER.EXE` (object 2 offset `0xCE14`), row `n` at
`0x0009CE14 + 28 * n` (FND-ASSAULT-032). Each weapon, and each combatant template, uses one row.
The values are per-weapon statistics and stay in the game's files.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `dice` | Number of damage dice. | supported | FND-ASSAULT-031, FND-ASSAULT-032 |
| `0x04` | 4 | `INT32LE` | `sides` | Sides of each damage die. | supported | FND-ASSAULT-031, FND-ASSAULT-032 |
| `0x08` | 4 | `INT32LE` | `penetration` | Armour the weapon ignores. | supported | FND-ASSAULT-031, FND-ASSAULT-032 |
| `0x0C` | 4 | `INT32LE` | `swing_divisor` | Divides the player's weapon swing speed; 380 to 1,000. | supported | FND-ASSAULT-032, FND-ASSAULT-040 |
| `0x10` | 4 | `INT32LE` | `reach` | Contact distance in 8.8 map units. | supported | FND-ASSAULT-020, FND-ASSAULT-027, FND-ASSAULT-031 |
| `0x14` | 8 | `BYTE[8]` | `unk_14` | Purpose unknown. | supported | FND-ASSAULT-032 |
| `0x1C` | | | | Total size 28 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Checked against the code that reads rows 0 to 24 in the one build.

## Open questions

- The purpose of `unk_14` is unknown.
- Which combat row each weapon uses is set by the weapon store's data and is described with
  the store.
