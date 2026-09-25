---
id: SCR-UI-003
title: Character options
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-PERSON-004, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-013]
conflicting: []
split_with: []
related: [SCR-UI-004, SCR-UI-005, RULE-PERSON-003]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#CHAR_OPS.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Name | Font not traced | Row 0's name | Centred on x 222 | After the name is entered | FND-UI-013 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (132, 209, 175, 95) | Always | Switches to SCR-UI-004. | FND-UI-003 |
| region 1 | (192, 342, 223, 106) | Always | Switches to SCR-UI-005. | FND-UI-003 |
| region 2 | (75, 38, 231, 144) | Always | Reads the knight's name from the keyboard. | FND-UI-013 |
| region 3 | (501, 102, 34, 39) | Always | Stores 0 in field 19 of row 0 (RULE-PERSON-003) and draws the choice. | FND-PERSON-004 |
| region 4 | (552, 102, 34, 39) | Always | Stores 3 in field 19 of row 0 and draws the choice. | FND-PERSON-004 |
| region 5 | (525, 150, 34, 39) | Always | Stores 5 in field 19 of row 0 and draws the choice. | FND-PERSON-004 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| Printable keys | While region 2 reads the name | Adds the character, up to 20. | FND-UI-013 |
| Backspace (8) | While region 2 reads the name | Removes the last character. | FND-UI-013 |
| Enter (13) | While region 2 reads the name | Ends the name. | FND-UI-013 |

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
