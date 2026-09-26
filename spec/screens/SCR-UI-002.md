---
id: SCR-UI-002
title: Game options
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-010, FND-UI-012]
conflicting: []
split_with: []
related: [SCR-UI-003, SCR-UI-008, SCR-UI-012, SCR-SAVE-001, SCR-SAVE-002]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#OPTFIN.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Switches | `C1086.GOB#OPTION.CSF`, frames 0 and 1 | On and off | (185, 118), (529, 120), (203, 47), (552, 52), (235, 200) | When a switch changes | FND-UI-012 |
| Movie and credits switches | `C1086.GOB#OPTION.CSF`, frames 2 and 3 | On and off | (494, 443), (349, 443) | When a switch changes | FND-UI-012 |
| Resume control | `C1086.GOB#OPTION.CSF`, frame 4 | None | Region 11 | Not traced | FND-UI-002, FND-UI-010 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (145, 90, 100, 65) | Always | Toggles the `SOUND_EFFECTS` setting and draws its switch (FND-UI-012). It needs `0x0009DBAC`; turning it off also turns speech off. | FND-UI-012 |
| region 1 | (500, 85, 85, 70) | Always | Toggles the `ANIMATIONS` setting and draws its switch (FND-UI-012). Turning it off also turns the movie and credits settings off. | FND-UI-012 |
| region 2 | (10, 20, 75, 65) | Always | Asks whether to quit to DOS; yes stores 2 in `0x0009ADC0`. | FND-UI-012 |
| region 3 | (15, 243, 255, 253) | Always | Resets the game and switches to SCR-UI-003. | FND-UI-012 |
| region 4 | (160, 0, 120, 85) | Always | Toggles the `CDMUSIC` setting and draws its switch (FND-UI-012). and starts or stops the CD music. | FND-UI-012 |
| region 5 | (340, 0, 145, 230) | Always | Switches to SCR-UI-008. | FND-UI-012 |
| region 6 | (310, 233, 100, 55) | Always | Runs SCR-SAVE-001; when it returns 1, switches to SCR-UI-012. | FND-UI-012 |
| region 7 | (411, 233, 100, 55) | Always | Runs SCR-SAVE-002 and redraws the screen. | FND-UI-012 |
| region 8 | (290, 320, 140, 175) | Always | Toggles the `CREDITS` setting and draws its switch (FND-UI-012). | FND-UI-012 |
| region 9 | (430, 320, 150, 175) | Always | Toggles the `MOVIE` setting and draws its switch (FND-UI-012). | FND-UI-012 |
| region 10 | (510, 15, 90, 70) | Always | Toggles the `MIDIMUSIC` setting and draws its switch (FND-UI-012). It needs `0x0009DBB4`. | FND-UI-012 |
| region 11 | (525, 155, 80, 40) | When the screen enables it | Returns to the previous screen when the byte at `0x0009ADFC` is 1. | FND-UI-012 |
| region 12 | (200, 160, 90, 60) | Always | Toggles the `DIG_SPEECH` setting and draws its switch (FND-UI-012). It needs `0x0009DBAC` and sound effects on. | FND-UI-012 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

| Sound | Resource | Played when | Evidence |
|---|---|---|---|
| Click | The sound at `0x0009ADF0` | Each click | FND-UI-012 |

## States

None known.

## Timing

None known.

## Differences between builds

None known.

## Open questions

- When the byte at `0x0009ADFC` is 1, and what shows region 11's frame.
