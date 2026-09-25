---
id: FND-TOURNEY-005
title: The melee sets its wager from the average sword experience and settles it in wealth and the tallies
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005CC7A..0x0005CE46
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005CE97..0x0005D136
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002C20C..0x0002C235
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The melee reads row 0's field 15 and the opponent's (`0x0009DC74`), and halves their sum with a
signed division. A half below 10 gives a tier of 0, below 13 gives 1, below 16 gives 2, below 18
gives 3, and otherwise 4. It takes `0x000AC180` modulo 3 with a signed division, draws
`0x00024C38(20)`, and multiplies the tier plus 1 by the draw plus 1. When row 0's field 17 is 0 it
shows a refusal and returns; otherwise it lowers the product to field 17 when larger and shows an
offer with the opponent's name and the wager. When the player declines it returns. Otherwise it
keeps row 0's field 22, closes the tent, builds a scene name, draws `0x00024C38(2)` twice, and
calls `0x0005877C` with the name, the second draw plus 8, the first draw plus 8, and 0. A result
of 1 is a win: after a 240-pass pause it adds 1 to `0x0009DC40`, adds the wager to the value
`0x0002C20C` returns and stores it with `0x0002C218`, adds the wager to row 0's field 17 and 1 to
its field 15, adds 1 to row 0's field 6 when `0x0009DC40` is then 3, adds 1 to row 0's fields 0
and 1 when the kept field 22 plus 1 plus row 0's field 20 is above 0 and divisible by 6, and adds
1 to row 0's field 22 and to the opponent's field 23. Any other result is a loss: it subtracts
the wager from that value and from row 0's field 17, and adds 1 to the opponent's fields 15 and 22
and to row 0's field 23. Either way it then reopens the tent with `0x0005C99C` and adds 1 to
`0x0009DC3C`. `0x0002C20C` returns the dword at offset 4 of the record the pointer at `0x0009ABEC`
points to, and `0x0002C218` stores its argument there, or 0 when the argument is negative.

## Interpretation

Field 15 is EXPERIENCE_WITH_SWORD, 17 WEALTH, 20 JOUST_WON, 22 MELEE_WON and 23 MELEE_LOST. The
stake runs from 1 to 105 and grows with the two fighters' experience. The two draws plus 8 are
likely the number of fighters on each side.

## Alternatives

What the dword at `0x000AC180` holds, and what the value behind `0x0009ABEC` stands for, were not
traced.

## How to reproduce

Disassemble `0x0005CC65` to `0x0005D136` and `0x0002C20C`.
