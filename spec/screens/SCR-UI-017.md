---
id: SCR-UI-017
title: Inn
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-TALK-007, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013]
conflicting: []
split_with: []
related: [RULE-TALK-004, SCR-UI-006, SCR-UI-013]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#INNPEOPL.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (3, 233, 38, 70) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 1 | (51, 221, 35, 74) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 2 | (98, 156, 42, 102) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 3 | (226, 193, 47, 60) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 4 | (435, 96, 70, 70) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 5 | (456, 176, 66, 217) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 6 | (185, 157, 28, 101) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 7 | (241, 333, 121, 102) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 8 | (146, 267, 72, 81) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 9 | (22, 307, 78, 93) | Always | Talks to the patron of the region when the tournament is here or the partner is 21 (RULE-TALK-004), switching to SCR-UI-013. | FND-TALK-007 |
| region 10 | (1, 437, 634, 41) | Always | Stores 0 in the byte at `0x0009DEA8` and switches to SCR-UI-006. | FND-UI-013 |
| region 11 | (1, 437, 634, 41) | Always | As region 10. | FND-UI-013 |

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
