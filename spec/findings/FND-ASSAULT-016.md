---
id: FND-ASSAULT-016
title: The assault main loop runs input, then the effect scheduler, then the thinker, with no wait
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000583AA..0x000584DF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00055D0F..0x000561DB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004C7F4..0x0004C816
tool: Ghidra 12.1.3
environment: null
---

## Observation

The assault main loop at `0x000583AA`..`0x000584DF` handles input (the dispatch at
`0x00055D0F`..`0x000561DB`, which installs retainer orders), then calls effect scheduler
`0x000530D0`, then thinker `0x00050524`, draws the frame, and jumps back at `0x000584DF`
without calling a wait or timer routine. Effect constructor `0x0004C7F4`..`0x0004C816` scans
effect slots 0 to 63 and takes the first whose field `+0x0C` is 0.

## Interpretation

How many thinker calls happen per second depends on how fast the machine runs the loop. Which
actor the thinker visits first after an order therefore depends on processor speed and input
timing, and when two actors are ordered to the same cell the original has no fixed winner.

## Alternatives

None known.

## How to reproduce

Open the loop at `0x000583AA` and follow the calls in order to the jump at `0x000584DF`.
