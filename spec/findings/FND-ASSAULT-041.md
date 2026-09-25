---
id: FND-ASSAULT-041
title: A strike shows one of two blood effects, chosen by whether the target dies, and the effect advances once per drawn frame
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00055171..0x00055189
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000552FB..0x00055395
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005841F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000584DF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005D758
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x00055171`..`0x00055189` the player's strike path selects base frame 43 of
`SKIRMISH.RES/skirmish.csf` when the target's health after the hit is below 1, and base frame 48
otherwise. Renderer `0x000552FB`..`0x00055395` draws the base frame plus offsets 0 to 3, one per
call, then turns the effect off. The assault main loop calls that renderer once per pass at
`0x0005841F`, copies the frame to the screen through `0x0005D758`, which has no time check, and
jumps back at `0x000584DF`. The loop does not call the game's wait routine at `0x000657C0`.

## Interpretation

A killing blow shows the large blood effect and any other hit the small one. The effect lasts
four drawn frames, so its duration depends on how fast the machine draws.

## Alternatives

None known.

## How to reproduce

Open `0x00055171` and follow the frame number into the renderer at `0x000552FB`.
