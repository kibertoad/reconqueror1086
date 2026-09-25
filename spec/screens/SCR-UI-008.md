---
id: SCR-UI-008
title: Practice grounds
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-BATTLE-001, FND-JOUST-001, FND-TOURNEY-007, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013]
conflicting: []
split_with: []
related: [RULE-BATTLE-001, RULE-JOUST-002, RULE-TOURNEY-005]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#PRACTICE.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (58, 250, 250, 138) | Always | Starts a practice field battle with drawn army sizes (RULE-BATTLE-001). | FND-UI-013, FND-BATTLE-001 |
| region 1 | (279, 140, 436, 70) | Always | Starts the practice joust (RULE-JOUST-002). | FND-UI-013, FND-JOUST-001 |
| region 2 | (374, 336, 202, 102) | Always | Starts the practice melee (RULE-TOURNEY-005). | FND-TOURNEY-007 |
| region 3 | (567, 239, 48, 18) | Always | Returns to the previous screen through `0x00059760`. | FND-UI-003 |
| region 4 | (1, 1, 234, 170) | Always | Starts the castle skirmish from `R121.RES`, `R131.RES` or `R141.RES`. | FND-UI-013 |

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

- How the army sizes of the practice battle are drawn.
