---
id: FND-UI-011
title: Picture, sound and music numbers are indexes into the C1086.GOB directory
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024DB8..0x00024DB8
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B0D4..0x0005B0D4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B584..0x0005B584
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The numbers the screens pass to the picture routine `0x00024DB8(mode, number, object)`, the music
routine `0x0005B0D4(number, 1)` and the sound loader `0x0005B584(1, number)` name these entries of
the `C1086.GOB` directory when read as zero-based indexes: `0x12E` (302) `comscrn1.pcx`, `0x139` (313)
`forgesmi.pcx`, `0x13A` (314) `forge.pcx`, `0x13F` (319) `swdtemp.pcx`, `0x141` (321) `fmtemp1.pcx`,
`0x142` to `0x144` (322 to 324) `f_eco.pcx`, `f_per.pcx` and `f_poli.pcx`, `0x164` (356) `vopts.666`,
`0x165` (357) `vsmith.666`, `0x177` (375) `church.hmp`, `0x18D` (397) `village.hmp`, `0x1B5` (437)
`engmap1.pcx`, `0x1B8` (440) `help.pcx` and `0x1B9` (441) `message.pcx`. The kind of each entry fits
the routine: pictures for `0x00024DB8`, `.666` banks for `0x0005B584` and `.hmp` songs for
`0x0005B0D4`.

## Interpretation

A resource number is the entry's index in the archive directory.

## Alternatives

The match rests on 15 numbers and the extensions they name; the loaders were not traced to the
directory lookup itself.

## How to reproduce

List the `C1086.GOB` directory and look up the indexes the screens push before these calls.
