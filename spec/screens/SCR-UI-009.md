---
id: SCR-UI-009
title: Castle office
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010]
conflicting: []
split_with: []
related: [SCR-UI-010, SCR-UI-011, SCR-UI-012]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#TACTICAL.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (248, 184, 64, 31) | Always | Switches to screen 17 (`FOVIEW.HAT`). | FND-UI-003 |
| region 1 | (172, 153, 72, 44) | Always | Switches to SCR-UI-011, screen 19. | FND-UI-003 |
| region 2 | (277, 146, 17, 38) | Always | Switches to SCR-UI-011, screen 21. | FND-UI-003 |
| region 3 | (295, 141, 18, 43) | Always | Switches to SCR-UI-011, screen 20. | FND-UI-003 |
| region 4 | (314, 147, 15, 39) | Always | Switches to SCR-UI-011, screen 22. | FND-UI-003 |
| region 5 | (359, 123, 35, 76) | Always | Switches to SCR-UI-010. | FND-UI-003 |
| region 6 | (0, 113, 66, 210) | Always | Switches to SCR-UI-012. | FND-UI-003 |
| region 7 | (568, 84, 72, 200) | Always | None; the setup binds no routine in any slot. | FND-UI-003 |
| region 8 | (200, 55, 68, 85) | Always | Switches to screen 17 (`FOVIEW.HAT`). | FND-UI-003 |
| region 9 | (332, 180, 26, 37) | Always | Runs a click routine that was not traced. | FND-UI-003 |

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

- What region 9 does.
