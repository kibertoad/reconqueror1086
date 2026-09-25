---
id: FMT-RES-001
title: Resource container, a GOB, RES or LOW file
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB", "CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
byte_order: little
size: null
text: false
definition: fmt_res_001.ksy
evidence: [FND-RES-001, FND-RES-002, FND-RES-005]
conflicting: []
split_with: []
related: [RULE-RES-001]
---

## Layout

A whole container file. The entry data lies between the header and the directory, and the
directory is the last thing in the file. The game reads the header and the directory once, when it
opens the file, and each entry's bytes when it reads that entry (RULE-RES-001).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `magic` | `.RES`, with no NUL; the game refuses a file without it. | supported | FND-RES-001, FND-RES-002 |
| `0x04` | 4 | `UINT32LE` | `directory_offset` | File offset of `entry_count`; also where the next entry's data would be written. | supported | FND-RES-001, FND-RES-002, FND-RES-005 |
| `0x08` | `directory_offset - 8` | `BYTE[directory_offset - 8]` | `data` | The stored bytes of the entries, packed in directory order. | supported | FND-RES-001 |
| `directory_offset` | 4 | `UINT32LE` | `entry_count` | Number of directory records. | supported | FND-RES-001, FND-RES-002 |
| `directory_offset + 4` | `entry_count * 52` | `FMT-RES-002[entry_count]` | `directory` | The records, in the order the game searches them. | supported | FND-RES-001, FND-RES-002 |
| | | | | Total size `directory_offset + 4 + entry_count * 52` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 100 containers of the release: `C1086.GOB` (486 entries), the 50 `.RES` and the 49 `.LOW` scene
files (26,276 entries). Each file ends exactly at the end of its directory, and the data of each
entry starts where the previous one ends, apart from one 1,236-byte gap in `SKIRMISH.RES`
[FND-RES-001]. A file the game writes has the same layout (FND-RES-005).

## Open questions

- What, if anything, the 1,236 bytes before `FNT6.PCX` in `SKIRMISH.RES` held.
