---
id: FMT-BATTLE-002
title: Pointer event, one entry of the pointer queue
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 20
text: false
definition: fmt_battle_002.ksy
evidence: [FND-BATTLE-020]
conflicting: []
split_with: []
related: []
---

## Layout

A structure the game keeps only in memory, the list `pointer_events`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `time` | `pointer_clock` when the event arrived. | supported | FND-BATTLE-020 |
| `0x04` | 4 | `INT32LE` | `x` | Pointer x. | supported | FND-BATTLE-020 |
| `0x08` | 4 | `INT32LE` | `y` | Pointer y. | supported | FND-BATTLE-020 |
| `0x0C` | 4 | `INT32LE` | `kind` | 0 primary press, 1 primary release, 2 secondary press, 3 secondary release. | supported | FND-BATTLE-020 |
| `0x10` | 4 | `BYTE[4]` | `unk_10` | Purpose unknown. | supported | FND-BATTLE-020 |
| `0x14` | | | | Total size 20 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read against the mouse callback, the queue pop and the classifier.

## Open questions

- The purpose of `unk_10` is unknown.
