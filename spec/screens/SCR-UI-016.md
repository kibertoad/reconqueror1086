---
id: SCR-UI-016
title: Tournament tents
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-TOURNEY-002, FND-TOURNEY-003, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013]
conflicting: []
split_with: []
related: [RULE-TOURNEY-002, RULE-TOURNEY-003, RULE-TOURNEY-004, SCR-UI-014]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#TENTS.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (235, 1, 180, 478) | Always | Switches to SCR-UI-014. | FND-UI-013 |
| region 1 | (0, 198, 74, 138) | Always | Plays the click sound and runs the action of regions 6 to 10 (RULE-TOURNEY-003, RULE-TOURNEY-004). | FND-UI-013, FND-TOURNEY-003 |
| region 2 | (75, 190, 82, 85) | Always | Plays the click sound and runs the action of regions 6 to 10 (RULE-TOURNEY-003, RULE-TOURNEY-004). | FND-UI-013, FND-TOURNEY-003 |
| region 3 | (164, 197, 62, 61) | Always | Plays the click sound and runs the action of regions 6 to 10 (RULE-TOURNEY-003, RULE-TOURNEY-004). | FND-UI-013, FND-TOURNEY-003 |
| region 4 | (449, 196, 43, 45) | Always | Plays the click sound and runs the action of regions 6 to 10 (RULE-TOURNEY-003, RULE-TOURNEY-004). | FND-UI-013, FND-TOURNEY-003 |
| region 5 | (498, 180, 84, 84) | Always | Plays the click sound and runs the action of regions 6 to 10 (RULE-TOURNEY-003, RULE-TOURNEY-004). | FND-UI-013, FND-TOURNEY-003 |
| region 6 | (29, 337, 57, 62) | Always | Runs the action of the tent choice (RULE-TOURNEY-003, RULE-TOURNEY-004); the right button shows the opponent (RULE-TOURNEY-002). | FND-TOURNEY-002, FND-TOURNEY-003 |
| region 7 | (141, 276, 30, 39) | Always | Runs the action of the tent choice (RULE-TOURNEY-003, RULE-TOURNEY-004); the right button shows the opponent (RULE-TOURNEY-002). | FND-TOURNEY-002, FND-TOURNEY-003 |
| region 8 | (213, 261, 22, 23) | Always | Runs the action of the tent choice (RULE-TOURNEY-003, RULE-TOURNEY-004); the right button shows the opponent (RULE-TOURNEY-002). | FND-TOURNEY-002, FND-TOURNEY-003 |
| region 9 | (421, 264, 25, 27) | Always | Runs the action of the tent choice (RULE-TOURNEY-003, RULE-TOURNEY-004); the right button shows the opponent (RULE-TOURNEY-002). | FND-TOURNEY-002, FND-TOURNEY-003 |
| region 10 | (457, 286, 57, 65) | Always | Runs the action of the tent choice (RULE-TOURNEY-003, RULE-TOURNEY-004); the right button shows the opponent (RULE-TOURNEY-002). | FND-TOURNEY-002, FND-TOURNEY-003 |
| region 11 | (527, 268, 617, 386) | Always | None; the setup binds no routine. | FND-UI-003 |

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

None known.
