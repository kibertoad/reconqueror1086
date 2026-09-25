---
id: FND-ASSAULT-006
title: The retainer counter skips the player, and the loader removes retainers above the cap
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004D870
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00051B87
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x0004D870` counts combatant records after the ten templates whose side is 0 and whose
health (`+0x40`) is above 0, and skips the record whose index equals the global at
`0x0009D4D0`. At `0x00051B87` the loader repeats a removal step while that count exceeds the
global at `0x0009D49C`. Each pass removes one retainer. The choice of which retainer is removed
calls the game's random number routine. No path in the loader creates combatant records.

## Interpretation

The player is never counted as a retainer. A scene can supply fewer retainers than the cap
allows; the loader then keeps all of them. It never adds retainers the scene does not place.

## Alternatives

None known.

## How to reproduce

Open `0x0004D870`; the comparison with `0x0009D4D0` is inside its loop. Follow the loop at
`0x00051B87` in the loader.
