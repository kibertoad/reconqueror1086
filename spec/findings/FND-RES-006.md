---
id: FND-RES-006
title: The game builds three archive paths from CONQUER.INI and reopens C1086.GOB after reading any other archive
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002AA54..0x0002AAFD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A3D6..0x0002A409
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062EA0..0x00062ECE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024DB8..0x00024E49
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059FA0..0x0005A09A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021E88..0x00021EE2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050B6C..0x00050BD6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050CCB..0x00050D59
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000524A8..0x000524C9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000549B9..0x000549C1
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0002AA54` builds three paths with `0x00063014`, a `sprintf`. `0x00062EA0(key)` walks the list
at `0x000B0520` and returns the value stored for `key`, or 0. With `DATA` (default `.\DATA\`) it
writes `%sC1086ad.GOB` to `0x000A9D90`; with `CD_PATH` (default `.\CD_PATH\`) it writes `%s` to
`0x000A9D40`; with `GOB` (default `.\GOB\`) it writes `%sC1086.GOB` to `0x000A9CF0`. Startup
prints `Conq-Gob: Initializing master GOB file system.` and opens `0x000A9CF0` with `rb` at
`0x0002A3ED`, stopping with the message at `0x00093908` when that fails.

The picture routine `0x00024DB8(mode, number, object)` reads from the open archive when `mode` is
not 0, and the HAT loader `0x00059FA0(name, mode)` when `mode` is 1. Otherwise they close the archive, open
`0x000A9D90` with `rb`, stopping with an error when it cannot be opened, read the resource, close it
and open `0x000A9CF0` again. The music and sound loaders also open `0x000A9D90`, at
`0x0005B13A` and `0x0005B5E4`.

The scene loader `0x00050B6C(path, low)` opens its scene archive with `0x00049200`. Both callers
pass `low` 1, and the one at `0x000524C4` builds the path from `0x000A9D40` and a scene name. After reading `Viewer` and
`Scenario`, when `low` is not 0 and the dword at `0x0009CB7C` divided by 1024 is above 0, it drops
the last three characters of the path, appends `LOW`, closes the archive and opens the `.LOW` file,
stopping with `Unable to open Resource` when that fails. `0x00021E88` closes the archive with
`0x00049478(0)` before a scene and opens `0x000A9CF0` with `rb` again after it.

The release has no `C1086ad.GOB`.

## Interpretation

`C1086.GOB` is the game's resident archive. Every other archive is opened in its place and
`C1086.GOB` is reopened afterwards, because only one archive can be open (FND-RES-002). The `DATA`
archive `C1086ad.GOB` is an optional second archive for pictures, layouts, music and sounds that
the shipped game does not include; a call with mode 0 would stop the game. The scene loader takes
the `.LOW` file only when the dword at `0x0009CB7C` is at least 1024.

## Alternatives

Whether any call passes mode 0 at run time was not established: 42 of the 65 calls to `0x00024DB8`
push 1, and the others pass a register or a value computed before the call. The only writes found
to `0x0009CB7C` are in the scene writer (FND-RES-005), which suggests the shipped game always
reads the `.RES` file, but no run confirmed it.

## How to reproduce

Disassemble the listed ranges and find the references to `0x000A9CF0`, `0x000A9D40`, `0x000A9D90`
and `0x0009CB7C`.
