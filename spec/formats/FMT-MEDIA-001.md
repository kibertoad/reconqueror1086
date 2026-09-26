---
id: FMT-MEDIA-001
title: CSF sprite file
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB", "CD:CONQUER/SKIRMISH.RES"]
byte_order: little
size: null
text: false
definition: fmt_media_001.ksy
evidence: [FND-MEDIA-001, FND-MEDIA-002]
conflicting: []
split_with: []
related: [RULE-MEDIA-001]
---

## Layout

A decoded `.CSF` entry. There are no offsets: frame `i` starts at the end of the size table plus the
sizes of frames 0 to `i - 1`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `char[2]` | `version` | `2J`; the game stops with an error on anything else. | supported | FND-MEDIA-001, FND-MEDIA-002 |
| `0x02` | 4 | `UINT32LE` | `count` | Number of frames. | supported | FND-MEDIA-001, FND-MEDIA-002 |
| `0x06` | `count * 4` | `UINT32LE[count]` | `sizes` | Byte size of each frame, in frame order. | supported | FND-MEDIA-001, FND-MEDIA-002 |
| `6 + count * 4` | sum of `sizes` | `FMT-MEDIA-002[count]` | `frames` | The frames, packed in order. | supported | FND-MEDIA-001, FND-MEDIA-002 |
| | | | | Total size `6 + count * 4 + sum of sizes` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 66 CSF entries of the release, 65 in `C1086.GOB` and `skirmish.csf` in `SKIRMISH.RES`, 3,262
frames in all [FND-MEDIA-002]. In every one the sizes add up exactly to the rest of the entry.

## Open questions

None.
