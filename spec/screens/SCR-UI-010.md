---
id: SCR-UI-010
title: War planning
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-ESTATE-002, FND-STRATEGY-035, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010]
conflicting: []
split_with: []
related: [RULE-ESTATE-001, RULE-STRATEGY-018]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#WARPLAN.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Army selectors | `C1086.GOB#WARPLAN.CSF`, frames 0 to 14 | The state of each of five armies | Regions 0 to 4 | On entry and after each change | FND-UI-010, FND-ESTATE-002 |
| Army controls | `C1086.GOB#WARPLAN.CSF`, frames 15 to 21 | None | Regions 5 to 7 | Not traced | FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (8, 0, 40, 38) | Always | Selects the army (RULE-ESTATE-001); the right button disbands it after a question. | FND-ESTATE-002 |
| region 1 | (23, 45, 40, 38) | Always | Selects the army (RULE-ESTATE-001); the right button disbands it after a question. | FND-ESTATE-002 |
| region 2 | (24, 91, 40, 38) | Always | Selects the army (RULE-ESTATE-001); the right button disbands it after a question. | FND-ESTATE-002 |
| region 3 | (7, 141, 40, 38) | Always | Selects the army (RULE-ESTATE-001); the right button disbands it after a question. | FND-ESTATE-002 |
| region 4 | (21, 185, 40, 38) | Always | Selects the army (RULE-ESTATE-001); the right button disbands it after a question. | FND-ESTATE-002 |
| region 5 | (8, 392, 114, 38) | Always | None; its routine returns at once. | FND-ESTATE-002 |
| region 6 | (2, 433, 138, 38) | Always | Sends out a spy (RULE-STRATEGY-018). | FND-STRATEGY-035 |
| region 7 | (224, 410, 198, 38) | Always | None; its routine returns at once. | FND-ESTATE-002 |
| region 8 | (115, 103, 300, 14) | Always | Adds a company of the row's kind to the selected army; the right button removes one (RULE-ESTATE-001). | FND-ESTATE-002 |
| region 9 | (115, 117, 300, 14) | Always | As region 8, for the second kind. | FND-ESTATE-002 |
| region 10 | (115, 131, 300, 14) | Always | As region 8, for the third kind. | FND-ESTATE-002 |
| region 11 | (538, 436, 57, 38) | Always | Cancels: returns to the previous screen through `0x00059760`. | FND-ESTATE-002 |
| region 12 | (550, 392, 55, 38) | Always | Commits the staged armies and spies (RULE-ESTATE-001) and returns. | FND-ESTATE-002 |
| region 13 | (315, 45, 100, 20) | Always | Edits the selected army's name, up to 15 characters. | FND-ESTATE-002 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| Enter (13) | Always | As region 12. | FND-ESTATE-002 |
| Escape (27) | Always | As region 11. | FND-ESTATE-002 |

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

- Which frame shows which army state.
