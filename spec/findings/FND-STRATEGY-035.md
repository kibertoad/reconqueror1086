---
id: FND-STRATEGY-035
title: A spy costs 80, only one is out at a time, and its report lists every active hostile force
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00036720..0x000367D4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00036E87
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00036AFC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038C68..0x00038C83
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038C84..0x00038D77
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00037340
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00036720` plays a sound and, when the dword at `0x000AA090` is below 80, shows a refusal. Otherwise,
when the confirmation `0x00025004` returns not 0, it subtracts 80 through `0x00037340`, adds 1 to the
dword at `0x000AA0EC`, stores the wealth in attribute 17 of row 0 and calls `0x00036AFC`.
`0x00036E87` stores 0 in `0x000AA0EC` when the war-planning screen opens, and `0x00036AFC` calls
`0x00038C68` once for each pending spy. `0x00038C68` returns 0 when the dword at `0x0009AE68` is 1,
and otherwise stores 1 there and returns 1.

`0x00038C84` does nothing when `0x0009AE68` is not 1. Otherwise it visits the five hostile records
and, for each whose `+0x00` is not 0 and whose origin's state byte is not 0, shows the counts
`+0x1C`, `+0x20`, `+0x24` and the property's name through `0x00015EAC`. Once any record is active it
stores 0 in `0x0009AE68` after the scan.

## Interpretation

`0x000AA090` is the wealth the war-planning screen works with. A spy stays out until a hostile force
exists, and then reports all of them at once, in slot order, before returning.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00036720..0x000367D4`, `0x00036E87`, `0x00036AFC`, `0x00038C68..0x00038C83`, `0x00038C84..0x00038D77`, `0x00037340` in `CD:CONQUER.EXE`.
