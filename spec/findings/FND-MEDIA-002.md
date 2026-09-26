---
id: FND-MEDIA-002
title: The release holds 66 CSF sprite files with 3,262 frames, all of the same layout
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B1
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0xA850D3..0xBB64D1
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0xC743E5..0xE069EC
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x111B818..0x11E5FB3
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x11E5FB4..0x12B0486
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x12B0487..0x137A003
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x21A0F58..0x21A2E65
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B0CFA6..0x1B0EFA7
  - build: BLD-GOG-EN
    file: CD:CONQUER/SKIRMISH.RES
    offset: 0x44040..0x59238
tool: Container census written for this project
environment: null
---

## Observation

`C1086.GOB` holds 65 entries named `.CSF`: five of kind 0 (`men8.CSF`, `credit.CSF`, `ica.CSF`,
`ics.CSF`, `icw.CSF`) and 60 of kind 1. `SKIRMISH.RES` holds one more, `skirmish.csf`, kind 1. Every
one of the 66 decoded entries:

- begins with the two bytes `2J`, a `UINT32LE` frame count and that many `UINT32LE` frame sizes, and
  the sizes add up exactly to the rest of the entry;
- has frames that begin with a `UINT16LE` width and height, then one row per height unit, each a
  segment count byte and that many segments of an operation byte and an `INT16LE` length, followed by
  `length` bytes for operation 0 and one byte for operation 2;
- uses only operations 0, 1 and 2, with lengths above 0, between 1 and 31 segments per row, and
  segment lengths that add up to the frame width in every one of the 267,374 rows;
- ends each frame exactly after its last row.

The 3,262 frames hold 225,692 segments of operation 0, 415,414 of operation 1 and 50,293 of
operation 2. The five kind-0 files:

| File | Frames | Smallest frame | Largest frame | Frame bytes | Sizes |
|---|---|---|---|---|---|
| `men8.CSF` | 723 | 71 | 3,412 | 1,247,405 | 720 at 90x90, one each at 3x9, 39x13 and 57x14 |
| `credit.CSF` | 32 | 50,717 | 52,472 | 1,648,002 | 275x190 |
| `ica.CSF` | 337 | 599 | 5,878 | 827,986 | 80x80 |
| `ics.CSF` | 337 | 599 | 5,878 | 827,273 | 80x80 |
| `icw.CSF` | 337 | 599 | 5,864 | 824,883 | 80x80 |

Together they hold 1,766 frames with 119,292, 254,433 and 19,573 segments of operations 0, 1 and 2.
Other frame counts: `CONFONT.CSF` 256 frames, all 13 high and 2 to 10 wide (174 at 8, 49 at 9);
`font.CSF` 256 frames of 11x13; `ffonta.CSF` 81 frames 5 to 11 wide; `swords.CSF` 39;
`lance1.CSF` 25; `moveobj.CSF` 57; `icon_men.CSF` 128 of 27x35; `warplan.CSF` 22; `mc.CSF` 70;
the 30 files `d121.CSF` to `d175.CSF` 12 frames of 99x149 each.

`skirmish.csf` has 53 frames: 0 to 23 are 96x43, 24 to 26 are 23x28, 27 to 42 have sizes between
91x148 and 246x71, 43 to 47 are 70x82 and 48 to 52 are 46x40. Drawn with `SKIRMISH.PAL`, frames 0 to
23 are weapons lying flat, 24 to 26 are three small shields (green, blue and red), 27 to 41 are five
groups of three views of a mailed hand holding an axe, a crossbow, a war hammer, a mace and a sword,
42 is a hand holding a dagger, and 43 to 47 and 48 to 52 are two five-frame groups of blood spatter.

The executable holds the names `CONFONT.CSF`, `icon_men`, `marker`, `moveobj`, `color`, `mc`,
`pregen`, `tent` and `things` with `.csf` or `.CSF` in its strings. `font.CSF`, `shield.CSF` and the
`textbox` files have no literal name there.

## Interpretation

CSF is the game's one sprite format (FMT-MEDIA-001, FMT-MEDIA-002). Operation 0 carries its own
pixels, operation 1 leaves the destination alone and operation 2 repeats one pixel (FND-MEDIA-003).

## Alternatives

The files without a literal name may be loaded through a formatted name or not at all; nothing here
tells which.

## How to reproduce

Decode each `.CSF` entry of `C1086.GOB` and `CD:CONQUER/SKIRMISH.RES` (RULE-RES-001) and walk the size
table and rows as described; draw `skirmish.csf` with `SKIRMISH.PAL` to see the groups.
