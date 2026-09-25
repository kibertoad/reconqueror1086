---
id: SCR-UI-005
title: Pre-generated characters
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010]
conflicting: []
split_with: []
related: []
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#PREGEN.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (69, 18, 119, 133) | Always | Calls `0x000426B8`, shared by all six, which switches screens through a variable. | FND-UI-003 |
| region 1 | (256, 18, 119, 133) | Always | Calls `0x000426B8`, shared by all six, which switches screens through a variable. | FND-UI-003 |
| region 2 | (439, 18, 119, 133) | Always | Calls `0x000426B8`, shared by all six, which switches screens through a variable. | FND-UI-003 |
| region 3 | (69, 250, 119, 133) | Always | Calls `0x000426B8`, shared by all six, which switches screens through a variable. | FND-UI-003 |
| region 4 | (256, 250, 119, 133) | Always | Calls `0x000426B8`, shared by all six, which switches screens through a variable. | FND-UI-003 |
| region 5 | (439, 250, 119, 133) | Always | Calls `0x000426B8`, shared by all six, which switches screens through a variable. | FND-UI-003 |

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

- Which character each region picks and which screen follows.
