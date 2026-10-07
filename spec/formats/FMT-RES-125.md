---
id: FMT-RES-125
title: Point-of-view settings, the POV entry of a scene file
status: unknown
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:CONQUER/*.RES", "CD:CONQUER/*.LOW"]
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

None known. The executable names the entry `POV` and reads it from a scene
file's archive into a buffer at `0x0009CB64`, with the error texts
`Failure reading POV from RES file.` and `Failure Reading POV`.

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

Every scene file holds one `POV` entry, 98 in all, stored (FND-RES-001).
No layout has been checked against them.

## Open questions

- What the POV entry holds and which fields the game reads: the reads at
  `0x00050CA0` and `0x00050D6F` are located but not read. (Q-RES-212)
