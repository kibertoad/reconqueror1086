---
id: FND-UI-004
title: The village exterior takes its six hot spots from a record of VILLAGE.DAT or TVILLAGE.DAT
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006076C..0x00060B72
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00060E1C..0x00060F0F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043080..0x000430AD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C614..0x0005C67C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038678..0x00038680
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The routine at `+0xAC` of screen 11, `0x0006076C`, first stores 0 in `0x0009DF04`, calls
`0x0005B2C0` and stores -1 in `0x0009AAC4` when `0x0009DF04` is not 0, and loads resource `0x18D`
through `0x0005B0D4`, stores 1 in `0x0009DED8` and `0x18D` in `0x0009AAC4` when `0x0009DED8` is 0.
It stores 0 in `0x0009DEF8` and loads resource `0x164` into `0x0009DF00`. When
`0x0009DED4` is 0 it stores `0x00038678()`, the dword at `+0x1C` of the block the pointer at
`0x0009AE00` points to, at `+0x08` of the tournament block (`0x0005C648`), and stores 1 in
`0x0009DED4` when `0x000AC180` equals the dword at `+0x04` of that block (`0x0005C614`), else 0. It
opens `tvillage.dat` when `0x0009DED4` is not 0 and `0x000AB0C4` differs from `0x000AC180`, and
`village.dat` otherwise, with mode `rt`. `0x00043080(p)` returns the byte at `0x0009BA5F + 18 * p`,
showing an error when it is above 66, and the routine reads records 0 to that value of `0x000AC180`
in turn into the same stack buffers. For each record it reads a line of up to 80 bytes with
`0x00065025`, cuts it at the first `;` and at the first LF, and copies 15 bytes of it with
`0x00068288`; then for each of six rows it reads a line the same way and calls
`0x00065108(line, "%d,%d,%d,%d,%d", &row+0, &row+4, &row+8, &row+0x10, &row+0x0C)`, the rows 20 bytes
apart. After the last record it closes the file and, for row `i` from 0 to 5, calls
`0x00059C70(row+4, row+8, row+0x10, row+0x0C, i)` when the dword at `row+0` is not 0 and
`0x00059D34(i, 0)` otherwise. It then draws the name of `0x000AC180` from `0x00043740` at `(230,
455)` and `(231, 456)`.

The slot-0 routine `0x00060E1C` draws the dword at `0x0009DEE0 + 4 * r` at the same two points for
region `r` below 7 and calls `0x00064030(5)`; the slot-1 routine `0x00060E98` calls
`0x00064030(0)` and redraws the name.

## Interpretation

The label after `;` is a comment: the game discards it and gives row `i` to region `i`, so the
action of a row depends on its position. The fourth number is the width and the fifth the height. A
row whose numbers stop early keeps the values the same row of the record before had. The tournament
catalog is used only for a tournament away from the player's home, and both catalogs are indexed by
the byte at `+0x0F` of the person record.

## Alternatives

The label table at `0x0009DEE0` is filled elsewhere; what fills it was not traced.

## How to reproduce

Disassemble `0x0006076C` to `0x00060B72` and `0x00060E1C` to `0x00060F0F`, and the helpers named.
