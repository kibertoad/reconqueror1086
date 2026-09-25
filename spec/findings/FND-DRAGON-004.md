---
id: FND-DRAGON-004
title: Winning the siege of person 100's castle is the crown ending
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00039DDD..0x00039DE6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A288..0x0003A2D4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A2E3..0x0003A439
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The siege routine stores `0x000437CC` of the selected record's cell, the person at the cell, in a
local at `0x00039DE6`. On its automatic path at `0x0003A288` it gives the result 0 when that person
is 100, and otherwise wins when `0x00024C38(a / 2 + 1)` is at most `b / 2` for two locals `a` and
`b`. After a result of 1, when the person is 100 (`0x0003A2E3`) it shows a `Victory` message box
with the text at `0x0009520C`. When `0x0009ADB8` is not 0 it plays `CROWNL30.SMK` (the string at
`0x000952AC`) through `0x00030100` with `0x000B084C` set to 1 for the call; otherwise it loads
resource `0x1DB`, shows picture `0x1C0` and draws three text lines. Then it calls
`0x0001BF54(4)`, stores 1 in `0x0009ADC0` and returns. For any other person it adds 1 to row 0's
field 24 at `0x0003A43A` and goes on.

## Interpretation

Person 100 is the king, whom the victory text names, and taking his castle by assault is the only
way to the crown: the automatic resolution cannot win it. Field 24 is BATTLE_WON.

## Alternatives

What `a` and `b` of the automatic path are was not traced here.

## How to reproduce

Disassemble `0x00039D95` to `0x0003A439`.
