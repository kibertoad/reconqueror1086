---
id: SCR-UI-012
title: Estate map
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-DRAGON-004, FND-STRATEGY-013, FND-STRATEGY-026, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-011, FND-UI-015]
conflicting: []
split_with: []
related: [SCR-UI-006, SCR-UI-009, RULE-DRAGON-002]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#ICONTEMP.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Estate tiles | `C1086.GOB#ica.CSF`, `ics.CSF` or `icw.CSF` | The estate terrain | Region 10 | Not traced | FND-UI-010 |
| England map | `C1086.GOB#engmap1.pcx`, picture `0x1B5` | None | (0, 0, 640, 480) | After region 15 until a click | FND-UI-011, FND-UI-015 |
| Orders | `C1086.GOB#message.pcx`, picture `0x1B9`, and `FFONTA2.FNT` | The tournament invitation, the king's and the overlord's orders, or that there is nothing to report | Not traced | After region 16 until a click | FND-UI-011, FND-UI-015 |
| Help | `C1086.GOB#help.pcx`, picture `0x1B8` | None | (0, 0, 640, 480) | After region 17 until a click | FND-UI-011, FND-UI-015 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (422, 4, 199, 159) | Always | Moves the map view to the clicked point of the inset. | FND-UI-015 |
| region 1 | (381, 6, 40, 437) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 2 | (0, 6, 18, 437) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 3 | (19, 0, 362, 5) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 4 | (19, 443, 362, 7) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 5 | (382, 0, 39, 5) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 6 | (0, 0, 18, 5) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 7 | (0, 444, 18, 10) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 8 | (382, 444, 39, 10) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 9 | (422, 178, 199, 264) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 10 | (19, 8, 370, 433) | Always | Runs the map click of FND-STRATEGY-026; the right button reports on the army under the pointer. | FND-STRATEGY-026, FND-UI-015 |
| region 11 | (18, 455, 180, 24) | Always | Switches to SCR-UI-009. | FND-UI-003 |
| region 12 | (400, 455, 200, 24) | Always | Switches to SCR-UI-006 when the dword at `0x0009AEF0` is 1. | FND-UI-015 |
| region 13 | (200, 455, 200, 24) | Always | Starts a siege (RULE-DRAGON-002) when the dword at `0x0009AEF4` is not 0. | FND-UI-015, FND-DRAGON-004 |
| region 14 | (464, 380, 28, 20) | Always | Raises the map speed by 1 up to 15; the right button lowers it down to 1. | FND-UI-015, FND-STRATEGY-013 |
| region 15 | (424, 163, 50, 20) | Always | Shows the England map until a click. | FND-UI-015 |
| region 16 | (478, 164, 73, 17) | Always | Shows the orders until a click. | FND-UI-015 |
| region 17 | (554, 163, 65, 20) | Always | Shows the help picture until a click. | FND-UI-015 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

| Sound | Resource | Played when | Evidence |
|---|---|---|---|
| Click | The sound at `0x0009AEFC` | Each click on regions 0 and 11 to 14 | FND-UI-015 |

## States

None known.

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Which tile set the game picks, and when.
- What sets `0x0009AEF0` and `0x0009AEF4`.
- What the right-button report `0x0001265C` offers for the selected army.
