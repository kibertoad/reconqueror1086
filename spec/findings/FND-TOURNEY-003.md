---
id: FND-TOURNEY-003
title: The tent allows three jousts and one melee between clears of its counts, and rewards wins through 0x0009DC40
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005CA8C..0x0005CBF4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005CC65..0x0005CC74
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005D137..0x0005D165
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005CC4B..0x0005CC64
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0005CA8C` branches on the dword at `0x000AFF44`. For 0, when `0x0009DC38` is 3 or more it shows
the message that everyone is too tired to joust and returns. Otherwise it keeps row 0's field 20,
calls the joust `0x000408CC` with `0x0009DC74`, and when that returns 1 it adds 1 to `0x0009DC38`
and compares the kept value with field 20 again. When field 20 grew, it adds 1 to `0x0009DC40`;
when that is then 3 it adds 1 to row 0's field 6; and when the kept value plus 1 plus row 0's
field 22 is above 0 and divisible by 6 it adds 1 to row 0's fields 0 and 1. For 1, at
`0x0005CC65`..`0x0005CC74`, when `0x0009DC3C` is 1 or more it shows the message that everyone is
too tired to melee, and otherwise it runs the melee. For 2 it calls `0x0005CA70`.

## Interpretation

The limits are three jousts and one melee between clears of the counts, a period the refusal messages call a day. Field 20 is JOUST_WON, 22 MELEE_WON, 6 FAME, 0
STRENGTH and 1 DEXTERITY: the third win since the counter was cleared gives a point of fame, and
every sixth joust or melee win of the character's career gives a point of strength and dexterity.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0005CA8C` to `0x0005CC64` and `0x0005D137` to `0x0005D165`.
