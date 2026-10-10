---
id: SCR-UI-004
title: Youth dilemma
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-PERSON-004, FND-PERSON-005, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013, FND-UI-021]
conflicting: []
split_with: []
related: [RULE-PERSON-003, RULE-PERSON-004, SCR-UI-019]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#MORALITY.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Choice pictures | The dilemma sequences, 99 by 149 frames | The three answers | (62, 310), (269, 310), (478, 310) | While a dilemma is shown | FND-UI-010, FND-UI-013 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (62, 310, 100, 150) | Always | Applies the first answer (RULE-PERSON-004), enables region 5 and disables regions 0 to 2. | FND-PERSON-005 |
| region 1 | (268, 310, 100, 150) | Always | Applies the second answer, enables region 5 and disables regions 0 to 2. | FND-UI-013 |
| region 2 | (478, 310, 100, 150) | Always | Applies the third answer, enables region 5 and disables regions 0 to 2. | FND-UI-013 |
| region 3 | (3, 184, 632, 16) | Always | Moves the text five lines one way. | FND-UI-013 |
| region 4 | (3, 289, 632, 16) | Always | Moves the text five lines the other way. | FND-UI-013 |
| index 5 (stored ID 6) | (473, 132, 97, 40) | The resource starts enabled; answer callbacks enable this index and Reroll disables it. | Continues: at age 18 switches to SCR-UI-019, otherwise loads the next dilemma. | FND-PERSON-004, FND-UI-013, FND-UI-021 |
| index 6 (stored ID 5) | (470, 48, 110, 30) | Always | Rerolls the statistics and the dilemmas (RULE-PERSON-003), keeping the name and field 19. | FND-UI-013, FND-UI-021 |

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

- Which of regions 3 and 4 scrolls up. (Q-UI-013)
