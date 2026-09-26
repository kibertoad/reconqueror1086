---
id: FND-SAVE-005
title: Startup writes default.dat from the pristine person and property tables, and a new game reads it back
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00042E4C..0x00042F35
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00042F64..0x00043000
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A74C..0x0002A751
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00037B6E..0x00037B7D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A930..0x0002A9B0
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Startup calls `0x00042F64` at `0x0002A74C`, after registering the screens. It writes `default.dat`
(mode `wb`; `Unable to Write default.dat` on failure): for each of the 176 person records at
`0x0009BA50`, 4 bytes from `+0x06` and 4 from `+0x07`, storing `0xFF` in `+0x0D` after each; then 14
records of 15 bytes from `0x0009B8EC`.

When a new game starts, the options screen's routine calls `0x00042E4C` at `0x00037B73`. It stores
0 at `0x0009ABE8` and `0x0009B718` and `0xFF` at `0x0009B8C0` and `0x0009B8C4`, frees the
conversation variables at `0x0009A928`, stores 0 in the 70 item counts at `0x0009C6C8 + 8 * i`, and
reads `default.dat` (mode `rb`; `Unable to Read default.dat`, code 0, on failure) in the same layout,
storing `0xFF` in `+0x0D` of each person.

At `0x0002A930`, before the screens start, startup deletes `temp.jap`, `ictempmf.jp`,
`ictempmt.jp`, `ictempmc.jp`, `ictempmm.jp`, `all.cbf`, `all.cif`, `all.tmb` and `all.tmi` through
`0x000681D1`.

## Interpretation

`default.dat` is a snapshot of the persons' and properties' starting values, taken from the
executable's data before any play, so that a second new game in the same session starts from the
same state as the first. It is written to the current directory, `C:\` in the GOG configuration,
on every start. The person list links start empty in both cases.

## Alternatives

Whether anything else writes `default.dat` was not searched beyond the two direct calls.

## How to reproduce

Disassemble the listed ranges; the strings are at object-2 offsets `0x71DC` to `0x7204` and
`0x3A84` to `0x3AB4`.
