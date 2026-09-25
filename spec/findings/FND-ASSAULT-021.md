---
id: FND-ASSAULT-021
title: The tests for modes 12, 13 and 17 check the destination, the health threshold 6 and a clear line to the player
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FD7B..0x0004FD95
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FD9A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FB39..0x0004FC33
tool: Ghidra 12.1.3
environment: null
---

## Observation

Test `0x0004FD7B` (mode 12) returns true when the actor's current cell equals the integer
coordinates stored in fields `+0x28` and `+0x2C`. Test `0x0004FD9A` (mode 13) returns true when
the actor's health (field `+0x40`) is at least 6. Test `0x0004FB39` (mode 17) casts a centred
ray from the actor towards the player combatant (index at `0x0009D4D0`), and returns true only
when the returned block belongs to the player and the depth is strictly below `0x7FFF`; it
then stores the player in field `+0x24` and the player's cell in `+0x28` and `+0x2C`.

## Interpretation

Mode 12 ends when the actor reaches the exact destination cell. Mode 13 sends an actor with at
least 6 health back to fighting. Follow resumes its direct approach whenever the player is in
sight.

## Alternatives

None known.

## How to reproduce

Open each test from the table at `0x0004F414` (FND-ASSAULT-014).
