---
id: FND-DRAGON-001
title: The dragon encounter wrapper limits lance experience to 0..20, stores it back after the run, and reports failure for results 1 and 2
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001BB5C..0x0001BC0C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B250..0x0001B293
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0001BB5C` calls `0x0001B250(1)`, which formats `LANCE%1d.CSF` with 1, loads it through `0x00018430`
and keeps the handle at `0x000A96E4`. It reads row 0's field 16 through `0x00015EF0` into a local,
lowers it to 20 when above and raises it to 0 when below. It loads resource `0x1D8` through
`0x0005B584` and keeps the handle at `0x000A96E0`, calls `0x0001B3AC` with the address of the local,
releases the resource, stores the local as row 0's field 16 through `0x00015F0C`, and releases the
lance sprites through `0x0001B284`. When `0x0001B3AC` returned 2 or 1 it stores 1 in `0x0009ADC0` and
returns 0; otherwise it stores 1 in `0x0009ADC0` and returns 1.

## Interpretation

The run works on a copy of the player's lance experience held to 0 to 20, and whatever the run
leaves in the copy becomes the new lance experience. Either way the map session ends.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0001BB5C` to `0x0001BC0C` and `0x0001B250` to `0x0001B293`.
