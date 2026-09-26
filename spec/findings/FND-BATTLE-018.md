---
id: FND-BATTLE-018
title: The resolver reads WAR_MODE and loads BATTLE.PCX and MEN8.CSF
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000258FC..0x000260D7
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The resolver names `BATTLE.PCX`, `MEN8.CSF` and `WAR_MODE` at object-2 offsets `0x3250`, `0x32CC`
and `0x32C0`. It reads `WAR_MODE` as a number and tries a larger display mode unless it is 640, a
missing key included, storing 1024 or 800 in `0x000A9CC8` on success (FND-CONFIG-004). `BATTLE.PCX` decodes to a 1024 by 728
image and `MEN8.CSF` to 723 frames: 720 of 90 by 90, then 9 by 9, 39 by 13 and 57 by 14. The interactive
path at `0x00025A89` allocates `0x000A9C70 + 1` bytes at `0x000A9C84` and stores 0 in the first. After
the constructor, `0x00025F08` sets `0x000A9C74` to 160 when `0x000A9CC8` is 1024 and to 80 when it is
800, and writes the three control rectangles after the unit rectangles at `0x000A9CB0`. Each pass of
the loop at `0x0002602E` calls `0x00026B88`, and when `0x000A9CC0` is 1 sorts and draws the field and
stores 0 there. Direct calls to `0x000258FC` are at `0x0002A80B`, `0x00035ADC`, `0x0003B468` and `0x00042363`.

## Interpretation

The battlefield is 1024 by 728 pixels and scrolls in a smaller view.

## Alternatives

What the calls at `0x0002A80B` and `0x00042363` are for was not traced.

## How to reproduce

Disassemble `0x000258FC..0x000260D7`, `C1086.GOB`.
