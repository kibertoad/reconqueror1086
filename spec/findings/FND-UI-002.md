---
id: FND-UI-002
title: The HAT loader reads a 40-byte header and 24-byte region records, taking the region count from the file size
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059FA0..0x0005A17F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005A2A4..0x0005A413
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059D70..0x00059E75
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059C4C..0x00059C6D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059C70..0x00059CDA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059D34..0x00059D4C
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00059FA0(name, flag)` allocates a 184-byte screen object and fills it through `0x0005A2A4`.
`0x0005A2A4` takes the file size from `+0x2C` of the open file, computes `(size - 0x28) / 24` as an
unsigned division, reads the whole file, and copies its first 40 bytes. It stores the dword at `0x00`
at object `+0x60`, the dwords at `0x04`, `0x08`, `0x0C` and `0x10` at `+0x50`, `+0x54`, `+0x58` and
`+0x5C`, and the dword at `0x14` at `+0x64`, copies 13 bytes from `0x18` to `+0x00` with
`0x00068288`, and allocates `4 * (dword at 0x14)` bytes for the region pointers at `+0x6C`. For each
of the `(size - 0x28) / 24` records, in file order, it calls `0x00059D70(x, y, width, height, id,
enabled)` with the six dwords of the record, which are id, x, y, width, height and enabled in that
order, and stores the result in the next pointer slot. `0x00059D70` allocates a 104-byte region and
an 8-byte rectangle, stores x, y, width and height as 16-bit values in the rectangle, the id at region
`+0x04`, the enabled dword at `+0x08`, and clears ten callback slots from `+0x40`. `0x00059C4C(index,
slot, routine)` stores `routine` at `+0x40 + 4 * slot` of the region at position `index` of the
pointer array. `0x00059C70(a, b, c, d, index)` passes its four values to the routines at `+0x1C`,
`+0x24`, `+0x2C` and `+0x34` of the region at `index`, and `0x00059D34(index, value)` stores `value`
in the region's enabled dword.

The 27 HAT entries of the archive decode to 64 to 568 bytes. In each, the dword at `0x14` equals
`(size - 0x28) / 24`, every declared screen is 640 by 480 at `(0, 0)`, and in the 25 registered files
the screen number at `0x00` matches the registration number of the file (FND-UI-001). The two
unregistered files carry 4 (`DBATH.HAT`) and 15 (`VSTABLES.HAT`). The name at `0x18` ends with a NUL
within its 13 bytes; bytes `0x25` to `0x27` are `C0 45 00` in all 27, and byte `0x24` is 0 or `0x6D`.
`TOPTS.HAT` ends with the three bytes `0D 0A 1A` after its five records. The region ids of
`CHARGEN.HAT` run 0 to 4, then 6, then 5. Region 11 of `GAMEOPTS.HAT` and regions 3 to 6 of
`VSMITH.HAT` have enabled 0; every other region has 1.

## Interpretation

The layout is fixed-size records after a fixed header, and `0x00059C70` sets a
region's x, y, width and height. A callback bound by index follows file
order; the id field is kept but the binding does not use it. Bytes `0x25` to `0x27` are never read.

## Alternatives

What reads the region id and the enabled dword was not traced.

## How to reproduce

Disassemble the listed ranges; decode the HAT entries of `C1086.GOB` and compare their sizes with the
dword at `0x14`.
