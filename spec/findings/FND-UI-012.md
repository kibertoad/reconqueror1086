---
id: FND-UI-012
title: The game options screen toggles six settings in CONQUER.INI and routes to new game, load, save, practice, resume and quit
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00037B28..0x000384DA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00037360..0x0003762B
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Every click routine of screen 1 first plays the sound the pointer at `0x0009ADF0` holds, when it is
not 0. A setting routine writes `ON` or `OFF` under its key in the INI file through
`0x00062D94(ini, key, value)`, draws a frame of the `OPTION.CSF` sprite set at `0x0009ADF8` through
`0x00065130(x, y, frame, set)`, and ends with `0x00063270(1, 0)`. The settings are:

| Region | Routine | Key | State | Drawn at | Frames on, off |
|---|---|---|---|---|---|
| 0 | `0x00037DC0` | `SOUND_EFFECTS` | `0x0009ADBC` | (185, 118) | 0, 1 |
| 1 | `0x00037C54` | `ANIMATIONS` | `0x0009ADB8` | (529, 120) | 0, 1 |
| 4 | `0x00037F14` | `CDMUSIC` | `0x0005B368()` | (203, 47) | 0, 1 |
| 8 | `0x00038330` | `CREDITS` | `0x0009ADC8` | (349, 443) | 2, 3 |
| 9 | `0x000383F0` | `MOVIE` | `0x0009ADC4` | (494, 443) | 2, 3 |
| 10 | `0x00038244` | `MIDIMUSIC` | `0x0009ADCC` | (552, 52) | 0, 1 |
| 12 | `0x00037FDC` | `DIG_SPEECH` | `0x0009ADD4` | (235, 200) | 0, 1 |

Turning sound effects on requires the dword at `0x0009DBAC` to be not 0; otherwise `0x00025004`
shows the text at object-2 offset `0x4E34` and the switch stays off. Turning them off also turns
digitized speech off. Turning animations off also turns the movie and credits settings off. Turning
digitized speech on requires `0x0009DBAC` (text `0x4E6C` otherwise) and sound effects on (text
`0x4EBC` otherwise). Turning MIDI music on requires the dword at `0x0009DBB4` (text `0x4F04`
otherwise). Turning CD music on calls `0x00072380` the first time, marked by `0x0009DBB0`, and then
`0x0005B370(1)`; turning it off calls `0x0005B370(0)`.

Region 2 (`0x00037BFC`) asks the question at `0x4DE0` through `0x00025004` and, on yes, stores -1 in
`0x0009AAC4` and 2 in `0x0009ADC0`. Region 3 (`0x00037B28`) shows pointer frame 1 and, when
`0x0009ADD0` is 0, resets the game through `0x00042E4C`, `0x00010FF0` and `0x0002A1A0`, frees the
buffer at `0x0009A928`, and opens the war resource through `0x00049200`, stopping with the text at
`0x4DB0` when that fails; it then stores -1 in `0x0009AAC4`, 0 in `0x0009ADD0` and `0x0009ADC0` and
calls `0x000596C0(2, 1, 1)`. Region 5 (`0x000384B0`) calls `0x000596C0(15, 0, 1)`. Region 6
(`0x00038110`) runs the leave routine `0x000379CC` and then `0x0004B9EC`; when that returns 1 it
stores 0 in `0x0009ADD0` and `0x0009DED4`, reruns the entry routine `0x0003762C`, stores 1 in
`0x0009DEF8` and calls `0x000596C0(23, 0, 1)`. Region 7 (`0x000381C4`) runs `0x000379CC`,
`0x0004B4E4` and `0x00040ED0(0, 0, -1, 0x00059CDC(1))` and reruns `0x0003762C`. Region 11
(`0x0003822C`) calls `0x00059760(1, 1)` when the byte at `0x0009ADFC` is 1. The HAT file marks
region 11 disabled.

## Interpretation

Regions 0, 1, 4, 8, 9, 10 and 12 are the seven setting switches; region 2 quits to DOS, region 3
starts a new game at the character options, region 5 opens the practice grounds, region 6 loads a
saved game and goes to the estate map, region 7 saves, and region 11 resumes the game in progress.

## Alternatives

That `0x0004B9EC` loads and `0x0004B4E4` saves rests on the destinations and the screen's pictures;
neither routine was traced.

## How to reproduce

Disassemble `0x00037B28` to `0x000384DA` and read the strings at the object-2 offsets pushed.
