---
id: FND-STRATEGY-004
title: The hostile generator 0x0003BE58 tries a reactive pursuit, then a timed movement every 5,000 speed units
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003BE58..0x0003C086
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003BDC0..0x0003BE22
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003BE24..0x0003BE57
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003BE58` adds 1 to the dword at `0x0009AEA8` and calls `0x0003BB4C(&p, &s)`. When that returns
not 0, and neither `p == 7` with the counter below 1000 and `s` not the dword at `0x0009AE6C`, nor
`0x0003BE24(s)` returning 1 with `s` not that dword or the counter at most 50, it calls
`0x0003A764(s, p)`. When that returns 1 it stores 0 in the counter, adds the dword at `0x0009AEEC`
to the dword at `0x0009AE54`, and draws `0x00024C38(6)`. When the draw is below 2 and the dword at
`0x0009AE60` is below 5 it calls `0x0003AC5C(t, p, 2, 0)` and, when that returns 0,
`0x0003AC5C(t, p, 1, 0)`, with `t` the dword at `0x000AB0C4`, and then stores 0 in `0x0009AE54`.
After a successful pursuit it returns either way.

Otherwise, when `0x0009AE54` is below 5000 it adds `0x0009AEEC` to it and returns. At 5000 or more
it stores 0 there and returns when `0x0009AE60` is 5 or more. It draws `0x00024C38(100)`. When the
draw is above 96, `0x00043150()` returns 1 and the byte at `0x0009B8F9 + 15 * p` is not 0, it tries
each player record `k` from 0 to 4 whose dword at `0x000AA4B8 + 0x118 * k` is 1: it stores
`q = 0x0003BDC0()` in `p`, and when `q` is not negative and `0x0003A764(k, q)` returns not 0 it
returns. Then, when `0x00043150()` returns 0, it calls `0x0003AC5C(t, o, 2, 0)` and, when that
returns 0, `0x0003AC5C(t, o, 1, 0)` with `o` the dword at `0x000AB0CC`. When it returns 1 it
stores `q = 0x0003BDC0()` in `p` and, when `q` is not negative, calls `0x0003AC5C(t, q, 2, 1)` and,
when that returns 0, `0x0003AC5C(t, q, 1, 1)`.

`0x0003BDC0` collects the property indexes 0 to 13 whose byte `+0x00` at `0x0009B8EC + 15 * i` is
not 0 and whose byte `+0x0D` is 1, in index order. With none it returns -1, and otherwise the entry
at `0x00024C38(count - 1)`. `0x0003BE24(s)` returns 1 when a hostile record 0 to 4 has `+0x00` 1,
`+0x14` equal to `s` and `+0x34` equal to 3.

## Interpretation

`0x0009AEA8` counts passes since the last reactive pursuit. The draw above 96 only decides whether
player-targeted pursuits are tried before the ordinary timed movement; the ordinary movement is
tried every time the accumulator reaches 5,000. The accumulator starts at 5,000 in a new game
(FND-STRATEGY-023). `p` is only written by `0x0003BB4C` when it finds something and by the
pursuit loop, so the test of `p` in the timed branch can read a value left from an earlier pass
or the frame's previous contents.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003BE58..0x0003C086`, `0x0003BDC0..0x0003BE22`, `0x0003BE24..0x0003BE57` in `CD:CONQUER.EXE`.
