---
id: SCR-UI-004
title: Youth dilemma
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
| Background | `C1086.GOB#MORALITY.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Choice pictures | The dilemma sequences, 99 by 149 frames | The three answers | Regions 0 to 2 | Not traced | FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (62, 310, 100, 150) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 1 | (268, 310, 100, 150) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 2 | (478, 310, 100, 150) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 3 | (3, 184, 632, 16) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 4 | (3, 289, 632, 16) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 6 (record 5) | (473, 132, 97, 40) | Always | Switches screens through a variable; not traced. | FND-UI-003 |
| region 5 (record 6) | (470, 48, 110, 30) | Always | Runs a click routine that was not traced. | FND-UI-003 |

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

- What each click routine does; the region at position 5, id 6, switches screens through a variable.
