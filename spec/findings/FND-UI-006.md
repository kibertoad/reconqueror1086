---
id: FND-UI-006
title: VILLAGE.DAT has 67 records and TVILLAGE.DAT 12, with one row of VILLAGE.DAT that stops after two numbers
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009BA50..0x0009C6AF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B8F5..0x0009B9B8
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The `village.dat` entry of `C1086.GOB` decodes to 11,120 bytes of CRLF lines: 67 records of seven
lines, a background name of the form `V<number>_<four digits 0 or 1>.PCX` and six rows. The
`tvillage.dat` entry decodes to 2,021 bytes: 12 records, backgrounds named `T...`. Every row has the
form `enabled,x,y,width,height ; label` except three rows of `village.dat`: line 115 (record 16, row
1) reads `1,,539,374,100,59`, line 131 (record 18, row 3) ends its numbers with a comma, and line 265
(record 37, row 4) reads `1,356 141 43,52`. Row 1 of record 15 is `1,539,374,100,59` and row 4 of
record 36 is `1,59,108,75,69`. Line 11 has a space after a comma. Thirteen `village.dat` backgrounds,
records 14 to 18, 20, 32, 38, 46, 48, 58, 59 and 61, and the `tvillage.dat` background of record 4
are absent from the archive. The byte at `+0x0F` of the 175 used person records takes values from 0
to 66; none selects a record whose background is absent, and persons 21, 28, 81, 87, 90, 141, 146
and 147 select record 37. The 14 tournament sites' places, the byte at `0x0009B8F5 + 15 * site`, have
values 0 to 3, 5 to 7 and 9 to 11 there.

## Interpretation

In the original, row 1 of record 16 reads as its predecessor's same row, which has the same numbers,
so that defect has no effect. Row 4 of record 37 keeps x = 356 but takes y, width and height 108, 75
and 69 from record 36, so the lender's hot spot is misplaced in that village. The trailing comma is
harmless. The missing backgrounds are never shown.

## Alternatives

The sscanf of the original runtime could accept a separator the C library normally rejects; the
reading above assumes the standard behaviour, where matching stops at the first mismatch.

## How to reproduce

Decode the two entries of `C1086.GOB`, check the line forms, and read the person bytes and the
tournament table from the executable's data.
