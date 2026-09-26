---
id: FND-SAVE-001
title: The load and save screens list five titled slots, SAVEGAME\CONQ1.SAV to CONQ5.SAV
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004B4E4..0x0004B9D0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004B9EC..0x0004BDF1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004B198..0x0004B278
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004B290..0x0004B326
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004B328..0x0004B4CB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004A960..0x0004A9BF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The save screen `0x0004B4E4` and the load screen `0x0004B9EC` share one layout. Each copies six
rectangles of four `INT16` (x, y, width, height) from `0x0004A960` (save) or `0x0004A990` (load); both
tables hold (26, 128, 590, 35), (26, 173, 590, 35), (26, 218, 590, 35), (26, 263, 590, 35),
(26, 308, 590, 35) and (10, 383, 65, 60). Each shows picture `0x185` (`savegame.pcx`) or `0x184`
(`loadgame.pcx`) through `0x00024DB8(1, index, 0)`, closes the open archive with `0x00049478(0)` and
calls `0x0004B290(0)`.

`0x0004B290(skip)` draws, for each slot from 1 to 5 other than `skip`, the title `0x0004B198` gives
at (100, 138 + 45 * (slot - 1)) in colour 255 through `0x000644D4`. `0x0004B198(out, slot)` builds
`SAVEGAME\CONQ` + the slot in decimal + `.SAV` through `0x000642CC` and `0x000641A0`, and when
`0x0006A190` finds the file opens it as an archive with `0x00049200(path, "r+b")`, reads its `TITLE`
entry into `out` through `0x0004957C` and `0x000495F4`, and closes it; a missing entry or failed
read stops the game with `Error in loading game data. Aborting.` (code 0). A missing file gives an
empty title.

Each pass of a screen's loop tests the pointer (`0x000B07E8`, `0x000B07EC`) against the six
rectangles through `0x0006FF10`, showing pointer frame 5 over a rectangle and frame 0 elsewhere,
and reads the next event through `0x00063114`. A waiting key (`0x00065380`, `0x00065388`) of `1` to
`5` picks that slot and `Esc` picks rectangle 6; an event of type 2 or 3 picks the rectangle under
the pointer. Rectangle 6 leaves the screen.

On the save screen a slot reopens `C1086.GOB` (`0x000A9CF0`, mode `rb`), shows the picture again,
closes it, redraws the other titles with `0x0004B290(slot)`, reads the slot's title and lets the
player edit it at (100, 138 + 45 * (slot - 1)) through `0x0004B328`, then shows pointer frame 1 and
calls `0x0004A9C0("CONQn.SAV", title)` (FND-SAVE-002) and leaves. On the load screen a slot shows
pointer frame 1 and calls `0x0004ADD4("CONQn.SAV")` (FND-SAVE-003); it leaves the screen only when
that returns 1. Both screens reopen `C1086.GOB` on the way out (`Not enough memory to load war Res.
Aborting.` on failure) and show pointer frame 0. The load screen returns 1 unless rectangle 6 ended
it.

`0x0004B328(x, y, text)` saves the screen under (x, y, screen width - x, 50) through `0x00064320`
and draws `text` followed by `-`. Each waiting key below 256 other than Enter is handled: Backspace
removes the last character when there is one; any other key, Backspace on an empty text included,
is appended while the text is shorter than 40 characters, and at 40 the routine calls
`0x0006576E(300)`, `0x000657C0(100)` and `0x000657B1`. Keys of 256 and above are ignored. Enter
ends the edit; there is no way to cancel it.

## Interpretation

The game has five save slots, files `CONQ1.SAV` to `CONQ5.SAV` in `SAVEGAME\` under the current
directory, each with a title of up to 40 characters that the player types when saving. The slot
list is the titles read from the files; an empty slot shows nothing. Picking a slot on the save
screen commits to saving there: `Esc` in the title editor is typed as a character. At 40 characters
the PC speaker beeps at 300 Hz for 100 ms. `~~1.SAV` to `~~5.SAV` are the temporary files a save
is built from (FND-SAVE-002), not slots.

## Alternatives

That event types 2 and 3 are button presses rests on their use here; `0x00063114` was not traced.
That `0x0006576E`, `0x000657C0` and `0x000657B1` are `sound`, `delay` and `nosound` rests on the
arguments.

## How to reproduce

Disassemble the listed ranges; decode the rectangle tables at file offsets `0x86BB4` and `0x86BE4`
(object 1 offsets `0x3A960` and `0x3A990`); look up entries `0x184` and `0x185` of `C1086.GOB`.
