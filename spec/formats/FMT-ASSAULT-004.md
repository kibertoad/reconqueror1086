---
id: FMT-ASSAULT-004
title: Live effect record, one of the 64 effects the scheduler runs
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 64
text: false
definition: null
evidence: [FND-ASSAULT-016, FND-ASSAULT-028, FND-ASSAULT-030, FND-ASSAULT-042]
conflicting: []
split_with: []
related: []
---

## Layout

A structure the game keeps only in memory, in 64 slots (`effects`).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 12 | `BYTE[12]` | `unk_00` | Purpose not recorded. | unknown | None |
| `0x0C` | 4 | `INT32LE` | `active` | 0 when the slot is free. | supported | FND-ASSAULT-016 |
| `0x10` | 4 | `UINT32LE` | `deadline` | Clock value at which the next tick is due. | supported | FND-ASSAULT-028 |
| `0x14` | 16 | `BYTE[16]` | `unk_14` | Purpose not recorded. | unknown | None |
| `0x24` | 4 | `INT32LE` | `moving_block` | Block written into the new cell when the mover changes cell. | supported | FND-ASSAULT-030 |
| `0x28` | 4 | `INT32LE` | `covered_block` | Block the mover covers, put back when it leaves the cell. | supported | FND-ASSAULT-030 |
| `0x2C` | 20 | `BYTE[20]` | `unk_2C` | Purpose not recorded. | unknown | None |
| `0x40` | | | | Total size 64 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked as a whole; the fields named here come from the code that uses them.

## Open questions

- The record also holds the values the constructor copies from the descriptor (FMT-ASSAULT-003),
  the tick counter, and the combatant or block that owns the effect. Their offsets are not
  recorded; the rules name them `tick_count`, `interval`, `flags`, `step_x`, `step_y`, `ticks`
  and `owner`.
- FND-ASSAULT-030 names `+0x24` and `+0x28` of "the record the scheduler moves"; whether that
  is this record or the combatant record is not settled.
