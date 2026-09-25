---
id: FND-UI-015
title: The office, overview and estate map regions open the report pictures, the orders message and the map controls
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00030808..0x0003083C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00032CC4..0x00032EA4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003D018..0x0003D07A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003D6E8..0x0003D83B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003D8B8..0x0003DB30
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003DB4C..0x0003E080
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Castle office, screen 16: region 9 (`0x00030808`) shows pointer frame 0, plays the sound at
`0x0009AC24` and jumps to `0x0003DB4C`.

Overview, screen 17: regions 0, 1 and 2 (`0x00032CC4`, `0x00032D4C`, `0x00032DD4`) show pictures
`0x144`, `0x142` and `0x143` through `0x00024DB8(1, number, 0)`, run `0x000312B8`, `0x00031B84` or
`0x00031E28`, redraw the current screen and call `0x00032814`. Region 3 (`0x00032E5C`), in both slots
2 and 3, restores the music in `0x0009AAC4` and calls `0x00059760(1, 1)`.

Estate map, screen 23: region 11 (`0x0003D050`) calls `0x000596C0(16, 1, 1)`. Region 12
(`0x0003D018`) calls `0x000596C0(11, 0, 1)` only when the dword at `0x0009AEF0` is 1. Region 13
(`0x0003D710`) jumps to `0x00039D8C` when the dword at `0x0009AEF4` is not 0. Region 10 jumps to
`0x000122AC` (slot 2) and `0x0001265C` (slot 3). Region 14 adds 1 to `0x0009AEEC` up to 15 (slot 2)
or subtracts 1 down to 1 (slot 3) and passes the value to `0x00038690`. Region 0 (`0x0003D7DC`)
stores the pointer's x minus 422 in `0x000AB104` and `(y - 4) * 2`, limited to 2 to 300, in
`0x000AB108`. Region 15 (`0x0003D8B8`) shows picture `0x1B5`, changes palette entries from colour
index `0xE5` to `0xED` chosen by the dword at `0x000AB0C4`, waits for a click through `0x00024D14`
and restores the palette and screen. Region 17 (`0x0003DA7C`) shows picture `0x1B8` and waits for a
click. `0x0003DB4C` shows picture `0x1B9` and writes, with `FFONTA2.FNT`: the tournament invitation
built from the strings at `0x5988` and `0x5968` when `0x0005C664` returns a person, the king's order
to attack when the dword at `0x000AA4AC` is 1, and the overlord's order to eliminate brigands when
the dword at `0x000AA104` is 1, or the text at `0x5A70` when none applies; it then waits for a click.
Every estate map routine first plays the sound at `0x0009AEFC`. Regions 1 to 9 have no routine.

## Interpretation

The office's region 9 and the map's region 16 open the same orders message. The map's footer leads
to the office and, when the player is at a place, to the village exterior; region 13 starts a siege
(FND-DRAGON-004). The three tabs show the England map, the orders and the help picture, and region
14 is the speed setting of FND-STRATEGY-013.

## Alternatives

The overview pages' routines `0x000312B8`, `0x00031B84` and `0x00031E28` were not traced.

## How to reproduce

Disassemble the listed routines and read the strings at the object-2 offsets pushed.
