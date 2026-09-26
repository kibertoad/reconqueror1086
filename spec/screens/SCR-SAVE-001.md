---
id: SCR-SAVE-001
title: Load game
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-SAVE-001]
conflicting: []
split_with: []
related: [RULE-SAVE-001, RULE-SAVE-003, SCR-UI-002]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#LOADGAME.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-SAVE-001 |
| Slot titles | None | `TITLE` of `SAVEGAME\CONQn.SAV`, by `read_slot_title` of RULE-SAVE-001 | (100, 138), (100, 183), (100, 228), (100, 273), (100, 318) | When the slot's file exists | FND-SAVE-001 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| slot 1 | (26, 128, 590, 35) | Always | Loads `CONQ1.SAV` (RULE-SAVE-003); leaves the screen only when that succeeds. | FND-SAVE-001 |
| slot 2 | (26, 173, 590, 35) | Always | Loads `CONQ2.SAV` (RULE-SAVE-003); leaves the screen only when that succeeds. | FND-SAVE-001 |
| slot 3 | (26, 218, 590, 35) | Always | Loads `CONQ3.SAV` (RULE-SAVE-003); leaves the screen only when that succeeds. | FND-SAVE-001 |
| slot 4 | (26, 263, 590, 35) | Always | Loads `CONQ4.SAV` (RULE-SAVE-003); leaves the screen only when that succeeds. | FND-SAVE-001 |
| slot 5 | (26, 308, 590, 35) | Always | Loads `CONQ5.SAV` (RULE-SAVE-003); leaves the screen only when that succeeds. | FND-SAVE-001 |
| back | (10, 383, 65, 60) | Always | Returns to SCR-UI-002. | FND-SAVE-001 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| `1` to `5` | Always | As a click on that slot. | FND-SAVE-001 |
| `Esc` | Always | As a click on back. | FND-SAVE-001 |

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

None.
