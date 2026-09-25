---
id: FND-ASSAULT-023
title: The mode-12 handler aims at the stored destination, rounds to a cardinal and starts a direct movement effect
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FF53..0x0005001F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FFBC..0x0004FFDA
tool: Ghidra 12.1.3
environment: null
---

## Observation

Handler `0x0004FF53` (mode 12) subtracts the actor's current cell from the destination in
fields `+0x28` and `+0x2C`, turns the difference into a heading with `0x000445C4`, rounds it
with `(heading + 0x20) & 0xC0`, and clears field `+0x24`. At `0x0004FFBC` it loads the dword at
block `+0x44` and shifts it right by 16 to get the movement selector, and at
`0x0004FFD1`..`0x0004FFDA` it rewrites the descriptor's low flag byte as
`(flags & 0xA7) | 0x10` before calling `0x0004C7F4`.

## Interpretation

A retainer sent to a point walks one movement effect at a time along the cardinal direction
nearest the destination, and aims again each time an effect ends. A diagonal exactly between two
cardinals rounds to the clockwise one.

## Alternatives

The mode-12 effect was once thought to keep flag `0x40` and turn left at walls. The rewrite at
`0x0004FFD1` sets `0x10` instead.

## How to reproduce

Open `0x0004FF53` from the handler table; the shift by 16 follows the load of block `+0x44`.
