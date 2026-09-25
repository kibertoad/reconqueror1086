---
id: SCR-UI-008
title: Practice grounds
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010]
conflicting: []
split_with: []
related: []
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#PRACTICE.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (58, 250, 250, 138) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 1 | (279, 140, 436, 70) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 2 | (374, 336, 202, 102) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 3 | (567, 239, 48, 18) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| region 4 | (1, 1, 234, 170) | Always | Runs a click routine that was not traced. | FND-UI-003 |

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

- What the click routines of regions 0, 1, 2 and 4 start.
