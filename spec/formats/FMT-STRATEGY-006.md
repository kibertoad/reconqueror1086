---
id: FMT-STRATEGY-006
title: Brigand order, the descriptor of one brigand force
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 32
text: false
definition: fmt_strategy_006.ksy
evidence: [FND-STRATEGY-002, FND-STRATEGY-032, FND-STRATEGY-033]
conflicting: []
split_with: []
related: []
---

## Layout

A structure the game keeps only in memory, the three `brigand_orders`, one for each record of
`brigand_forces`. The routine that creates a brigand force takes a descriptor in the same layout.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `unk_00` | Purpose unknown; every creator writes 1. | supported | FND-STRATEGY-032 |
| `0x04` | 4 | `INT32LE` | `end_month` | Zero-based month from which the order ends. | supported | FND-STRATEGY-032, FND-STRATEGY-033 |
| `0x08` | 4 | `INT32LE` | `end_year` | Year from which the order ends. | supported | FND-STRATEGY-032, FND-STRATEGY-033 |
| `0x0C` | 4 | `BYTE[4]` | `unk_0C` | Purpose unknown. | supported | FND-STRATEGY-032 |
| `0x10` | 4 | `INT32LE` | `origin` | Index in `properties` of the property the brigands come from. | supported | FND-STRATEGY-032 |
| `0x14` | 4 | `INT32LE` | `active` | 1 while the order runs. | supported | FND-STRATEGY-032, FND-STRATEGY-033 |
| `0x18` | 4 | `INT32LE` | `fought` | 1 once a player army has fought the brigands to a result. | supported | FND-STRATEGY-033 |
| `0x1C` | 4 | `INT32LE` | `slot` | Index of the order and of its force. | supported | FND-STRATEGY-032 |
| `0x20` | | | | Total size 32 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read against the creator, the brigand pass and the two fixed creators.

## Open questions

- The purpose of `unk_00` and `unk_0C` is unknown.
