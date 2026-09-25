---
id: FND-BATTLE-002
title: The automatic battle compares totals plus the remainder of each morale value by 3
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000259C3..0x00025A88
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000256FC..0x000258FA
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

When `0x000256FC` returns 0, the resolver takes the remainder of the signed division of `0x000A9CA0`
by 3 (EDX after `idiv`) and adds it to `0x000A9C70`, and the remainder of `0x000A9C78` by 3 added to
`0x000A9C94`. When the first sum is not greater, it stores 0 in the three player counts and returns 0,
leaving the foe's counts alone. Otherwise, with `t` the foe's total, it draws `0x0006B3F1`, takes the
signed remainder by `t`, subtracts it from the first player count (storing 0 for a negative result),
lowers `t` by it, and does the same for the second and third counts, and returns 1.

## Interpretation

The morale values change the automatic result by 0 to 2 only. The old notes said the value was
divided by 3; the instruction keeps the remainder. A foe total of 0 with a winning player divides by
zero.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000259C3..0x00025A88`, `0x000256FC..0x000258FA`.
