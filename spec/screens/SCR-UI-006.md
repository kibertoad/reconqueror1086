---
id: SCR-UI-006
title: Village exterior
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-004, FND-UI-005, FND-UI-008, FND-UI-010]
conflicting: []
split_with: []
related: [RULE-UI-001, RULE-UI-002]
---

## Drawn elements

The background shown is the catalog record's picture; `VOPTS.HAT` supplies the regions.

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#TOWN_Y.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Label strip | Font not traced | The name of the place's person, or the hovered region's label | (230, 455) | Always; the label while the pointer is over regions 0 to 5 | FND-UI-004 |
| Pointer | `C1086.GOB#FFMOUSE.CSF`, frame 5 | None | At the pointer | While the pointer is over regions 0 to 5 | FND-UI-004, FND-UI-008 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (4, 319, 137, 371) | Always | RULE-UI-002; the rectangle and enabled state come from the catalog (RULE-UI-001). | FND-UI-004, FND-UI-005 |
| region 1 | (541, 347, 100, 54) | Always | RULE-UI-002; the rectangle and enabled state come from the catalog (RULE-UI-001). | FND-UI-004, FND-UI-005 |
| region 2 | (157, 166, 73, 39) | Always | RULE-UI-002; the rectangle and enabled state come from the catalog (RULE-UI-001). | FND-UI-004, FND-UI-005 |
| region 3 | (388, 229, 43, 30) | Always | RULE-UI-002; the rectangle and enabled state come from the catalog (RULE-UI-001). | FND-UI-004, FND-UI-005 |
| region 4 | (225, 257, 35, 19) | Always | RULE-UI-002; the rectangle and enabled state come from the catalog (RULE-UI-001). | FND-UI-004, FND-UI-005 |
| region 5 | (489, 192, 52, 253) | Always | RULE-UI-002; the rectangle and enabled state come from the catalog (RULE-UI-001). | FND-UI-004, FND-UI-005 |
| region 6 | (471, 185, 71, 39) | Always | None; the setup binds no routine. | FND-UI-003 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

| Sound | Resource | Played when | Evidence |
|---|---|---|---|
| Click | Resource `0x164`, loaded on entry | Each click on regions 0 to 5 | FND-UI-004, FND-UI-005 |

## States

None known.

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Whether the background drawn is the catalog record's picture or `TOWN_Y.PCX`; the entry routine was not traced that far.
