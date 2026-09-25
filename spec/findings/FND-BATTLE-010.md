---
id: FND-BATTLE-010
title: The rectangle hit test returns the first containing rectangle, counted from 1
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006FF10..0x0006FF4C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00064164..0x0006419B
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0006FF10` takes a point, a count and a list of eight-byte `(left, top, width, height)` rectangles,
tests them in order with `0x00064164`, and returns the one-based index of the first hit, or 0.
`0x00064164` tests `left <= x < left + width` and `top <= y < top + height`.

## Interpretation

Rectangles are half-open.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0006FF10..0x0006FF4C`, `0x00064164..0x0006419B`.
