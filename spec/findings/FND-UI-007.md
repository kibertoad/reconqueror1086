---
id: FND-UI-007
title: The store offers a random share of WEAPONS.DAT and lists the owned items it can buy back
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006137C..0x000619D7
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Click routine 2 of screen 13, `0x0006137C`, calls `0x00064030(1)`, disables regions 0 to 2 and
enables regions 3 to 6 through `0x00059D34`, shows picture `0x13F`, loads `SWORDS.CSF` and
`BUYSELL.CSF` through `0x00018430`, and opens `weapons.dat`. It takes `s = 0x000AC180 & 3` and
reads records 0 to 38, 39 in all. For each it reads a line of up to 600 bytes and cuts it at LF,
then reads three numbers with `"%d\n"` into a local `c`, `0x000B050C` and `0x000B0510`. When `record
% 4 == s`, `0x0004310C` of the third number is 0, and `0x00024C38(20)` is below `c`, it appends a
20-byte entry at `0x000B0504`: the movie line copied at `+0x0C`, or 0 when the line starts with `#`,
the second number at `+0x04` and the third at `+0x08`, then reads the price into `+0x00` and the
next line into a copy at `+0x10`, and counts it. Otherwise it reads the price into `0x000B0508` and
the next line. It then returns to the start of the file through `0x0006B4C3` and reads records 0 to 38
again; for each whose third number
passes `0x0004310C` with 1 and whose record number is not 16 it appends an entry the same way without
counting it. It stores the count of the first pass in `0x000B0500`.

The `weapons.dat` entry decodes to 8,209 bytes: 40 records of six CRLF lines (movie name or `#`, a
number from 2 to 20, a frame, an item, a price, a description), then an empty line. Records 0 to 22
name a movie and 23 to 39 have `#`; each of the 23 names has an entry in the ISO 9660 directory of
the installation's disc image `game.gog`. Frames and items run 0 to 36 in step except records 3 and 10,
which swap frames 10 and 3, and records 37 to 39 (frame 38 item 37, frame 37 item 38, frame 38 item
37). `SWORDS.CSF` has 39 frames and `BUYSELL.CSF` four: two of 81 by 71 and two of 100 by 32.

## Interpretation

The second number is the chance, out of 21, that a record appears in a visit's stock. Each place
stocks the records of one residue of the record number modulo 4, and never an item the player owns.
Record 39 is never read. Record 16's item cannot be sold.

## Alternatives

The entries of the second pass lie after the counted ones, and what bounds them was not traced.

## How to reproduce

Disassemble `0x0006137C` to `0x000619D7`; decode `weapons.dat` from `C1086.GOB`.
