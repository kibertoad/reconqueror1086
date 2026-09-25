---
id: SCR-UI-011
title: Fief management tables
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

Four screens, 19 to 22, share the background and differ in their number of rows: 18, 14, 9 and 8.

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#FIEFMGMT.PCX` | None | (0, 0, 640, 480) | While the screen is shown (`FCASTLE.HAT`) | FND-UI-001, FND-UI-002, FND-UI-010 |
| Background | `C1086.GOB#FIEFMGMT.PCX` | None | (0, 0, 640, 480) | While the screen is shown (`FVILLAGE.HAT`) | FND-UI-001, FND-UI-002, FND-UI-010 |
| Background | `C1086.GOB#FIEFMGMT.PCX` | None | (0, 0, 640, 480) | While the screen is shown (`FFARM.HAT`) | FND-UI-001, FND-UI-002, FND-UI-010 |
| Background | `C1086.GOB#FIEFMGMT.PCX` | None | (0, 0, 640, 480) | While the screen is shown (`FFOREST.HAT`) | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| `FCASTLE.HAT` region 0 | (37, 68, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 1 | (37, 82, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 2 | (37, 96, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 3 | (37, 110, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 4 | (37, 124, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 5 | (37, 138, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 6 | (37, 152, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 7 | (37, 166, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 8 | (37, 180, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 9 | (37, 194, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 10 | (37, 208, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 11 | (37, 222, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 12 | (37, 236, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 13 | (37, 250, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 14 | (37, 264, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 15 | (37, 278, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 16 | (37, 292, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 17 | (37, 306, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 18 | (30, 435, 30, 20) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| `FCASTLE.HAT` region 19 | (70, 435, 70, 20) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| `FCASTLE.HAT` region 20 | (383, 20, 232, 380) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FCASTLE.HAT` region 21 | (532, 0, 86, 16) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 0 | (37, 68, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 1 | (37, 82, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 2 | (37, 96, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 3 | (37, 110, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 4 | (37, 124, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 5 | (37, 138, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 6 | (37, 152, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 7 | (37, 166, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 8 | (37, 180, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 9 | (37, 194, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 10 | (37, 208, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 11 | (37, 222, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 12 | (37, 236, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 13 | (37, 250, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 14 | (30, 435, 30, 20) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| `FVILLAGE.HAT` region 15 | (70, 435, 70, 20) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| `FVILLAGE.HAT` region 16 | (250, 435, 80, 20) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 17 | (383, 20, 232, 380) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FVILLAGE.HAT` region 18 | (532, 0, 86, 16) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 0 | (37, 68, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 1 | (37, 82, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 2 | (37, 96, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 3 | (37, 110, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 4 | (37, 124, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 5 | (37, 138, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 6 | (37, 152, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 7 | (37, 166, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 8 | (37, 180, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 9 | (30, 435, 30, 20) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| `FFARM.HAT` region 10 | (70, 435, 70, 20) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| `FFARM.HAT` region 11 | (383, 20, 232, 380) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFARM.HAT` region 12 | (532, 0, 86, 16) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 0 | (37, 68, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 1 | (37, 82, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 2 | (37, 96, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 3 | (37, 110, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 4 | (37, 124, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 5 | (37, 138, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 6 | (37, 152, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 7 | (37, 166, 300, 14) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 8 | (30, 435, 30, 20) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| `FFOREST.HAT` region 9 | (70, 435, 70, 20) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| `FFOREST.HAT` region 10 | (383, 20, 232, 380) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| `FFOREST.HAT` region 11 | (532, 0, 86, 16) | Always | Runs a click routine that was not traced. | FND-UI-003 |

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

- What the row routines in slots 2 and 3 change, and what the region at `(383, 20)` and the
  last region at `(532, 0)` do.
