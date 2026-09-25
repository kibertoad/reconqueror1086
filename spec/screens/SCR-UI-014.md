---
id: SCR-UI-014
title: Tournament grounds
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-TOURNEY-003, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013]
conflicting: []
split_with: []
related: [SCR-UI-006, SCR-UI-015, SCR-UI-016]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#JOUSTMAP.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (37, 77, 131, 100) | Always | Stores 1 in `0x0009DBD0` and switches to SCR-UI-006. | FND-UI-013 |
| region 1 | (422, 345, 86, 62) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 2 | (356, 146, 172, 28) | Always | Switches to SCR-UI-015. | FND-UI-013 |
| region 3 | (259, 180, 338, 28) | Always | Stores 0 in the tent choice at `0x000AFF44` (FND-TOURNEY-003) and switches to SCR-UI-016. | FND-UI-013, FND-TOURNEY-003 |
| region 4 | (237, 301, 172, 80) | Always | Stores 1 in the tent choice and switches to SCR-UI-016. | FND-UI-013, FND-TOURNEY-003 |

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

- What region 1 covers and why it has no routine.
