---
id: SCR-UI-003
title: Character options
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010]
conflicting: []
split_with: []
related: [SCR-UI-004, SCR-UI-005]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#CHAR_OPS.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (132, 209, 175, 95) | Always | Switches to SCR-UI-004. | FND-UI-003 |
| region 1 | (192, 342, 223, 106) | Always | Switches to SCR-UI-005. | FND-UI-003 |
| region 2 | (75, 38, 231, 144) | Always | Runs a click routine that was not traced. | FND-UI-003 |
| region 3 | (501, 102, 34, 39) | Always | Calls `0x000145A4`, shared by regions 3 to 5; not traced. | FND-UI-003 |
| region 4 | (552, 102, 34, 39) | Always | Calls `0x000145A4`; not traced. | FND-UI-003 |
| region 5 | (525, 150, 34, 39) | Always | Calls `0x000145A4`; not traced. | FND-UI-003 |

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

- What region 2 and the shared routine of regions 3 to 5 do.
