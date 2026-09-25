---
id: FND-TALK-006
title: Conversation variables come from ALL.VTB and are read and written by index
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00022564..0x0002258D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062610..0x0006280F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062828..0x00062891
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000628AC..0x00062915
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00022564` calls `0x00062610("all")` and stores the result in `0x0009A928`, ending the program if
it is null. `0x00062610` opens `"%s.vtb"` (`"%s" "." "vtb"`) with `"rb"`, reads three dwords into
`+0x04`, `+0x08` and `+0x0C` of a 20-byte record, allocates `+0x04 * +0x08` bytes at `+0x10` and
reads `+0x04` elements of `+0x08` bytes into them. `0x00062828(table, v, &out)` and
`0x000628AC(table, v, &value)` fail with 0 when `v` is negative or greater than `+0x04`, and
otherwise copy one element between the table and the pointer, one, two or four bytes as the kind at
`+0x0C` is 0 or 1, 2 or 3, or 4 or 5, and give 1. In the GOG archive `ALL.VTB` is 772 bytes: a count
of 190, a size of 4 and a kind of 5.

## Interpretation

The table is a flat array of 32-bit variables numbered from 0. The bound check lets `v` equal the
count, one element past the allocation.

## Alternatives

None known.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
