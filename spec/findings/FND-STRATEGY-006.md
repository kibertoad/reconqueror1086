---
id: FND-STRATEGY-006
title: The pursuit constructor 0x0003A764 detaches a force aimed at a player force
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A764..0x0003A9B9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A738..0x0003A760
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A6A0..0x0003A736
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003A764(t, p)` returns 0 when `t` is 5 or more, when `0x0003A738` finds no hostile record whose
`+0x00` is 0, or when `0x00043640(p)` is 0 or less. Otherwise, for the first free record `s` it
stores `p` in `+0x28`, `0x0004377C(p)` in `+0x2C`, `t` in `+0x14`, the word at `0x0009AE88 + 2 * p`
shifted left 3 in `+0x38`, 1 in `+0x00`, the truncated floats `+0x5C` and `+0x60` of player record
`t` in `+0x3C` and `+0x40`, the player record's `+0x44` and `+0x48` in `+0x44` and `+0x48`, and 3
in `+0x34`. It anchors the property's grid words `+0x01` and `+0x03` through `0x00063E20` and
stores them as floats in `+0x5C` and `+0x60`. It divides the differences between `+0x3C`, `+0x40`
and the truncated position by `0x0002FC70` of their squared length, stores the two quotients as
floats in `+0x64` and `+0x68`, calls `0x00038B40(s, t)`, adds 1 to the dword at `0x0009AE60` and
returns 1.

`0x0003A738(&s)` stores the first index 0 to 4 whose `+0x00` is 0. `0x0003A6A0(s)` does nothing
when `+0x00` is not 1; otherwise, when `+0x0C` is 0 and `+0x34` is 2 it frees the pointer `+0x6C`,
stores 0 there and 1 in `+0x0C`, then subtracts 1 from `0x0009AE60` and stores 0 in `+0x00`, and
stops the game with an error message when the count went below 0.

## Interpretation

A pursuit starts at the property, heads for the player force's current position and remembers the
player record it hunts. `0x0009AE60` counts live hostile records. Nothing guards a zero length:
when the property's anchor equals the player's truncated position the division is by 0.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003A764..0x0003A9B9`, `0x0003A738..0x0003A760`, `0x0003A6A0..0x0003A736` in `CD:CONQUER.EXE`.
