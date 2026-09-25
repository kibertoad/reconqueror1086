---
id: FND-UI-013
title: Character creation, practice and the tournament screens route between screens 3, 6, 8 to 12 and 24
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000143A0..0x000145A0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014CE0..0x00014E64
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014E68..0x00014FB9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014FBC..0x00015053
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000151F8..0x000153B7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041F68..0x00042011
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004215C..0x000422B1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000422B4..0x000423EA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005BE78..0x0005BF4F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C2AC..0x0005C2DA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005CBF8..0x0005CC22
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000604AC..0x000604F2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018B4C..0x00018B5A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019C80..0x00019CB1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00060600..0x00060600
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Character options, screen 2: region 2 (`0x000143A0`) reads keys through `0x00065380` and
`0x00065388` until Enter, keeps up to 20 characters, removes one on key 8, stores the text as row 0's
name through `0x00015EBC(0, name)` (a blank default from object-2 offset `0x770` when no key was
typed) and draws it centred on x 222.

Youth dilemma, screen 3: regions 1 (`0x00014CE0`) and 2 (`0x00014E68`) apply the second and third
answers as region 0 does for the first (FND-PERSON-005), draw the answer's picture at (269, 310) or
(478, 310), enable region 5 and disable regions 0 to 2 through `0x00059D34`. Regions 3
(`0x00014FBC`) and 4 (`0x00015008`) move the text window at `0x0009A5B8` five lines up through
`0x00065F50` or down through `0x00065F14` and redraw it. Region 6 (`0x000151F8`) rerolls: it keeps the
name and field 19 of row 0, reloads the table through `0x00015AA0` and `0x00015920`, loads dilemma
`0x00024C38(4)`, draws the three answer pictures at (62, 310), (269, 310) and (478, 310), disables
region 5 and enables regions 0 to 2. Region 5 is the continue routine of FND-PERSON-004.

Practice, screen 15: region 0 (`0x000422B4`) draws army sizes from `0x00024C38` and starts a field
battle through `0x000258FC`; region 1 (`0x00041F68`) runs `0x00041CEC`, the practice joust of
FND-JOUST-001; region 4 (`0x0004215C`) builds the name `R1` + (`0x00024C38(2)` + 2) + `1.RES` and
passes it to `0x0005921C`; all three then restore music `0x0009AAC4` (or `0x178`) and redraw screen
15. The disc holds `R121.RES`, `R131.RES` and `R141.RES`.

Tournament grounds, screen 8: region 0 (`0x0005BE78`) stores 1 in `0x0009DBD0` and -1 in
`0x0009AAC4` and calls `0x000596C0(11, 0, 1)`; region 2 (`0x0005BEC0`) calls `0x000596C0(9, 1, 1)`;
regions 3 and 4 (`0x0005BEFC`) store 0 or 1 in `0x000AFF44` and call `0x000596C0(10, 1, 1)`. Region 1
has no routine. Stands, screen 9: region 6 (`0x0005C2AC`) stores 0 in the byte at `0x0009DC24` and
calls `0x000596C0(8, 1, 1)`. Tents, screen 10: region 0 (`0x0005CBF8`) calls `0x000596C0(8, 1, 1)`, and regions 1 to 5
(`0x0005CA70`) play the sound at `0x000AFF40` and run on into `0x0005CA8C`, the routine of regions 6
to 10 (FND-TOURNEY-003), with no return between them.
Inn, screen 12: regions 10 and 11 (`0x000604AC`) store 0 in the byte at `0x0009DEA8`, show pointer
frame 0 and call `0x000596C0(11, 1, 1)`. Screen 5 (`CWAR.HAT`): region 0 (`0x00018B4C`) calls
`0x000596C0(6, 1, 1)`. Screen 6 (`DKING.HAT`): region 0 (`0x00019C80`) stores 0 in `0x0009A708` and
calls `0x000596C0(11, 0, 1)`. Every routine of screen 14 (`VMONYLND.HAT`) is `0x00060600`, which only
returns.

## Interpretation

The tournament grounds lead back to the village, to the stands and to the tents; the inn's exit
returns to the village; the dubbing screen, reached from the youth dilemmas, the pre-generated
knights and screen 5, leads to the village exterior. Practice region 4 is the castle skirmish, a
first-person scene from one of three archives.

## Alternatives

What screen 5 shows, and which routine switches to it, was not traced; `CHRCH1.PCX`, its background,
is missing from the archive.

## How to reproduce

Disassemble the listed routines and the pregenerated and continue routines of FND-PERSON-004 and
FND-PERSON-006, and search the disc image directory for `R1?1.RES`.
