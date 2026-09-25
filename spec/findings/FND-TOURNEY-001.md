---
id: FND-TOURNEY-001
title: Each month the game picks a tournament site different from the last one
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002B1DE..0x0002B1E3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C550..0x0005C5CA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C5CC..0x0005C613
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C380..0x0005C3DD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C520..0x0005C54E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00010FF0..0x00011021
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C3FC..0x0005C51E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004377C..0x00043792
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0005C550` draws `0x00024C38(13)` until the result differs from the dword at `0x0009DC34`, then
stores it there and at offset 0 of the 12-byte block the pointer at `0x0009DC30` points to. It
stores `0x0004377C(site)`, the byte at `0x0009B8F5 + 15 * site`, at offset 4 and 0 at offset 8, and
sets `0x0009DC38`, `0x0009DC3C`, `0x0009DBCC` and `0x0009DED4` to 0. It leaves `0x0009DC40` alone.
`0x0005C5CC` then shows a message that there is a tournament at the place `0x00043740` names for
offset 4. Both are called from `0x0002B1DE` and `0x0002B1E3`, at the end of a routine that just
set row 0's WEALTH. `0x0005C380` allocates the block, fills its three dwords with -1, sets
`0x0009DBCC`, `0x0009DC38`, `0x0009DC3C` and `0x0009DC40` to 0 and `0x0009DED4` to 1; it is called
once, at `0x0002A08B`. `0x0005C520` sets `0x0009DC38`, `0x0009DC3C`, `0x0009DC40` and `0x0009DED4`
to 0, stores -1 at offset 4 and clears `0x0009DBCC`; no direct call to it was found. `0x00010FF0`
also sets `0x0009DC38`, `0x0009DC3C` and `0x0009DC40` to 0. `0x0005C3FC` writes the block and then
the three dwords at `0x0009DC38`, `0x0009DC3C` and `0x0009DC40` to a file, and `0x0005C47C` reads
them back in the same order.

## Interpretation

A new tournament is announced when the routine at `0x0002B1DE` runs, likely at the turn of the
month, and it resets the day's joust and melee counts but not the count of wins, which only a new
session or the other resets clear. The site number is not a character row; it indexes a table of
15-byte records whose byte at offset 0 is the place.

## Alternatives

The routine that ends at `0x0002B1DE` may run at another time than the turn of a month.

## How to reproduce

Disassemble the listed ranges, and scan object 1 for calls to `0x0005C550`, `0x0005C380`,
`0x0005C520` and `0x00010FF0`.
