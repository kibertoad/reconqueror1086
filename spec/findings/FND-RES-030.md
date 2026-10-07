---
id: FND-RES-030
title: SETUP extracts named members from SETUP.SOL, a directory of DH9 records followed by packed streams
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:05F1..0001:067A
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:11A8..0001:1259
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:125A..0001:1288
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:1288..0001:13DE
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:13DE..0001:1537
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:1538..0001:166D
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:166E..0001:16D3
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    address: 0001:16D4..0001:1757
  - build: BLD-GOG-EN
    file: CD:SETUP.SOL
    offset: 0x00..0xC3848
tool: capstone 5.0.7 bounded 16-bit disassembly of SETUP segment 1; Python bounded NE relocation-chain walk; Python bounded ISO 9660 root read
environment: null
---

## Observation

SETUP's length and canonical fingerprint matched BLD-GOG-EN; FND-RES-029
gives the segment mapping, the relocation walk and the reading of near
offsets as segment 2 offsets. Wine's `lzexpand.dll16.spec` names LZEXPAND
ordinals 2, 6, 7, 8 and 9 as `LZOpenFile`, `LZClose`, `LZStart`,
`CopyLZFile` and `LZDone`.

Callers. From 0001:05F1, `fn_0001_024E` takes SETUP's own path with
`GetModuleFileName`, replaces the part after the last `\` with `SETUP.SOL`,
and calls `fn_0001_16D4` with that path, the directory at segment 2 offset
0x1094, and in turn `_SETUP.EXE`, `SETUP32.EXE`, `MIDIEX.MID`,
`MIDIBASE.MID` and `EREGLIB.DLL`, then, by the language number of
FND-RES-029, `SOL_GER`, `SOL_FRE`, `SOL_SPA`, `SOL_ITA` or `SOL_ENG` with
`.DLL` and with `.HLP`.

`fn_0001_16D4(archive, directory, name)` stores the directory, clears a
member count and a byte position, and calls `fn_0001_1538(archive)`; a
result of 0 returns 0. Otherwise it allocates 12,574 bytes with
`GlobalAlloc` (KERNEL 15, flags 0x40), locks them with `GlobalLock` (18), calls `fn_0001_13DE(name)` and then
`fn_0001_1288` with the locked address, unlocks and frees the block (19 and 17), and
returns `fn_0001_1288`'s result.

`fn_0001_1538(archive)` opens the archive through a library routine at
0001:30C2 with mode 0; a nonzero result returns 0. It reads 6 bytes through
a library routine at 0001:30EA, adds the count read to the byte position,
and keeps a file length from a library routine at 0001:2BA6. Nothing tests
the 6 bytes. It then loops: `fn_0001_11A8` reads one record. When the
record's first 3 bytes differ from `DH9` (library routine at 0001:29E6 with
count 3), it closes the archive and copies the archive file with
`fn_0001_166E` to `<directory>\<archive>`, and returns 1. When the name at record offset 4 compares equal to `THE_END`
(routine at 0001:33F2), it returns 1. Otherwise it copies the record's 26
bytes into a new 32-byte block from `LocalAlloc` (KERNEL 5 in Wine's table,
flags 0x40), clears the block's
offsets 0x1A, 0x1C and 0x1E, links it after the previous one by offset
0x1A, counts it, and reads the next record. A read past the end of the file
is not tested.

`fn_0001_11A8(record)` clears record offsets 0x16 and 0x18, reads 22 bytes
into the record, and, only when the byte at offset 3 is `1` (0x31) or `8`
(0x38), reads 2 bytes into offset 0x16 and 2 into offset 0x18. Every read
adds its count to the byte position.

`fn_0001_13DE(name)` marks records for extraction by setting offset 0x1C.
A null name marks every record. A name without `*` marks each record whose
name at offset 4 compares equal to it through the routine at 0001:33F2. A
name with `*` takes a branch through library routines at 0001:348C,
0001:2CDE, 0001:29CA, 0001:29E6 and 0001:2D6E that this finding does not
read.

`fn_0001_1288(work)` walks the records in directory order. For each it
takes the 32-bit value at record offset 0x12 as the member's stored length.
For an unmarked record, `fn_0001_125A` moves the archive's file position
forward by that length through a library routine at 0001:25D6 with mode 1,
and adds it to the byte position. For a marked record it chooses an output
name: the record's own name, unless its first 4 bytes compare equal to
`SOL_` (routine at 0001:3434), in which case the name is `SETUPL.DLL` when
the 3 bytes at record offset 0x0C compare equal to `DLL` and `_SETUP.HLP`
otherwise. It creates `<directory>\<output name>` through a library routine
at 0001:3011 and makes a far call to 0001:41AB, an in-segment target fixed
up through segment 1's relocation chain, with the work block's far address and
the near values 0x1182, 0x1356, 0x1104 and 0x0F24; 0x1182 is the start of a
routine in the committed inventory, and the others are not read here. A result of 0 sets the return value to 1. When
record offset 0x16 is nonzero, offsets 0x16 and 0x18 are passed with the
output handle to a library routine at 0001:311E. The output is closed. Each
record's block is freed with `LocalFree` (KERNEL 7) after it is handled. A
nonzero result from 0001:41AB ends the walk. The archive is closed at the
end and the return value, 1 after any successful member, is returned. The
routine at 0001:41AB and what it does with the values passed are not read here.

`fn_0001_166E(source, target)` calls `LZStart`, opens the source with
`LZOpenFile` mode 0 and the target with mode 0x1000, copies with
`CopyLZFile`, closes both with `LZClose` and calls `LZDone`.

Shipped file. `CD:SETUP.SOL` was read from the owned disc image's root as
FND-RES-026 describes, with a 64 MiB cap; it is 800,840 bytes. Its first 6
bytes are ASCII `DISK01`. From offset 6 it holds 16 records of 26 bytes, each
starting `DH9` and with `1` at offset 3, the last named `THE_END` (at offset
396), so the directory ends at offset 422. The stored lengths of the first 15
sum to 800,418, which is the number of bytes after offset 422. Their names,
in order, are `SETUP32.EXE`, `_SETUP.EXE`, `SOL_ENG.DLL`, `SOL_SPA.DLL`,
`SOL_GER.DLL`, `SOL_FRE.DLL`, `SOL_ITA.DLL`, the five `.HLP` files in the
same language order, `MIDIBASE.MID`, `MIDIEX.MID` and `EREGLIB.DLL`, so the
`DLL` test at record offset 0x0C (name position 8) is true for the
`SOL_xxx.DLL` names. Each member's stored bytes, at the offset the earlier
lengths give, start with the two bytes 0x00 and 0x06. Every record's word at
offset 0x16 lies from 0x1D37 to 0x1F5A.

## Interpretation

SETUP.SOL is the first stage's member archive: a 6-byte header that SETUP
ignores, a directory of `DH9` records ended by `THE_END`, and the members'
stored streams packed in directory order. SETUP extracts `_SETUP.EXE` and
its companions into the directory at 0x1094 before running `_SETUP.EXE`
(FND-RES-029), and renames the chosen language's DLL and help file.

The stream format is the routine at 0001:41AB's. Its leading bytes 0x00 and
0x06 and the 12,574-byte work block match the header and the documented
work-buffer size of PKWARE's Data Compression Library "explode", in binary
mode with a 4,096-byte dictionary; this is circumstantial until 0001:41AB
is read. The words at record offsets 0x16 and 0x18 fit a DOS date and time
(1994 to 1995), which is also circumstantial.

## Alternatives

- A 26-byte record for every type: ruled out; only types `1` and `8` read
  the last 4 bytes.
- A header that SETUP checks: ruled out; the 6 bytes are read and never
  tested.
- An offset field per member: ruled out for SETUP's reader, which finds each
  member by adding the lengths of the records before it.

## How to reproduce

Verify SETUP's manifest size and XXH3-128 and map it as FND-RES-024 does.
Disassemble the listed ranges of segment 1 as 16-bit code and name imports
with the relocation walk of FND-RES-029 and Wine's `krnl386.exe16.spec` and
`lzexpand.dll16.spec`. Read `SETUP.SOL` from the disc image's root with the
stated cap, parse the header and 22- or 26-byte records to `THE_END`, sum the
stored lengths and compare the sum with the bytes after the directory, and
read the first two bytes of each member. Keep listings and member bytes in
ignored local storage.
