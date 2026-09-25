---
id: FND-UI-008
title: The pointer is a frame of FFMOUSE.CSF chosen through 0x00064030
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A4DC..0x0002A4E6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00072180..0x00072205
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00064030..0x0006409D
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Startup calls `0x00072180` with the string `FFMOUSE` at `0x00093A70`. `0x00072180` returns early
with the text at `0x000997BC` when `0x00084954` returns 0, passes the 205 bytes of the mouse callback
from `0x0008497C` to `0x0007113C`, and loads the sprite set through `0x00018850`. `0x00064030(n)` does nothing and returns
0 when `n` is not below the frame count `0x0006A830` returns; otherwise it stores `n` in
`0x0009DF98`, takes frame `n` of the set the pointer at `0x0009E038` holds and its hot spot from the
16-byte entry `n` of the table the pointer at `0x000B07E4` points to, and installs them through
`0x0007CE40`. The `ffmouse.CSF` entry of the archive holds six frames of 20 by 20. Rendered with the game's palette, they show in order a sword, an
hourglass, travel arrows, a speaking mouth, a target and a pointing hand.

## Interpretation

The pointer has six shapes; a screen selects one by number. The village hover shows frame 5, the
store shows frame 1 while it loads, and frame 0 is the ordinary pointer.

## Alternatives

Which frames the other screens select, and what the hot spot table holds, were not traced.

## How to reproduce

Disassemble the listed ranges; decode `ffmouse.CSF`.
