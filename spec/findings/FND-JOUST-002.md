---
id: FND-JOUST-002
title: The practice lance moves with 8.8 velocities that decay to 80 per cent, pull towards the pointer, and drop by (150 - p) * 50 every seventh movie frame
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004116D..0x000411CF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041360..0x00041430
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041562..0x00041632
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004177F..0x00041846
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041D2E
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x0004116D`..`0x000411CF` the worker sets the bounds x 50 to 400 and y 20 to 280, and at
`0x00041360`..`0x00041430` it clears both velocities, sets `interval = 500 / frame_ms` from the
movie's frame duration and `impulse = (150 - p) * 50`, where `p` is its fifth argument, which
caller `0x00041D2E` sets to `0x78` (120). Each pass of its loop, at
`0x00041562`..`0x00041632`, adds each velocity to the matching 8.8 position, takes the integer
position by division by 256 toward zero, and when that is below the lower bound or above the
upper one sets the position to the bound and the 8.8 position to the bound shifted left by 8.
At `0x0004177F`..`0x00041846`, after the frame is drawn, it sets each velocity to
`velocity * 80 / 100`, adds `((pointer - position) / 10) * 0x3200 / p` with each division
truncated toward zero, where `pointer` is the dword at `0x000B07E8` for x and `0x000B07EC` for
y, and subtracts `impulse` from the y velocity when the movie's frame counter (offset `0x678`
of the movie object) divided by `interval` leaves no remainder.

## Interpretation

The lance sways under the player's pointer with inertia, and every half second of movie it
jerks upward. With the practice movie's 71 ms frames the interval is 7 frames and the jerk is
1,500 8.8 units.

## Alternatives

None known.

## How to reproduce

Open `0x00041164` at `0x000413D5` for the impulse and at `0x0004177F` for the velocity update;
`imul ..., 50h` and `idiv` by `64h` give the 80 per cent.
