---
id: SCR-UI-007
title: Forge and store
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-TALK-007, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-007, FND-UI-008, FND-UI-010]
conflicting: []
split_with: []
related: [RULE-UI-003, SCR-UI-006, SCR-UI-013]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#FORGESMI.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Store picture | Picture `0x13F` | None | (0, 0, 640, 480) | While the store is open | FND-UI-007 |
| Item picture | `C1086.GOB#SWORDS.CSF` | The selected entry's frame | Not traced | While the store is open | FND-UI-007 |
| Store controls | `C1086.GOB#BUYSELL.CSF` | None | Not traced | While the store is open | FND-UI-007 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (253, 109, 107, 164) | Always | Starts the smith's conversation, SCR-UI-013. | FND-UI-003, FND-TALK-007 |
| region 1 | (1, 258, 631, 212) | Always | Switches to SCR-UI-006. | FND-UI-003 |
| region 2 | (54, 2, 180, 141) | Always | Opens the store: turns regions 0 to 2 off and 3 to 6 on, and stocks it by RULE-UI-003. | FND-UI-003, FND-UI-007 |
| region 3 | (427, 422, 100, 37) | When the screen enables it | Calls `0x00061AFC`, shared with regions 4, 7 and 8; not traced. | FND-UI-003 |
| region 4 | (540, 420, 55, 37) | When the screen enables it | Calls `0x00061AFC`; not traced. | FND-UI-003 |
| region 5 | (246, 410, 36, 41) | When the screen enables it | Calls `0x00061F70`, shared with region 6; not traced. | FND-UI-003 |
| region 6 | (303, 410, 36, 41) | When the screen enables it | Calls `0x00061F70`; not traced. | FND-UI-003 |
| region 7 | (347, 407, 81, 40) | Always | Calls `0x00061AFC`; not traced. | FND-UI-003 |
| region 8 | (347, 450, 81, 38) | Always | Calls `0x00061AFC`; not traced. | FND-UI-003 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Forge | The screen is entered | Region 2 is clicked | FND-UI-007 |
| Store | Region 2 is clicked | Not traced | FND-UI-007 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- What the store's regions 3 to 8 do: browsing, buying, selling and leaving.
- Which of the frames of `BUYSELL.CSF` each control state shows.
- Whether picture `0x13F` is archive entry 319 of `C1086.GOB`, `swdtemp.pcx`, the empty store frame.
