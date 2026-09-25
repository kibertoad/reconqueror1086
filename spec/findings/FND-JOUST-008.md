---
id: FND-JOUST-008
title: The dragon run scores the lance against two 26-entry target tables on movie frames 108 to 133 and stops at frame 134
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B584..0x0001BB54
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B6B9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001BA25..0x0001BA69
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009A794..0x0009A7FB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009A7FC..0x0009A863
  - build: BLD-GOG-EN
    file: CD:CONQUER/DRJSTRUN.SMK
    offset: 0x00..0xA6A44
tool: Ghidra 12.1.3
environment: null
---

## Observation

Worker `0x0001B584` reads the frame counter at offset `0x678` of its movie, `DRJSTRUN.SMK`, a
640 by 300 movie of 138 frames at 71 ms, and at `0x0001B6B9` continues only while it is below
`0x86` (134). At `0x0001BA25`..`0x0001BA69`, on each pass where the counter is 108 to 133, it
adds `abs(tx[counter] - x)` and `abs(ty[counter] - y)` to two totals, where `tx` is the dword
table based at `0x0009A5E4` and `ty` the one based at `0x0009A64C`, indexed by the frame
counter. For frames 108 to 133 `tx` holds 277, 277, 277, 277, 277, 277, 277, 276, 276, 276, 276,
277, 276, 274, 275, 274, 274, 272, 272, 270, 269, 267, 265, 262, 258, 254 and `ty` holds 125,
124, 124, 123, 122, 122, 122, 120, 119, 117, 117, 115, 115, 114, 111, 108, 108, 104, 101, 98, 95,
91, 85, 81, 75, 68. The worker draws no marker at the targets.

## Interpretation

The targets follow the dragon's eye in the movie over its last 26 scored frames.

## Alternatives

None known.

## How to reproduce

Read 26 dwords at `0x0009A794` and at `0x0009A7FC`, and open `0x0001BA25`.
