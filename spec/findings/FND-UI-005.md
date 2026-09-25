---
id: FND-UI-005
title: The village exterior actions: tournament gate, map, inn, forge, lender and church
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00060BE0..0x00060E18
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043048..0x0004307D
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Each of the six click routines first plays `0x0009DF00` through `0x0005B3B0(handle, 4, 0, 0x7FFF)`.
Region 0 (`0x00060BE0`): when `0x0009DED4` is 0 it shows a message box through `0x00025004` with the
text at `0x00098E98` and title at `0x00098E64`; otherwise, when `0x00038678()` differs from `+0x08`
of the tournament block it shows the text at `0x00098E78`, and when they are equal it calls
`0x0005B2C0`, stores -1 in `0x0009AAC4` and 0 in `0x0009DED8` and switches to screen 8. Region 1
(`0x00060C64`) calls `0x0005B2C0`, stores -1 in `0x0009AAC4` and 0 in `0x0009DED8`; when
`0x0009DED4` is 1 it stores 0 in `0x0009DC38`, `0x0009DC3C`, `0x0009DC40`, `0x0009DED4` and
`0x0009DBCC` and -1 at `+0x04` of the tournament block; then it stores 1 in `0x0009DEF8` and switches
to screen 23. Region 2 calls `0x0005B2C0`, stores -1 in `0x0009AAC4` and 0 in `0x0009DED8`, and
switches to screen 12; region 3 switches to screen 13. Region 4 stores the byte at
`0x0009BA61 + 18 * p` (`0x00043064`) in `0x000A9B20`, `p` the dword at `0x000AC180`, stores 1 in
`0x0009DEDC` and switches to screen 24. Region 5 calls `0x0005B2C0`, stores -1 in `0x0009AAC4` and 0 in `0x0009DED8`,
stores the byte at `0x0009BA60 + 18 * p` (`0x00043048`) in `0x000A9B20`, loads resource `0x177` through `0x0005B0D4`, stores `0x177` in
`0x0009AAC4` and 1 in `0x0009DF04`, and switches to screen 24. Across the 175 used person records the
byte at `+0x10` is 12 to 18 and the byte at `+0x11` is 8 to 11.

## Interpretation

The tournament opens only while the calendar field saved on arrival has not changed, and leaving by
the map with a tournament in town clears the joust, melee and win counts and closes it. The lender
and the church each start a conversation with the partner the person record names.

## Alternatives

The calendar field is taken to be the day; what `0x0009AE00 + 0x1C` counts was not traced.

## How to reproduce

Disassemble `0x00060BE0` to `0x00060E18` and `0x00043048` to `0x0004307D`; read the person bytes from
`0x0009BA50` onwards.
