---
id: SCR-UI-007
title: Forge and store
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
resolution: 640x480
evidence: [FND-TALK-007, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-007, FND-UI-008, FND-UI-010, FND-UI-011, FND-UI-014]
conflicting: []
split_with: []
related: [RULE-UI-003, RULE-UI-004, SCR-UI-006, SCR-UI-013]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `C1086.GOB#FORGESMI.PCX` | None | (0, 0, 640, 480) | While the screen is shown | FND-UI-001, FND-UI-002, FND-UI-010 |
| Forge without the smith | `C1086.GOB#forge.pcx`, picture `0x13A` | None | (0, 0, 640, 480) | When `tournament_here` is 0 | FND-UI-011, FND-UI-014 |
| Store frame | `C1086.GOB#swdtemp.pcx`, picture `0x13F` | None | (0, 0, 640, 480) | While the store is open | FND-UI-007, FND-UI-011 |
| Item picture | `C1086.GOB#SWORDS.CSF` | The entry's frame | (28, 27) with a movie, (32, 27) without | While the store is open | FND-UI-014 |
| Buy or sell control | `C1086.GOB#BUYSELL.CSF`, frame 2 (owned) or 3 | None | (426, 425) | While the store is open | FND-UI-014 |
| Price | Font not traced | The sale price, `price - price / 4`, at (409, 384), or the price at (521, 384) | (409, 364) or (521, 364) | While the store is open | FND-UI-014 |
| View control | `C1086.GOB#BUYSELL.CSF`, frame 1 (movie) or 0 | None | (347, 407) | While the store is open | FND-UI-014 |
| Wealth | Font not traced | Field 17 of row 0 | (285, 364) | While the store is open | FND-UI-014 |
| Description | `FFONTA2.FNT` | The entry's description | Not traced | After browsing | FND-UI-014 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| region 0 | (253, 109, 107, 164) | Always | Starts the smith's conversation, SCR-UI-013. Disabled when `tournament_here` is 0. | FND-TALK-007, FND-UI-014 |
| region 1 | (1, 258, 631, 212) | Always | Switches to SCR-UI-006. With a tournament in town its rectangle is (1, 371, 632, 106). | FND-UI-014 |
| region 2 | (54, 2, 180, 141) | Always | Opens the store: turns regions 0 to 2 off and 3 to 6 on, and stocks it by RULE-UI-003. Without a tournament its rectangle is (54, 2, 280, 240). | FND-UI-007, FND-UI-014 |
| region 3 | (427, 422, 100, 37) | When the screen enables it | Buys or sells the shown item (RULE-UI-004). | FND-UI-014 |
| region 4 | (540, 420, 55, 37) | When the screen enables it | Closes the store: turns regions 0 to 2 on and 3 to 6 off and redraws the forge. | FND-UI-014 |
| region 5 | (246, 410, 36, 41) | When the screen enables it | Shows the previous entry, wrapping to the last. | FND-UI-014 |
| region 6 | (303, 410, 36, 41) | When the screen enables it | Shows the next entry, wrapping to the first. | FND-UI-014 |
| region 7 | (347, 407, 81, 40) | Always | Plays the entry's movie from `CD_PATH`, when it has one. | FND-UI-014 |
| region 8 | (347, 450, 81, 38) | Always | None; its routine returns at once. | FND-UI-014 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

| Sound | Resource | Played when | Evidence |
|---|---|---|---|
| Click | `C1086.GOB#vsmith.666`, resource `0x165` | Each click | FND-UI-011, FND-UI-014 |

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Forge | The screen is entered, or region 4 closes the store | Region 2 is clicked | FND-UI-007, FND-UI-014 |
| Store | Region 2 is clicked | Region 4 is clicked | FND-UI-007, FND-UI-014 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Where the description window is placed.
