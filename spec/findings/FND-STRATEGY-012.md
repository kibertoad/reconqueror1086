---
id: FND-STRATEGY-012
title: The retargeter 0x0003A9BC sends an arrived force at the first player force within 300 units, else home or at the player
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003A9BC..0x0003AC5A
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003A9BC(s)` stores 1 in `+0x34`. For each player record 0 to 4 whose `+0x00` is 1 it stores the
player's truncated position in `+0x3C` and `+0x40` and computes the floating square root of the
squared distance to the truncated `+0x5C` and `+0x60`. The first record whose distance is below the
float at `0x0009537D` (300.0) wins, and the differences divided by that distance become `+0x64` and
`+0x68`. When none wins it computes the distance `a` to the anchor of the dwords at `0x000AB0D4` and
`0x000AB0D0`, then the distance `b` to the origin's words `+0x05` and `+0x07`. When `a` is above `b`
it aims at the origin with the origin's coordinates in `+0x3C` and `+0x40`; otherwise it aims at
the anchor and stores it in `+0x3C` and `+0x40`.

## Interpretation

The loop writes each candidate's position into the destination before testing it, so when no
player force is near, the destination is overwritten by the later choice. The fallback point is
the player's home cell, and a tie goes to the home. There is no guard for a zero distance.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003A9BC..0x0003AC5A` in `CD:CONQUER.EXE`.
