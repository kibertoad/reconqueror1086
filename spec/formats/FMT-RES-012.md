---
id: FMT-RES-012
title: Unidentified .GOB data candidates in CD root
status: superseded
builds: [BLD-GOG-EN]
superseded_by: [FMT-RES-001]
files: ["CD:C1086.GOB"]
byte_order: little # provisional hypothesis; see Open questions
size: null
text: false # provisional hypothesis; see Open questions
definition: null
evidence: [FND-RES-018]
conflicting: []
split_with: []
related: []
---

## Layout

This provisional suffix-based inventory listing is superseded by FMT-RES-001.
FND-RES-018 establishes complete byte identity with the installed copy and checks
the resource-container directory and stored extents against the disc manifest path.
No separate format or decoder is warranted for that copy. This table redirects
the former whole-file listing to its active format rather than defining another
layout.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| 0 | Whole file | FMT-RES-001 | archive_file | The byte-identical resource container described by FMT-RES-001. | supported | FND-RES-018 |

## Enumerations and flags

See FMT-RES-001 / FMT-RES-002.

## Differences between builds

None known.

## Coverage

The complete disc file and installed copy were compared and their directories
checked in FND-RES-018. The active format now lists both manifest paths.

## Open questions

None for this superseded listing. Runtime copy selection is retained under
FMT-RES-001 (Q-RES-157).
