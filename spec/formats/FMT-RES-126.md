---
id: FMT-RES-126
title: SVG entry of SKIRMISH.RES
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/SKIRMISH.RES"]
byte_order: little
size: null
text: false
definition: null
evidence: []
conflicting: []
split_with: []
related: []
---

## Layout

None known. The executable forms the names `%s.SVG` and `%s%s.SVG` near the
text `for File Pointers` at `0x0007DD59` and `0x0007DEBD`.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The one entry, `skirmish.svg`, is 125,702 bytes, the size of `skirmish.csf`,
with a different start (FND-MEDIA-007). No layout has been checked against it.

## Open questions

- What reads `skirmish.svg`, and what it holds: the name forms at
  `0x0007DD59` and `0x0007DEBD` are located but not read. (Q-RES-213)
