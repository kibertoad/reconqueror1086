---
id: SCR-UI-012
title: Estate map
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010]
conflicting: []
split_with: []
related: [SCR-UI-006, SCR-UI-009]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#ICONTEMP.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Estate tiles | `C1086.GOB#ica.CSF`, `ics.CSF` or `icw.CSF` | The estate terrain | Region 10 | Not traced | FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (422, 4, 199, 159) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 1 | (381, 6, 40, 437) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 2 | (0, 6, 18, 437) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 3 | (19, 0, 362, 5) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 4 | (19, 443, 362, 7) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 5 | (382, 0, 39, 5) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 6 | (0, 0, 18, 5) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 7 | (0, 444, 18, 10) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 8 | (382, 444, 39, 10) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 9 | (422, 178, 199, 264) | Always | None; the setup binds no routine. | FND-UI-003 |
| region 10 | (19, 8, 370, 433) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 11 | (18, 455, 180, 24) | Always | Switches to SCR-UI-009. | FND-UI-003 |
| region 12 | (400, 455, 200, 24) | Always | Switches to SCR-UI-006. | FND-UI-003 |
| region 13 | (200, 455, 200, 24) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 14 | (464, 380, 28, 20) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 15 | (424, 163, 50, 20) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 16 | (478, 164, 73, 17) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 17 | (554, 163, 65, 20) | Always | Runs a click routine that was not traced. | FND-UI-003 |

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

- Which tile set the game picks, and when.
- What regions 0, 10 and 13 to 17 do.
