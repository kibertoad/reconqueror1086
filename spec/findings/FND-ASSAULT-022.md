---
id: FND-ASSAULT-022
title: Handlers for modes 1 to 8, 16 and 17 set the state block, start a wandering effect with flag 0x40 or a direct effect with flag 0x10
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FDAB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FDCD..0x0004FE68
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FE17..0x0004FE30
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FE76..0x0004FF52
tool: Ghidra 12.1.3
environment: null
---

## Observation

Handler `0x0004FDAB` (modes 1 to 4) changes the actor's block state and returns without
starting an effect.

Handler `0x0004FDCD` (modes 5, 6 and 17) clears the target fields, keeps the actor's heading,
reads the movement selector from the signed word at block `+0x46` (the high word of the dword
at `+0x44`), and at `0x0004FE17`..`0x0004FE30` rewrites the low byte of the descriptor's flags
as `(flags & 0xA7) | 0x40` before calling effect constructor `0x0004C7F4`.

Handler `0x0004FE76` (modes 7, 8 and 16) subtracts the actor's live position from the target
combatant's (field `+0x24`), turns the difference into a heading with `0x000445C4`, rounds it to
a cardinal with `(heading + 0x20) & 0xC0`, rewrites the flags as `(flags & 0xA7) | 0x10`, and
starts the movement effect.

## Interpretation

Wandering modes keep walking in the direction they face and turn left at walls (flag `0x40`,
FND-ASSAULT-029). Approach modes walk towards their target along the nearest cardinal direction
and stop at an obstacle (flag `0x10`). The shipped descriptors have flags `0x142`, which the
wandering rewrite leaves unchanged and the direct rewrite turns into `0x112`.

## Alternatives

None known.

## How to reproduce

Open each handler from the table at `0x0004F458`. The `0xA7` mask and the `0x40` or `0x10` bit
are applied just before the call to `0x0004C7F4`.
