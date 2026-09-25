---
id: FND-ASSAULT-040
title: The foreground weapon swing picks a target and start point by combat-row family and moves at a speed set by the row's divisor
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00054C07..0x00054F12
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00054F98..0x00054FE5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00054FEC..0x0005501E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000550F2..0x00055140
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005529B..0x00055448
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005590C..0x0005591D
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x0005590C`..`0x0005591D` the attack path passes the centred pointer position `(x, y)` to
foreground setup `0x00054C07`..`0x00054F12`. With `(w, h)` the selected frame's size and
`(W, H)` the combat view's size, setup computes a target and a start point by combat row:

| Rows | Target | Start |
|---|---|---|
| 0 to 3 | `(x - random(8), max(y - random(16), H - h))` | `(target.x, target.y + H / 2)` |
| 4 to 14 | `(x + 16 - random(40), max(y - random(40), H - h))` | below the `H / 3` line: `(target.x +/- W / 2, target.y - H / 2)`; above it: `(target.x, target.y + H / 2)` |
| 15 to 22 | `(x + 16 - random(32), max(y - random(24), H - h))` | `(target.x +/- W / 2, target.y - 56)` |
| 23 and 24 | `(x - w / 2, H - h)` | no motion |

Where the start is offset sideways, the side follows the target's x, and the frame is mirrored
for the left side. At `0x00054F98`..`0x00054FE5` each axis's velocity is
`((target - start) << 9) / divisor`, with the divisor from combat-row column 3 (record offset
`0x0C`). At `0x00054FEC`..`0x0005501E` the signed 8.8 position advances by velocity times the
elapsed milliseconds. On contact, `0x000550F2`..`0x00055140` reverses the horizontal velocity and
reverses and halves the vertical velocity for melee rows. Renderer `0x0005529B`..`0x00055448`
draws frame offset 2 while approaching, offset 1 while the horizontal distance to the target is
below `W / 4`, and offset 0 while returning; rows 23 and 24 stay on offset 2 and rise from the
bottom of the view by the frame's height.

## Interpretation

The player's weapon swings in from below or from the side towards the clicked point and back,
faster for weapons with a smaller divisor. Crossbows are raised and held instead of swung.

## Alternatives

A fixed 70 ms frame timer for melee swings was once assumed. The motion is time-integrated from
the divisor.

## How to reproduce

Open `0x00054C07`; the branches on the combat row and the calls to the random routine with 8,
16, 40, 32 and 24 are visible in each branch.
