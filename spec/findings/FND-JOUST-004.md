---
id: FND-JOUST-004
title: The practice joust compares the lance with three targets on movie frames 80 to 82
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004184D..0x00041922
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B84C..0x0009B867
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x0004184D`..`0x00041922`, on every pass where the movie frame counter is at least the dword
at `0x0009B864` (80) and below it plus 3, the worker takes `k = counter - 80`, adds
`abs(tx[k] - x)` and `abs(ty[k] - y)` to two error totals, and adds `tx[k] - x` and
`ty[k] - y` to two signed totals, where `tx` is the three dwords at `0x0009B84C` (191, 161 and
122) and `ty` the three at `0x0009B858` (191, 203 and 211).

## Interpretation

The opponent's shield passes three points of the movie on frames 80 to 82, and the lance is
measured against them on each pass during those frames.

## Alternatives

None known.

## How to reproduce

Read the tables at `0x0009B84C`, `0x0009B858` and `0x0009B864`, and open `0x0004184D`.
