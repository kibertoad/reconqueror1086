---
id: FMT-RES-127
title: SFG entry of SKIRMISH.RES
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

None known. The executable holds the texts `Unable to open WAR SFG file`,
used at `0x00025CAA` beside `MEN8.CSF` and `WAR_MODE`, and a message about
old SFG files used at `0x0003C9EB`.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The one entry, `skirmish.sfg`, is 150 bytes (FND-MEDIA-007). No layout has
been checked against it.

## Open questions

- What reads SFG data, and what it holds: the code at `0x00025C6A` and
  `0x0003C9EB` is located but not read. (Q-RES-214)
