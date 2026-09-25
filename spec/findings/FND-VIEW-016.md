---
id: FND-VIEW-016
title: Each scene archive has 128 colour maps Pal0 to Pal127 of 256 bytes, in four families of 32 that fade from identity
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND0.LOW
    offset: 0x00..0x47700
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND0.RES
    offset: 0x00..0xA2F6C
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND1.LOW
    offset: 0x00..0x4D507
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND1.RES
    offset: 0x00..0xB2D06
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND2.LOW
    offset: 0x00..0x4A565
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND2.RES
    offset: 0x00..0xA457F
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE0.LOW
    offset: 0x00..0x500EE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE0.RES
    offset: 0x00..0xC207B
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE00.LOW
    offset: 0x00..0x5010A
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE00.RES
    offset: 0x00..0xC2064
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE01.LOW
    offset: 0x00..0x50255
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE01.RES
    offset: 0x00..0xC21B4
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE02.LOW
    offset: 0x00..0x500EE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE02.RES
    offset: 0x00..0xC207B
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE03.LOW
    offset: 0x00..0x50322
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE03.RES
    offset: 0x00..0xC2289
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE04.LOW
    offset: 0x00..0x5031C
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE04.RES
    offset: 0x00..0xC2917
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE1.LOW
    offset: 0x00..0x440C4
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE1.RES
    offset: 0x00..0xB2D8F
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE10.LOW
    offset: 0x00..0x440AF
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE10.RES
    offset: 0x00..0xB2D8B
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE11.LOW
    offset: 0x00..0x440D6
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE11.RES
    offset: 0x00..0xB2D95
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE12.LOW
    offset: 0x00..0x440CC
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE12.RES
    offset: 0x00..0xB2D90
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE13.LOW
    offset: 0x00..0x441CF
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE13.RES
    offset: 0x00..0xB2EAE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE14.LOW
    offset: 0x00..0x441B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE14.RES
    offset: 0x00..0xB2E88
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE2.LOW
    offset: 0x00..0x38D25
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE2.RES
    offset: 0x00..0x910A5
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE20.LOW
    offset: 0x00..0x38D40
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE20.RES
    offset: 0x00..0x910AE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE21.LOW
    offset: 0x00..0x38D53
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE21.RES
    offset: 0x00..0x910B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE22.LOW
    offset: 0x00..0x38D34
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE22.RES
    offset: 0x00..0x910B7
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE23.LOW
    offset: 0x00..0x38F39
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE23.RES
    offset: 0x00..0x912BB
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE24.LOW
    offset: 0x00..0x38F47
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE24.RES
    offset: 0x00..0x912CB
tool: Scene resource decoder written for this project
environment: null
---

## Observation

Each of the 42 `MELEE*` and `DEFEND*` archives has exactly one resource named `Pal0` to `Pal127`
for every number, each 256 bytes. Every byte is a palette index. `Pal0`, `Pal32`, `Pal64` and
`Pal96` map most indices to themselves, and the maps that follow each of them move steadily
more colours towards a few indices until the 32nd map of the family.

## Interpretation

A colour map replaces each palette index of a texture pixel by a darker or tinted one. The
maps form four families of 32, one family for each colour group a block can name (FMT-VIEW-001
`color_family`), and within a family a higher number is further away.

## Alternatives

None known.

## How to reproduce

Decode `Pal0` to `Pal127` from `CD:CONQUER/MELEE0.RES` and compare each map with the identity.
