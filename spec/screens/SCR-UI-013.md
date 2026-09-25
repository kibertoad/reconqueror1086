---
id: SCR-UI-013
title: Conversation
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-TALK-007, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-009, FND-UI-010]
conflicting: []
split_with: []
related: [RULE-TALK-004]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#COMSCRN1.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Conversation picture | Picture `0x12E` | None | (0, 0, 640, 480) | While a conversation runs | FND-UI-009 |
| Prompt window | Font not traced | The node prompt (RULE-TALK-004) | (264, 32, 322, 215) | While a conversation runs | FND-UI-009 |
| Name window | Font not traced | Not traced | (34, 236, 205, 20) | While a conversation runs | FND-UI-009 |
| Response rows | Font not traced | The responses, one to a row | (41, 302 + 35 * k, 558, 35) for k from 0 to 4 | While the node has responses | FND-UI-009 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (0, 0, 640, 480) | Always | None; the setup binds no region routine. The response picker tests the pointer against the rows itself. | FND-UI-003, FND-UI-009 |

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

- Which keys pick a response.
- What the name window shows.
