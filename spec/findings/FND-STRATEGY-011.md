---
id: FND-STRATEGY-011
title: Mode 2 at 0x0004A300 follows a route one waypoint at a time and stops on bad terrain
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004A300..0x0004A61B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002FC70..0x0002FCAB
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0004A300(r, &done)` stores 0 in `done` and returns 0 when `+0x0C` is not 0. It stores the pair
at `+0x6C` index `+0x30` in `+0x3C` and `+0x40` and takes `(ex, ey)` as those minus the truncated
position. When both `abs(ex)` and `abs(ey)` are below 6, or a difference is 0 or more while its
direction float `+0x64` or `+0x68` is below 0, or 0 or less while the float is above 0, it adds 1
to `+0x30`. When `+0x30`
reaches `+0x18` it frees `+0x6C`, stores 0 there and 1 in `+0x0C`, stores `0xFFFF` in `done` and
returns 0; otherwise it reads the next pair the same way.

It takes `n = 0x0002FC70(ex * ex + ey * ey)`. For `n` 0 or less, for a truncated `+0x64` or `+0x68`
above 50 or below -50, or for tile kind 9 at the projection of `(INT32(x) + INT32(+0x64),
INT32(y) + INT32(+0x68))` (stored in `+0x44` and `+0x48`), it frees the route, stores 1 in `+0x0C`
and 0 in `+0x00`, stores `0xFFFF` in `done` and returns 0. Otherwise it rounds `ex / n` and `ey / n` to
single precision and stores each times `w` times `k` in `+0x64` and `+0x68`, and adds both to `+0x5C` and `+0x60` only when the
absolute value of the first product, before it is stored, is below the double at `0x00097389`
(50.0). It returns 1. None of the three stops changes the count at `0x0009AE60`.

`0x0002FC70(v)` returns `v` for `v` of 1 or less. Otherwise it starts from `g = v / 2` and repeats
`g' = (g + v / g) / 2` while `g'` is below `g`, and returns the last `g`.

## Interpretation

The route's waypoint changes when the force is within 6 units on both axes or has passed it. The
prior step's size is checked before the new one is taken, so a step of 50 or more whose truncation
is still 50 leaves the force in place, and a larger one stops it on the next call. `0x0002FC70` is
the integer square root rounded down.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0004A300..0x0004A61B`, `0x0002FC70..0x0002FCAB` in `CD:CONQUER.EXE`.
