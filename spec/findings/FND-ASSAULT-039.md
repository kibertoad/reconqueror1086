---
id: FND-ASSAULT-039
title: The loader recolours actors: friendlies take the player's colour family, and a hostile that shares it takes another
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00051A8B..0x00051B74
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x00051A8B`..`0x00051B74` the loader reads the player's colour and maps red, green and blue
to the pair (palette family base at block word `+0x08`, walk texture base at block word
`+0x2C`) `(0, 64)`, `(32, 128)` and `(64, 96)`. It writes the player's pair to every friendly
actor. A hostile actor whose authored family base equals the player's is changed: to blue
`(64, 96)` when the player is red or green, and to green `(32, 128)` when the player is blue.
Other hostiles keep their authored values.

## Interpretation

Retainers always wear the player's colours, and no enemy wears them.

## Alternatives

None known.

## How to reproduce

Open `0x00051A8B`; the three pairs are loaded as constants after the read of the player's
colour.
