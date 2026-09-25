---
id: FND-STRATEGY-014
title: The month selects one of four terrain profiles through the table at 0x0009B720
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003866C..0x00038683
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E894..0x0003E8FF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E900..0x0003E927
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E7E4..0x0003E881
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003CCFF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003CDC1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B720
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B750
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B760
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003866C` returns the zero-based month. `0x0003E894` stores the dword at `0x0009B720 + 4 * m`
for that month in `0x0009B610` and loads the atlas the string pointer at `0x0009B760 + 4 * p`
names. The twelve dwords give the profile of each month. The four atlas pointers name `ics.csf`,
`ica.csf`, `icw.csf` and `ics.csf`, and the movie pointers at `0x0009B750` name `tran4.smk`,
`tran2.smk`, `tran3.smk` and `tran1.smk`. The map setup at `0x0003CCFF` calls `0x0003E894`, and the
map dispatcher at `0x0003CDC1` calls `0x0003E900`, which compares the month's profile with
`0x0009B610` at `0x0003E901`..`0x0003E925` and, when they differ, calls `0x0003E894` and then
`0x0003E7E4`. `0x0003E7E4` plays the profile's movie at
`(100, 100)` when the dword at `0x0009ADB8` is 1.

## Interpretation

The profiles are summer (0), autumn (1), winter (2) and spring (3), named by their movies and
atlases. Spring and summer share the atlas and the terrain speeds but have their own movies.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003866C..0x00038683`, `0x0003E894..0x0003E8FF`, `0x0003E900..0x0003E927`, `0x0003E7E4..0x0003E881`, `0x0003CCFF`, `0x0003CDC1`, `0x0009B720`, `0x0009B750`, `0x0009B760` in `CD:CONQUER.EXE`.
