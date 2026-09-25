---
id: FND-VIEW-019
title: The world renderer computes a kind-4 block's texture column and angle the same way as the raycaster
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00046E3B..0x00046F23
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x00046E3B`..`0x00046F23` the world renderer forms a kind-4 block's centre as
`(cell << 8) + 0x80 + offset` on each axis, subtracts the view position, rotates the difference
through `0x000447C4`, and computes the column `((lateral * depth) >> 14) - centre_lateral + 0x80`,
marking the block not drawn when that is outside 0 to `0xFF` and shifting it right by
`8 - width_shift`. When block `+0x2C` is not -1 and the angle count at `+0x34` is above 0, it
takes `sector = ((((0x80 / count) + block[+0x38] - view_heading) & 0xFF) * count) >> 8`, and
when behaviour bit 2 is set and the sector is above `count / 2` it mirrors the column and uses
`count - sector`.

## Interpretation

The drawn sprite angle and the angle the pointer test uses (FND-VIEW-009) always agree.

## Alternatives

None known.

## How to reproduce

Open `0x00046E3B`; the `idiv` by block `+0x34` and the `test dl, 4` follow the call to
`0x000447C4`.
