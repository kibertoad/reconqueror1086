---
id: FND-MEDIA-009
title: Movies play through a Smacker wrapper at 0x0002FCB0 and a frame loop at 0x0002FCF0 that a callback can stop
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002FCB0..0x0002FCEC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002FCF0..0x0002FED8
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00030100..0x0003029D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024D14..0x00024D53
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00061D67..0x00061D8B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00010631..0x00010652
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E83F..0x0003E860
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B91E..0x0005B92A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002ACBE..0x0002ACCA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001A34F..0x0001A35D
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0002FCB0(path, flags, extra)` reads the `CONQUER.INI` key `DIG_SPEECH` through `0x00062EA0`;
when it exists and compares equal to `OFF` through `0x00069890`, it keeps only bit 8 of the second
flags byte, so the `0x200` its callers pass becomes 0. It then opens the movie with `0x0006C287(path,
flags, extra)` and returns the movie object, or 0. Its nine callers pass flags `0x200`, except
`0x0002ACCA` and `0x0005B92A`, which pass `0x80`, and all pass `extra` -1.

`0x0002FCF0(movie, x, y, repeats, set_palette, stop, direct)` returns 0 at once for a null movie,
a negative `x` or `y`, or when `x` plus the movie width is above `0x000B0854`. A `repeats` below 1
counts as 1. When `direct` is not 0 it asks the movie library to draw to the screen at `(x, y)`;
otherwise to the buffer at `0x000B0840`, sized by `0x000B0854` and `0x000B0868`. For each frame: when the frame
carries a palette and `set_palette` is 1, it copies the movie's 256 colours at movie `+0x70` into the
palette buffer at `0x000B0844` through `0x0006DF1C`, as the PCX loader does (FND-MEDIA-005), and
`0x000300BC` then shifts those 768 colour bytes at movie `+0x70` right by 2 in place (the bytes at
`+0x374` when the dword at `+0x6C` is not 1). It then decodes the frame, and when `direct` is 0 copies the changed rectangles out. After each frame it
advances; when the frame counter at movie `+0x678` reaches the frame count at `+0x0C` minus 1, it
counts down `repeats` and stops at 0. Before each wait it calls `stop(frame)` when given, and stops
when that returns 1. It closes the movie at the end and returns the number of frames shown.

`0x00030100(name)` plays a movie full screen: it switches to a 320x200 mode, builds the path from
the `CONQUER.INI` key `CD_PATH` (or `.\CD\` when missing) and `name`, opens it with
`0x0002FCB0(path, 0x200, -1)` (stopping the game with `Unable to open Smack movie` on failure),
and plays it once centred, at `((320 - width) / 2, (200 - height) / 2)`, with palette, stop callback
`0x00024D14` and `direct` 1. It then calls `0x00072119(1)`, and `0x00072119(3)` as well when the key `DELAYVGA` is `ON`,
switches back to 640x480 and stops the game with `Conq-Vid: Initializing video system. Aborting.` when that fails.
It has five callers.

`0x00024D14` reads the next input event through `0x00063114` and returns 1 for event 3 or 7 or when
`0x00065380` reports a key, after clearing the input; otherwise 0.

The store preview at `0x00061D67` opens its movie with `0x0002FCB0` and plays it at (28, 27) with 10
repeats, no palette and the same callback. The callers at `0x00010652` and `0x0003E860` play at
(100, 100) once with no palette and no callback, and the conversation movie at `0x0001A35D` plays
at (40, 31) once with the callback.

## Interpretation

The game plays Smacker files through the Smacker library linked into the executable
(`0x0006C287` opens a file). `DIG_SPEECH=OFF` opens movies without their sound track. A click or
key stops the full-screen movies and the store preview; the movies drawn at (100, 100) cannot be
skipped.

## Alternatives

Events 3 and 7 are taken to be mouse button presses; the event codes of `0x00063114` were not
traced.

## How to reproduce

Disassemble the listed addresses in `CD:CONQUER.EXE`; read the strings at `0x000941E0` to
`0x00094238`.
