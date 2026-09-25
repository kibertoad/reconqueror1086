---
id: SCR-UI-019
title: Dubbing
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-PERSON-004, FND-PERSON-006, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013]
conflicting: []
split_with: []
related: [SCR-UI-006]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#DUBBING.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (0, 0, 640, 480) | Always | Stores 0 in `0x0009A708` and switches to SCR-UI-006. | FND-UI-013 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

None known.

## Timing

None known.

## Differences between builds

None known.

## Open questions

- What the entry routine `0x00019A8C` plays or shows.
