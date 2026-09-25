---
id: FND-JOUST-011
title: The dragon lance frame scan has no end check and reads past its five thresholds when the lance is above y 92
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B591..0x0001B8B4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B86C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009A780..0x0009A793
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x0001B591`..`0x0001B8B4` the worker selects the lance frame as the practice worker does
(FND-JOUST-003), with the thresholds 240, 188, 136, 114 and 92 at `0x0009A780`. The scan at
`0x0001B86C` has no limit. When the lance y is below 92 it goes on through the target tables
that follow (FND-JOUST-008), whose x entries are all above 250 and whose y entries fall to 68,
and past them. For every y from 20 to 91 it stops at a count of 53 or more, so
`5 * count - 1 - column` is above 24 and the frame is 24.

## Interpretation

The table was meant to end the scan, and does for every y from 92 up. Above that the read
overruns into other data, but the result is always the last frame, the lance at its highest.

## Alternatives

None known.

## How to reproduce

Read 80 dwords from `0x0009A780` and run the scan for y 20 to 91.
