---
id: FMT-RES-123
title: MIDI song, an HMP entry
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_res_123.ksy
evidence: [FND-SOUND-005]
conflicting: []
split_with: []
related: []
---

## Layout

An HMI MIDI song. The game reads the whole entry into a 32 KB DOS buffer and
hands it to the HMI MIDI driver; it reads none of the song's fields itself.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 14 | `CHAR[14]` | `tag` | `HMIMIDIP013195` in every shipped song. | supported | FND-SOUND-005 |
| `0x0E` | variable | `BYTE[]` | `body` | The HMI library's song data. | supported | FND-SOUND-005 |
| | | | | Total size variable | | |

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

All 12 `.hmp` entries of C1086.GOB, 1,948 to 28,705 bytes, start with the tag
[FND-SOUND-005].

## Open questions

- Does the game compare the tag before playing? The executable holds the same
  14 bytes, and FND-SOUND-005 did not locate their use. (Q-RES-210)
