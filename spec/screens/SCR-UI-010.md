---
id: SCR-UI-010
title: War planning
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
| Background | `C1086.GOB#WARPLAN.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Army selectors | `C1086.GOB#WARPLAN.CSF`, frames 0 to 14 | The state of each of five armies | Regions 0 to 4 | Not traced | FND-UI-010 |
| Army controls | `C1086.GOB#WARPLAN.CSF`, frames 15 to 21 | None | Regions 5 to 7 | Not traced | FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (8, 0, 40, 38) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 1 | (23, 45, 40, 38) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 2 | (24, 91, 40, 38) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 3 | (7, 141, 40, 38) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 4 | (21, 185, 40, 38) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 5 | (8, 392, 114, 38) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 6 | (2, 433, 138, 38) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 7 | (224, 410, 198, 38) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 8 | (115, 103, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 9 | (115, 117, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 10 | (115, 131, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 11 | (538, 436, 57, 38) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| region 12 | (550, 392, 55, 38) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| region 13 | (315, 45, 100, 20) | Always | Runs a click routine that was not traced. | FND-UI-003 |

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

- What the click and slot-3 routines of the army selectors and the three rows do.
- Which frame shows which army state.
