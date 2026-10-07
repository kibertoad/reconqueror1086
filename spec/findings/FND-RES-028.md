---
id: FND-RES-028
title: AUTOPLAY's directory enumeration supplies each entry's long name
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00411140..0x00411199
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00411199..0x004111E1
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x004122C5..0x00412300
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00412300..0x0041233F
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00412CC7..0x00412CEC
tool: capstone 5.0.7 bounded disassembly of the listed AUTOPLAY ranges
environment: null
---

## Observation

AUTOPLAY's length and canonical fingerprint matched BLD-GOG-EN. The import
slots are those FND-RES-026 names: 0x00440140 `FindFirstFileA` and 0x00440144
`FindNextFileA`.

`fn_004109B8` calls `fn_00411140` with the search pattern in EAX, the
attribute word 0x10 in EDX and a record address in EBX, and later calls
`fn_00411199` with that record in EAX (FND-RES-026).

`fn_00411140` calls `FindFirstFileA` with the pattern and a find-data buffer
of 0x140 bytes on its stack. A result of -1 is stored in the record's first
dword and the routine returns through an error routine at 0x0041226A. Otherwise
it calls `fn_00412300` with the handle, the attribute word and the find data.
A zero result returns through an error routine at 0x00412228 with the value 2.
A nonzero result stores the handle in the record's first dword and the
attribute word in its second, calls `fn_004122C5` to fill the record from the
find data, and returns 0.

`fn_00411199` calls `FindNextFileA` with the stored handle and a fresh
find-data buffer. A zero result returns through the error routine at
0x0041226A. Otherwise it calls `fn_00412300` with the stored handle and
attribute word; a zero result takes the same error exit, and a nonzero one
fills the record through `fn_004122C5` and returns 0.

`fn_00412300` makes a mask from the attribute word by setting bits 0x01, 0x20
and 0x80 of its low byte and clearing 0x08 there if it is set; for the word
0x10 the mask is 0xB1. It returns 1 when the find data's attribute dword
(offset 0) shares a bit with the mask. Otherwise it calls `FindNextFileA` on
the same handle and buffer, tests again on success, and returns 0 when that
call fails. It has no other exit.

`fn_004122C5` copies, from the find data into the record: a conversion of the
dword at offset 0x14 (the last-write time) through `fn_00412279` into record
offsets 0x16 and 0x18, the attribute byte at offset 0 into record offset
0x15, the dword at offset 0x20 (the low size) into record offset 0x1A, and up
to 255 bytes of the name at offset 0x2C into record offset 0x1E through
`fn_00412CC7`, then a 0 at record offset 0x11D. `fn_00412CC7` copies bytes
until a 0 or until its count runs out and fills the rest of the count with 0.
Offset 0x2C of the find data is `cFileName`; `cAlternateFileName` at offset
0x130 is not read.

`fn_004109B8` reads the attribute byte at record offset 0x15 and the name at
record offset 0x1E (FND-RES-026), so the name it compares with `DirName` and
`SIERRA` is `cFileName`, cut to 255 bytes.

## Interpretation

AUTOPLAY matches `DirName` and `SIERRA` against the long name Windows reports
for a directory, and every entry whose attributes include read-only,
directory, archive or normal reaches `fn_004109B8`, which then keeps only
directories. Which names the pattern `*.` makes Windows return, and whether
`cFileName` holds the long name, are the operating system's behaviour.

## Alternatives

- The short name could be the one compared: ruled out, since only offset 0x2C
  is copied.
- An entry that fails the attribute test could end the search: ruled out for
  every case but a failing `FindNextFileA`, since `fn_00412300` moves on to
  the next entry itself.

## How to reproduce

Verify AUTOPLAY's manifest size and XXH3-128 and map BEGTEXT from file offset
0x400 to 0x00410000. Disassemble each listed range from its first address to
its return, and follow each call to its import jump or listed routine. Keep
listings in ignored local storage.
