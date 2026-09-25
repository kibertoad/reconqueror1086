---
id: FND-UI-009
title: The conversation screen draws five 558-by-35 response rows from (41, 302) and picks a row by pointer height
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019D08..0x00019D93
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001A3B8..0x0001A54D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00066640..0x000666AF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00019CC0` shows picture `0x12E` and creates text windows through `0x00064320(x, y, width,
height)`: five of `(41, 302 + 35 * k, 558, 35)` for `k` from 0 to 4, kept from `0x000A96BC`, one of
`(264, 32, 322, 215)` at `0x000A96A0` and one of `(34, 236, 205, 20)` at `0x000A96A4`. `0x0001A3B8`
tests the pointer at `0x000B07E8` and `0x000B07EC` against the rectangle at `0x000A9660` through
`0x0006B480`; inside it, a y below 337 selects row 0, below 372 row 1 and below 407 row 2, and the
next rows follow the same steps. The row under the pointer is redrawn in the colour at `0x0009A714` and the
row it leaves in the colour at `0x0009A710`. The routine around `0x00066640` compares a keyword with
a list of strings, among them those from `0x00099224`, and returns a number for each: `WORDWRAP`
gives 11.

## Interpretation

Responses are chosen with the pointer over five fixed rows; the prompt sits in the upper window. The
keyword routine parses a text-box description whose keywords include word wrapping.

## Alternatives

Where the keyword routine is called from, and which keys the response picker accepts, were not
traced.

## How to reproduce

Disassemble the listed ranges.
