---
id: FND-STRATEGY-018
title: A route point maps to a grid cell through staggered diamonds, scanned in camera order, whose lower half is one line taller
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063E20..0x00063EBF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000629B0..0x00062B7C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E5A0..0x0006E8D8
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063EC0..0x00063EDC
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00063E20(row, col)` returns `(80 * (row + 1), 20 * (col + 1))` for the 80 by 80 cells.
`0x000629B0(x, y)` starts from the camera's row `r` and column `c` at `0x000AB104` and `0x000AB108`
and repeats until it makes no change: when `80 * r + 40 > x` it subtracts `(80 * r + 40 - x) / 80`
from `r`, and 1 more when `r` is then above 0; when `x > 80 * r + 423` it adds `(x - 80 * r - 40) / 80`
and then subtracts 1 when `r` is above 0; when `20 * c + 20 > y` it subtracts
`2 * ((20 * c + 20 - y) / 40)` from `c`, and 1 more when `c` is then above 0; when
`y > 20 * c + 454` it adds `2 * ((y - 20 * c - 20) / 40)` and then subtracts 1 when `c` is above 0.
It installs that camera, calls `0x0006E5A0(x - 80 * r - 40 + 20, y - 20 * c - 20 + 7)` and puts the
camera back.

`0x0006E5A0(px, py)` finds no cell when `px` is outside 20 to 403 or `py` outside 7 to 441. It starts
`y` at -53 and, while `y` is below 401, scans two lines of cells: the first from `x = -20` when the
camera column is even and 20 when it is odd, the second from 20 when even and -20 when odd. Each line
starts its row at the camera row, adds 80 to `x` and 1 to the row (wrapping at 200) after each cell
while `x` is below 403, and is followed by adding 1 to the column (wrapping at 400) and 20 to `y`;
the first line has the camera column. For each cell it walks the lines `yy` from `y + 40` with the
span `x + 40 - w` to `x + 40 + w`, starting at `w = 0`: it tests the point against the span, adds 2
to `w` when `yy` is at most `y + 60` and subtracts 2 otherwise, adds 1 to `yy`, and stops when `w` is
below 0. It returns the first cell whose span holds the point. `0x00063EC0` sets the rounding to
toward zero, rounds the x87 top of stack to an integer and restores the rounding.

Projected this way, all 8,304 points of the 102 `rt_` and `sc_` files find a cell with the camera at
`(0, 0)` and at `(199, 399)`, and 78 of them, at 49 coordinates, find a different cell with one
camera than with the other.

## Interpretation

The anchor is the top vertex of an even column's cell and the centre of an odd column's. A cell
covers the lines `y + 40` to `y + 82`: its half-width grows by 2 a line to 40 at `y + 60`, is 42 on
the next line and then shrinks by 2 a line, so the lower half is one line taller and 2 wider than
the upper. Lines where two cells overlap belong to whichever the scan reaches first, which depends on
the camera, so the camera is part of the state that decides a movement's cell.

## Alternatives

A symmetric cell, `abs(px - cx) + 2 * abs(py - cy) <= 40` about `(x + 40, y + 60)`, does not match
the scan: with the camera at `(0, 0)` it gives a different cell for 610 of the 8,304 route points.

## How to reproduce

Disassemble the listed ranges of `CD:CONQUER.EXE`, then project every point of the `rt_` and `sc_` files with both camera extremes.
