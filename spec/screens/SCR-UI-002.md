---
id: SCR-UI-002
title: Game options
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010]
conflicting: []
split_with: []
related: [SCR-UI-008]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#OPTFIN.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Switches | `C1086.GOB#OPTION.CSF`, frames 0 to 3 | The state of each setting | Not traced | Not traced | FND-UI-010 |
| Resume control | `C1086.GOB#OPTION.CSF`, frame 4 | None | Region 11 | Not traced | FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (145, 90, 100, 65) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 1 | (500, 85, 85, 70) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 2 | (10, 20, 75, 65) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 3 | (15, 243, 255, 253) | Always | Switches screens through a variable; not traced. | FND-UI-003 |
| region 4 | (160, 0, 120, 85) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 5 | (340, 0, 145, 230) | Always | Switches to screen 15, SCR-UI-008. | FND-UI-003 |
| region 6 | (310, 233, 100, 55) | Always | Switches screens through a variable; not traced. | FND-UI-003 |
| region 7 | (411, 233, 100, 55) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 8 | (290, 320, 140, 175) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 9 | (430, 320, 150, 175) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 10 | (510, 15, 90, 70) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 11 | (525, 155, 80, 40) | When the screen enables it | Switches screens through a variable; not traced. | FND-UI-003 |
| region 12 | (200, 160, 90, 60) | Always | Runs a click routine that was not traced. | FND-UI-003 |

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

- What each region's click routine changes; regions 3, 6 and 11 switch screens through a
  variable.
- Where the switches are drawn and which frame shows which state.
- How the five saved games (`SAVEGAME\\~~1.SAV` to `~~5.SAV`) are listed and loaded.
