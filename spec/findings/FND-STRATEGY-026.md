---
id: FND-STRATEGY-026
title: Map clicks at 0x000122AC pick a player record, confirm a target or add a route point
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000122AC..0x0001265A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00012044..0x00012283
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00012284..0x000122AB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000640A0..0x00064163
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00064164
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00084980..0x00084A48
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007234C..0x00072374
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000122AC` converts the pointer globals at `0x000B07E8` and `0x000B07EC` through `0x000640A0` into
`(x + 80 * r + 40, y + 20 * (c + 1))`, with `r` and `c` the camera row and column at `0x000AB104` and
`0x000AB108`. It then tests, in order, the active player records 0 to 5 against the rectangle
`(INT32(px) + 11, INT32(py) - 30, 24, 31)`, the active hostile records 0 to 4 and the active brigand
records 0 to 2 against `(INT32(px), INT32(py) - 20, 40, 40)`. `0x00064164` includes the left and top
edges and excludes the right and bottom ones. The first hit wins. A player hit selects that record
through `0x0009AE64` and stores 1 in `0x0009AE58`. A hostile hit at `0x000123FD`..`0x0001241B` or
a brigand hit at `0x000124F7`..`0x00012515` shows a message box with the prompt at object 2 `+0x364`
through `0x000256A0`; only a return of 1 stores `n | 0x1000` or `n | 0x10000` in the selected record's
`+0x14`, 0 in its `+0x18` and `+0x30` and `+0x0C`, and 0 in `0x0009AE58`. Otherwise the click is
used up.

A click that hits nothing adds a route point to the selected record. When `0x0009AE58` was 0 it
first stores 0 in `+0x14`, `+0x18`, `+0x30` and `+0x0C` and 1 in `0x0009AE58`. It accepts the point
only while `+0x18` is below 20, stores it at `+0x70 + 8 * +0x18` and adds 1 to `+0x18`; for the first
point `0x000120E4` sets the destination and direction and reads the terrain one direction unit ahead.
`0x00012044` removes the last point and, at none left, stores 1 in `+0x0C`, 0 in `+0x14` and 0 in
`0x0009AE58`. `0x00012284` stores 0 in `0x0009AE58`.

The mouse callback `0x00084980` stores the event's `CX` and `DX` in the pointer globals when bit 0
of the event is set, and `0x0007234C` stores the pair mouse services 4 and 3 return.

## Interpretation

The prompt asks whether to attack the unit. `0x0009AE58` is the route-drawing mode. The record has
room for 21 pairs before `+0x110`, and input stores at most 20. Undo leaves the removed pairs in
place.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000122AC..0x0001265A`, `0x00012044..0x00012283`, `0x00012284..0x000122AB`, `0x000640A0..0x00064163`, `0x00064164`, `0x00084980..0x00084A48`, `0x0007234C..0x00072374` in `CD:CONQUER.EXE`.
