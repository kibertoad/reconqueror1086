---
id: FND-VIEW-010
title: The pixel test reads a block texture by its width, width shift and height, and treats palette index 0 as transparent
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000444E8
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x000444E8` reads one pixel of a block's texture, using the texture width at block
`+0x10`, its base-2 logarithm at `+0x14` and its height at `+0x18`, and reports the pixel as
empty when its palette index is 0.

## Interpretation

Palette index 0 is the transparent colour of scene textures.

## Alternatives

None known.

## How to reproduce

Open `0x000444E8` from its call in `0x000470A8`.
