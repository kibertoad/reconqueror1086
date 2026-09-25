---
id: FND-BATTLE-008
title: Each loop pass draws rectangles, dispatches input, and runs the unit pass every 200 clock units
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026B88..0x00026C81
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00027E3B..0x00027ECF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002808E..0x000283FC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026C1A..0x00026C28
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Each pass of `0x00026B88` writes an eight-byte rectangle per unit at `0x000A9CB0`: four zero words for
`+0x20 <= 0`, and `(x - 15, y - 20, 25, 30)` from `+0x04` and `+0x08` otherwise. It calls `0x000264F8`
and returns 1 when that returns nonzero. When `0x000A9C7C` is nonzero it skips the unit pass. It reads
`0x00018420`, and skips while the unsigned value is at most the dword at `0x0009AA8C` plus `0xC8`;
otherwise it stores a second reading at `0x0009AA8C`, sets `0x000A9CC0` to 1, runs the unit pass over
the `0x000A9C90` units, and shows the player lane total in a panel at
`(0x000A9C74 + 40, view height - 25)`. After the input dispatch,
`0x0002808E` returns 2 when `0x000A9C94` is 0, then 1 when `0x000A9C70` is 0, and otherwise 0.

## Interpretation

The unit pass runs once per 200 clock units at most and never catches up.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00026B88..0x00026C81`, `0x00027E3B..0x00027ECF`, `0x0002808E..0x000283FC`, `0x00026C1A..0x00026C28`.
