---
id: SCR-UI-015
title: Tournament stands
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-TALK-007, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013]
conflicting: []
split_with: []
related: [RULE-TALK-004, SCR-UI-013, SCR-UI-014]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#JLADY.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (29, 235, 36, 171) | Always | Talks to the lady of the region: stores her partner number from `0x0009DC0C` (RULE-TALK-004), stores 1 in the byte at `0x0009DC24` and switches to SCR-UI-013. | FND-TALK-007 |
| region 1 | (66, 190, 57, 216) | Always | Talks to the lady of the region: stores her partner number from `0x0009DC0C` (RULE-TALK-004), stores 1 in the byte at `0x0009DC24` and switches to SCR-UI-013. | FND-TALK-007 |
| region 2 | (124, 179, 76, 228) | Always | Talks to the lady of the region: stores her partner number from `0x0009DC0C` (RULE-TALK-004), stores 1 in the byte at `0x0009DC24` and switches to SCR-UI-013. | FND-TALK-007 |
| region 3 | (200, 156, 85, 251) | Always | Talks to the lady of the region: stores her partner number from `0x0009DC0C` (RULE-TALK-004), stores 1 in the byte at `0x0009DC24` and switches to SCR-UI-013. | FND-TALK-007 |
| region 4 | (285, 102, 120, 315) | Always | Talks to the lady of the region: stores her partner number from `0x0009DC0C` (RULE-TALK-004), stores 1 in the byte at `0x0009DC24` and switches to SCR-UI-013. | FND-TALK-007 |
| region 5 | (410, 33, 172, 360) | Always | Talks to the lady of the region: stores her partner number from `0x0009DC0C` (RULE-TALK-004), stores 1 in the byte at `0x0009DC24` and switches to SCR-UI-013. | FND-TALK-007 |
| region 6 | (535, 420, 53, 23) | Always | Stores 0 in the byte at `0x0009DC24` and switches to SCR-UI-014. | FND-UI-013 |

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
