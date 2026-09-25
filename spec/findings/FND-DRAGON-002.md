---
id: FND-DRAGON-002
title: The dragon media routine adds 2 to the lance copy on a win and plays the win or loss movie
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B3AC..0x0001B580
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001BA6F..0x0001BAB6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001BB1B..0x0001BB5A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B6A0..0x0001B6AA
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0001B3AC(lance)` reads the environment variable `DIG_SPEECH` and uses movie flag `0x200` when it
equals `ON`, and reads `CD_PATH`, using an empty prefix when it is not set. It opens
`%sDRJSTRUN.SMK`, `%sDRJSTLSE.SMK` and `%sDRJSTWIN.SMK` through `0x0006C287`, and exits the game
through `0x00064FF5(1)` when one fails to open. It formats `%sDRRIDBYE.SMK` at `0x0001B485` but never
opens it. It calls the run worker `0x0001B584` with the run movie and `lance`. When the worker
returns 0 it adds 2 to the dword `lance` points to and plays the win movie through `0x0001B294`;
otherwise it plays the loss movie. It closes the three movies, then loops until `0x00063114` returns
3 or `0x00065380` returns non-zero, and returns the worker's result. The worker returns 0 at
`0x0001BB4B` and 2 at `0x0001BB4F`, and no other value. It sets `edi` to 1 at `0x0001B6AA` before
its first pass; on a pass with `edi` 1 it loops until `0x00063114` returns 3 or `0x00065380` returns
non-zero, then plays the resource at `0x000A96E0` through `0x0005B3B0(handle, 4, 100, 0x7FFF)` and
sets `edi` to 0.

## Interpretation

A won run is worth 2 points of lance experience. The run waits for a click or a key after its first
pass, as the practice joust does, and then plays the sound the wrapper loaded. After either movie
the game waits for a click or a key. There is no retreat movie: `DRRIDBYE.SMK` is named but never
played. The result 1 that the wrapper tests for never occurs.

## Alternatives

`0x0005B3B0` may do more than play the resource as a sound.

## How to reproduce

Disassemble `0x0001B3AC` to `0x0001B580`, and the worker `0x0001B584` from `0x0001B6A0` to its return
at `0x0001BB5A`.
