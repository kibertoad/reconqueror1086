---
id: FND-MEDIA-003
title: Sprite frames are drawn by three row blitters: a copy, a clipped copy and a one-colour mask
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007CEB0..0x0007CF13
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008A564..0x0008A5BC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008A794..0x0008A7E4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008A7E8..0x0008A86C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007DC00..0x0007DD14
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007CB24..0x0007CB95
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007E330..0x0007E444
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008A870..0x0008A8CD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008A8D0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006562C..0x00065767
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0008A564(dest, rows, height, skip, colour)` draws `height` rows. For each row it reads the
segment count byte, then for each segment the operation byte and an `INT16LE` length: operation 0
steps over `length` source bytes and fills `length` destination bytes with `colour`, operation 2
steps over one source byte and fills `length` bytes with `colour`, and any other operation moves the
destination on by `length`. After each row it adds `skip` to the destination. `0x0007CEB0(x, y,
colour, frame)` calls it with `dest = 0x000B0840 + y * 0x000B0854 + x`, the frame's rows,
height, `0x000B0854 - width` and `colour`, then passes the drawn rectangle to `0x0006DF90`.
`0x0007CEB0` has five callers: the two text routines of FND-MEDIA-004, `0x000651E3`, `0x00065263`
and the dispatcher below.

`0x0008A794(dest, rows, height, skip)` is the same loop with operation 0 copying its `length`
source bytes and operation 2 filling with its one source byte. `0x0008A7E8` is the clipped form: it
takes a left skip and a visible width, steps each row's segments until the skip is used up, draws
at most the visible width, and takes each row start from a row table. `0x0007DC00(x, y, frame,
row_table)` clips the frame's rectangle against the four dwords at `0x000B0810` through
`0x0007CB24`, draws nothing when it lies outside, calls `0x0008A794` when no edge is cut and
`0x0008A7E8` otherwise, and passes the drawn rectangle to `0x0006DF90`.

`0x0007E330(x, y, frame, object)` draws a frame of a second sprite object, built by `0x0007E0E0`
and freed by `0x0007E220`, choosing the routine from the `UINT16` at object `+2` through a table at
`0x0007E310`: mode 0 draws the mask through `0x0007CEB0`, mode 1 copies a plain rectangle through
`0x0008A870`, mode 2 draws the rows unclipped through `0x0008A8D0`, modes 3 to 6 draw clipped through
`0x0007DC00` (4 adds a per-frame x offset, 5 a y offset, 6 both) and mode 7 draws nothing. The
text routine at `0x0006562C` uses it to draw strings with the object at `0x0009DFD0`: `a` to `z` are
frames 0 to 25, `A` to `Z` frames 26 to 51, `0` to `9` frames 52 to 61, and other characters
frame `0x3D` plus the number `0x00065410` returns, or nothing when it returns 0.

## Interpretation

Operation 0 is literal pixels, 1 is a transparent run and 2 a run of one colour. The mask routine
ignores the frame's own colours, so a font frame is a shape drawn in the caller's colour.

## Alternatives

None known for the three blitters. What loads the object at `0x0009DFD0` and which file it comes
from was not traced.

## How to reproduce

Disassemble the listed addresses in `CD:CONQUER.EXE` and the table at `0x0007E310`.
