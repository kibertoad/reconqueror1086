---
id: SCR-UI-018
title: Fief overview
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-011, FND-UI-015]
conflicting: []
split_with: []
related: []
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#f_over.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Report pages | `C1086.GOB#f_poli.pcx`, `f_eco.pcx`, `f_per.pcx`, pictures `0x144`, `0x142`, `0x143` | Not traced | (0, 0, 640, 480) | After regions 0, 1 and 2 | FND-UI-011, FND-UI-015 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (182, 378, 43, 23) | Always | Shows picture `0x144` and runs `0x000312B8`. | FND-UI-015 |
| region 1 | (285, 378, 43, 23) | Always | Shows picture `0x142` and runs `0x00031B84`. | FND-UI-015 |
| region 2 | (389, 378, 43, 23) | Always | Shows picture `0x143` and runs `0x00031E28`. | FND-UI-015 |
| region 3 | (35, 375, 45, 17) | Always | Restores the music and returns to the previous screen, with either button. | FND-UI-015 |

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

- What the three report routines show, and how the player leaves them.
