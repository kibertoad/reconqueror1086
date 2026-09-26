---
id: FMT-SOUND-001
title: Sound bank, a .666 entry
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB", "CD:CONQUER/SKIRMISH.RES"]
byte_order: little
size: null
text: false
definition: fmt_sound_001.ksy
evidence: [FND-SOUND-001, FND-SOUND-003]
conflicting: []
split_with: []
related: [RULE-SOUND-001, RULE-SOUND-002]
---

## Layout

A decoded `.666` entry. Callers address a sample by the offset of its `length` field; the first is
at 4.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `tag` | `0x004A5031`, the bytes `1PJ` and 0. The game does not check it. | supported | FND-SOUND-001, FND-SOUND-003 |
| `0x04` | variable | `sample[]` | `samples` | Samples packed to the end of the entry. | supported | FND-SOUND-001 |
| | | | | Total size `4 + sum of (8 + length)` | | |

Each sample:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `length` | Number of sample bytes. | supported | FND-SOUND-001, FND-SOUND-003 |
| `0x04` | 4 | `UINT32LE` | `rate` | Samples per second. | supported | FND-SOUND-001, FND-SOUND-003 |
| `0x08` | `length` | `UINT8[length]` | `data` | Mono samples, unsigned, 128 is silence. | supported | FND-SOUND-001 |
| | | | | Total size `8 + length` | | |

## Enumerations and flags

### `rate`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 11025 | `RATE_11025` | Played at half the output step. 87 samples. | supported | FND-SOUND-001, FND-SOUND-003 |
| 11050 | `RATE_11050` | Not recognised by the game (BUG-SOUND-001). 5 samples, all in `fiefmgmt.666`. | supported | FND-SOUND-001, FND-SOUND-003 |
| 22050 | `RATE_22050` | Played at the output step. 10 samples. | supported | FND-SOUND-001, FND-SOUND-003 |
| 44100 | `RATE_44100` | Recognised by the game, used by no shipped sample. | supported | FND-SOUND-003 |

## Differences between builds

None known.

## Coverage

All 26 banks: 25 in `C1086.GOB` and `SKirmsnd.666` in `SKIRMISH.RES`, 102 samples, each bank used
exactly [FND-SOUND-001].

## Open questions

None.
