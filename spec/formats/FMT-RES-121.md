---
id: FMT-RES-121
title: Pointer hot spots, an MVG entry
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_res_121.ksy
evidence: [FND-RES-059, FND-UI-008]
conflicting: []
split_with: []
related: []
---

## Layout

An entry named like a sprite set with the extension `.MVG`. The game loads it
with the `.CSF` entry of the same name.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `magic` | `0xDEAD1234`; any other value fails the load. | supported | FND-RES-059 |
| `0x04` | 4 | `INT32LE` | `count` | Number of records, one per frame of the sprite set. | supported | FND-RES-059 |
| `0x08` | 4 | `INT32LE` | `buffer_size` | Size in bytes of each of the three work buffers the pointer drawing allocates. | supported | FND-RES-059 |
| `0x0C` | `count * 16` | `record[count]` | `records` | One record per frame. | supported | FND-RES-059 |
| | | | | Total size `12 + count * 16` | | |

A record:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `unk_00` | Never read; equals the frame's width in the shipped entry. | supported | FND-RES-059 |
| `0x04` | 4 | `INT32LE` | `unk_04` | Never read; equals the frame's height in the shipped entry. | supported | FND-RES-059 |
| `0x08` | 4 | `INT32LE` | `hot_x` | Subtracted from the pointer's x to place the frame. | supported | FND-RES-059 |
| `0x0C` | 4 | `INT32LE` | `hot_y` | Subtracted from the pointer's y to place the frame. | supported | FND-RES-059 |
| | | | | Total size 16 | | |

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The one MVG entry of the release, `ffmouse.mvg` in C1086.GOB, decodes to 108
bytes and fits the layout with count 6 and buffer size 484 [FND-RES-059].

## Open questions

- Whether `unk_00` and `unk_04` are read anywhere: the readers FND-RES-059
  found take the frame size from the CSF frame; a search for other readers of
  the record table was not made. (Q-RES-208)
