---
id: FND-ASSAULT-025
title: The mode-9 and mode-10 handler walks away from the stored target at one and a half times the descriptor step, without rounding to a cardinal
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050020..0x0005010A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005009E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000446BC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004472C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00044740
tool: Ghidra 12.1.3
environment: null
---

## Observation

Handler `0x00050020` (modes 9 and 10) subtracts the stored target's live position from the
actor's (current minus target), turns it into a heading with `0x000445C4`, and does not round
it. It multiplies the descriptor's per-tick x and y deltas (descriptor `+0x1C` and `+0x20`)
separately by the double at `0x00097CEA` (object 2 offset `0x7CEA`, reached through the fixup at
source `0x0005009E`), which holds 1.5, converts each product to an integer with the x87 `FISTP`
instruction, rewrites the flags to `0x112`, and starts the effect. The effect's steps are
rotated through `0x000446BC`, `0x0004472C` and `0x00044740`.

## Interpretation

An actor escaping walks directly away from its target in any direction, half as fast again per
tick as it normally walks. With the shipped step of 64, each tick moves 96 units.

## Alternatives

Because no placed actor starts in mode 9 or 10, this handler was once thought unreachable. The
mode 14 to 13 to 10 route (FND-ASSAULT-024, FND-ASSAULT-012) reaches it.

## How to reproduce

Open `0x00050020`; the relocated `fmul` operand at `0x0005009E` names `0x00097CEA`.
