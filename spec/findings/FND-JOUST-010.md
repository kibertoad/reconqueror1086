---
id: FND-JOUST-010
title: The dragon lance moves by the practice formula with pull 256 per 10 pixels and an upward jerk of 5,000
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B644..0x0001BA25
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B93D..0x0001B956
tool: Ghidra 12.1.3
environment: null
---

## Observation

Worker `0x0001B644`..`0x0001BA25` starts the lance at 8.8 position `(225, 150)` with no
velocity. Each pass it adds each velocity to the 8.8 position, takes the integer position by
division toward zero, and when that crosses x 50 to 400 or y 20 to 280 sets it to the bound and
the 8.8 position to the bound shifted left by 8. It then sets each velocity to
`velocity * 80 / 100 + 256 * ((pointer - position) / 10)`, with the pointer dwords at
`0x000B07E8` and `0x000B07EC`, and subtracts 5,000 from the y velocity when the frame counter
is a multiple of `500 / 71`, 7. At `0x0001B93D`..`0x0001B956` it paints the lance frame at
`(x, y + 90)`.

## Interpretation

The dragon lance moves exactly as the practice lance does with `p = 50`, since
`((d / 10) * 0x3200) / 50` is `256 * (d / 10)` and `(150 - 50) * 50` is 5,000.

## Alternatives

None known.

## How to reproduce

Open `0x0001B644`; compare with FND-JOUST-002.
