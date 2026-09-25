---
id: FND-STRATEGY-022
title: The staging routine 0x00035924 trims both sides to 60 and hands six counters to the battle
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00035924..0x00035C59
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00035924(i, j)` computes `m = (a6 + a5 + b) / 6`, with `a5` and `a6` character attributes 5 and 6
of row 0 and `b` 20 when `i` equals the dword at `0x0009AE6C` and 0 otherwise. It reads the hostile
counts `+0x1C`, `+0x20`, `+0x24` of record `j`. When their total is above 60 it takes
`r = (total - 60) / 3` and subtracts `r` from each count for which `r + 1` is below it. It reads the
player pools `p0 = 0x00029E58(i, 0)`, `p1` and `p2`. When their total is above 60 it counts the pools
of 20 or more as `n`, and when `n` is not 0 takes `r = (total - 60) / n` and, for each pool for which
`r + 1` is below it, subtracts `r` and keeps `r` aside for that pool.

It calls `e = 0x000258FC` with the six counts by reference, `m` and 0. It adds 1 to attribute 24 for
`e == 1` and to attribute 25 otherwise, and stores the three hostile counts back in `+0x1C`, `+0x20`
and `+0x24`. When `e` is 0 and a player pool is not 0 it draws `d = 0x00024C38(40) + 30` and, for
`p0`, `p2` and `p1` in that order, stores 0 when the pool is 1 and otherwise subtracts
`pool * d / 100`. It then sets each pool to `max(0, pool)` plus the amount kept aside for it, calls
`0x00029DE0(i, p0, p1, p2)` and returns `e`.

## Interpretation

Attribute 5 is HONOR and 6 FAME. A force larger than 60 fights with its excess set aside, and the
player's set-aside troops return after the battle whatever its result. A lost battle costs the player
a further 30 to 70 percent of each pool. Together with the routine that calls it (FND-STRATEGY-021),
each result is counted twice in BATTLE_WON or BATTLE_LOST.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00035924..0x00035C59` in `CD:CONQUER.EXE`.
