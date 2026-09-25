---
id: SCR-UI-011
title: Fief management tables
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-011, FND-UI-016]
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
| Full-screen terrain frame | `C1086.GOB#fmtemp1.pcx`, picture `0x141` | None | (0, 0, 640, 480) | While the terrain is full screen | FND-UI-011, FND-UI-016 |
| Tax | Font not traced | The tax rate | (265, 435) | On `FVILLAGE.HAT`, after a change | FND-UI-016 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| `FCASTLE.HAT` region 0 | (37, 68, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 1 | (37, 82, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 2 | (37, 96, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 3 | (37, 110, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 4 | (37, 124, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 5 | (37, 138, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 6 | (37, 152, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 7 | (37, 166, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 8 | (37, 180, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 9 | (37, 194, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 10 | (37, 208, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 11 | (37, 222, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 12 | (37, 236, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 13 | (37, 250, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 14 | (37, 264, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 15 | (37, 278, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 16 | (37, 292, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 17 | (37, 306, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FCASTLE.HAT` region 18 | (30, 435, 30, 20) | Always | OK: commits the staged values and returns to the previous screen. | FND-UI-016 |
| `FCASTLE.HAT` region 19 | (70, 435, 70, 20) | Always | Cancel: discards the staged values and returns to the previous screen. | FND-UI-016 |
| `FCASTLE.HAT` region 20 | (383, 20, 232, 380) | Always | Runs the terrain routine; the right button steps the chosen tile kind through 16 to 26. | FND-UI-016 |
| `FCASTLE.HAT` region 21 | (532, 0, 86, 16) | Always | Switches the terrain between the table view and the full screen. | FND-UI-016 |
| `FVILLAGE.HAT` region 0 | (37, 68, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 1 | (37, 82, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 2 | (37, 96, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 3 | (37, 110, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 4 | (37, 124, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 5 | (37, 138, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 6 | (37, 152, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 7 | (37, 166, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 8 | (37, 180, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 9 | (37, 194, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 10 | (37, 208, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 11 | (37, 222, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 12 | (37, 236, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 13 | (37, 250, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FVILLAGE.HAT` region 14 | (30, 435, 30, 20) | Always | OK: commits the staged values and returns to the previous screen. | FND-UI-016 |
| `FVILLAGE.HAT` region 15 | (70, 435, 70, 20) | Always | Cancel: discards the staged values and returns to the previous screen. | FND-UI-016 |
| `FVILLAGE.HAT` region 16 | (250, 435, 80, 20) | Always | Raises the tax by 5 up to 100; the right button lowers it by 5 down to 0. | FND-UI-016 |
| `FVILLAGE.HAT` region 17 | (383, 20, 232, 380) | Always | Runs the terrain routine; the right button steps the chosen tile kind through 16 to 26. | FND-UI-016 |
| `FVILLAGE.HAT` region 18 | (532, 0, 86, 16) | Always | Switches the terrain between the table view and the full screen. | FND-UI-016 |
| `FFARM.HAT` region 0 | (37, 68, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 1 | (37, 82, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 2 | (37, 96, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 3 | (37, 110, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 4 | (37, 124, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 5 | (37, 138, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 6 | (37, 152, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 7 | (37, 166, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 8 | (37, 180, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFARM.HAT` region 9 | (30, 435, 30, 20) | Always | OK: commits the staged values and returns to the previous screen. | FND-UI-016 |
| `FFARM.HAT` region 10 | (70, 435, 70, 20) | Always | Cancel: discards the staged values and returns to the previous screen. | FND-UI-016 |
| `FFARM.HAT` region 11 | (383, 20, 232, 380) | Always | Runs the terrain routine; the right button steps the chosen tile kind through 16 to 26. | FND-UI-016 |
| `FFARM.HAT` region 12 | (532, 0, 86, 16) | Always | Switches the terrain between the table view and the full screen. | FND-UI-016 |
| `FFOREST.HAT` region 0 | (37, 68, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFOREST.HAT` region 1 | (37, 82, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFOREST.HAT` region 2 | (37, 96, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFOREST.HAT` region 3 | (37, 110, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFOREST.HAT` region 4 | (37, 124, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFOREST.HAT` region 5 | (37, 138, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFOREST.HAT` region 6 | (37, 152, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFOREST.HAT` region 7 | (37, 166, 300, 14) | Always | Runs the table's row routine; the right button runs a second one. Not traced further. | FND-UI-016 |
| `FFOREST.HAT` region 8 | (30, 435, 30, 20) | Always | OK: commits the staged values and returns to the previous screen. | FND-UI-016 |
| `FFOREST.HAT` region 9 | (70, 435, 70, 20) | Always | Cancel: discards the staged values and returns to the previous screen. | FND-UI-016 |
| `FFOREST.HAT` region 10 | (383, 20, 232, 380) | Always | Runs the terrain routine; the right button steps the chosen tile kind through 16 to 26. | FND-UI-016 |
| `FFOREST.HAT` region 11 | (532, 0, 86, 16) | Always | Switches the terrain between the table view and the full screen. | FND-UI-016 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Table view | The screen is entered, or the full-screen region is clicked in the full screen | The full-screen region is clicked | FND-UI-016 |
| Full screen | The full-screen region is clicked in the table view | The full-screen region is clicked again | FND-UI-016 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- What the row and terrain routines change, and what `0x00034F20` computes from the tax.
