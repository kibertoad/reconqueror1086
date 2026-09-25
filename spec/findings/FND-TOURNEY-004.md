---
id: FND-TOURNEY-004
title: The joust wager is 1 + a scaled draw of 30 + the opponent's lance experience, capped at the player's wealth
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000408CC..0x00040AED
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000408CC(opponent)` reads the opponent's field 16, draws `0x000445B4(30)`, and adds the two and
1. When row 0's field 17 is 0 it shows a refusal message and returns 0. Otherwise it lowers the sum
to field 17 when it is larger, and shows an offer that names the joust of the day by an ordinal
word taken from a three-entry table at index `0x0009DC38`, the opponent's name and the sum. When
the player declines, `0x00025004` returns 0 and the joust returns 0. Otherwise it waits 60 passes
of `0x000730F8`, calls `0x0003FB28(1)`, reads row 0's field 16, and calls `0x0003FCA8` with the
opponent, the addresses of the two lance values and of a local set to 99, 120 and the wager. It
then stores the opponent's lance value, lowered to 20 when above, as his field 16; when
`0x0009DC38` is 2 it calls `0x000628AC`; it stores the player's lance value, lowered to 20, as row
0's field 16, and returns 1.

## Interpretation

Field 16 is EXPERIENCE_WITH_LANCE and 17 WEALTH, so the stake is 1 to 30 more than the opponent's
lance experience. The joust itself, and whatever it does with the wager and the tallies, is in
`0x0003FCA8`. Since `0x00015F0C` does not limit field 16, the joust limits it to 20 itself.

## Alternatives

The ordinal table's contents were not read; the three words first, second and third sit at
`0x00095E7C`, `0x00095E83` and `0x00095E8C`.

## How to reproduce

Disassemble `0x000408CC` to `0x00040AED`.
