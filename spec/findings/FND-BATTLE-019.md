---
id: FND-BATTLE-019
title: The renderer sorts dead units first then by y and x, and picks frames by lane, category, heading and phase
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000289CC..0x00028A47
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00028A48..0x00028B48
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00028B5C..0x00028B9D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00025FCD..0x00026000
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000268C8..0x000268E4
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00028A48` sorts pointers to the units with the comparator `0x000289CC`: `+0x20 <= 0` before
positive, then ascending `+0x08`, then ascending `+0x04`, ties equal. `0x00028AD8` draws `MEN8.CSF`
frame `+0x2C + +0x00 + 5 * +0x14 + (+0x18 % 5) + +0x1C` at `(x - scroll x - 45, y - scroll y - 45)`.
It then draws frame 720 at `(x - scroll x - 5, y - scroll y - 7)` for each selected unit in list
order. Setup draws frame 722 at the first control plus `(20, 6)`, and the first control when used
draws frame 721 there plus `(15, 6)`.

## Interpretation

The library sort decides the order of equal units.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000289CC..0x00028A47`, `0x00028A48..0x00028B48`, `0x00028B5C..0x00028B9D`, `0x00025FCD..0x00026000`, `0x000268C8..0x000268E4`.
