---
id: FND-VIEW-017
title: The colour map generator blends every palette colour towards one entry by i / count and picks the nearest palette colour by RGB distance
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00047914..0x00047A84
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000476C0..0x00047737
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
  - build: BLD-GOG-EN
    file: CD:CONQUER/MONEY.LOW
    offset: 0x00..0x3BBFF
  - build: BLD-GOG-EN
    file: CD:CONQUER/MONEY.RES
    offset: 0x00..0xA47A0
  - build: BLD-GOG-EN
    file: CD:CONQUER/OFFICE.LOW
    offset: 0x00..0x68836
  - build: BLD-GOG-EN
    file: CD:CONQUER/OFFICE.RES
    offset: 0x00..0x10FC2E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R001.LOW
    offset: 0x00..0x63A18
  - build: BLD-GOG-EN
    file: CD:CONQUER/R001.RES
    offset: 0x00..0x122C4B
  - build: BLD-GOG-EN
    file: CD:CONQUER/R011.LOW
    offset: 0x00..0x620A1
  - build: BLD-GOG-EN
    file: CD:CONQUER/R011.RES
    offset: 0x00..0x125552
  - build: BLD-GOG-EN
    file: CD:CONQUER/R022.LOW
    offset: 0x00..0x65556
  - build: BLD-GOG-EN
    file: CD:CONQUER/R022.RES
    offset: 0x00..0x13381E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R030.LOW
    offset: 0x00..0x4F5E9
  - build: BLD-GOG-EN
    file: CD:CONQUER/R030.RES
    offset: 0x00..0xE3216
  - build: BLD-GOG-EN
    file: CD:CONQUER/R040.LOW
    offset: 0x00..0x4FC6E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R040.RES
    offset: 0x00..0xE7527
  - build: BLD-GOG-EN
    file: CD:CONQUER/R050.LOW
    offset: 0x00..0x51867
  - build: BLD-GOG-EN
    file: CD:CONQUER/R050.RES
    offset: 0x00..0xE9D51
  - build: BLD-GOG-EN
    file: CD:CONQUER/R060.LOW
    offset: 0x00..0x423DD
  - build: BLD-GOG-EN
    file: CD:CONQUER/R060.RES
    offset: 0x00..0xB9E09
  - build: BLD-GOG-EN
    file: CD:CONQUER/R070.LOW
    offset: 0x00..0x5582E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R070.RES
    offset: 0x00..0xF0E0B
  - build: BLD-GOG-EN
    file: CD:CONQUER/R080.LOW
    offset: 0x00..0x5595D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R080.RES
    offset: 0x00..0xF78B7
  - build: BLD-GOG-EN
    file: CD:CONQUER/R090.LOW
    offset: 0x00..0x5644E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R090.RES
    offset: 0x00..0xF3E60
  - build: BLD-GOG-EN
    file: CD:CONQUER/R100.LOW
    offset: 0x00..0x589CB
  - build: BLD-GOG-EN
    file: CD:CONQUER/R100.RES
    offset: 0x00..0x101685
  - build: BLD-GOG-EN
    file: CD:CONQUER/R110.LOW
    offset: 0x00..0x568E8
  - build: BLD-GOG-EN
    file: CD:CONQUER/R110.RES
    offset: 0x00..0xFDA9A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R121.LOW
    offset: 0x00..0x4FF29
  - build: BLD-GOG-EN
    file: CD:CONQUER/R121.RES
    offset: 0x00..0xE4E06
  - build: BLD-GOG-EN
    file: CD:CONQUER/R131.LOW
    offset: 0x00..0x58052
  - build: BLD-GOG-EN
    file: CD:CONQUER/R131.RES
    offset: 0x00..0xFFF8C
  - build: BLD-GOG-EN
    file: CD:CONQUER/R141.LOW
    offset: 0x00..0x5C2E9
  - build: BLD-GOG-EN
    file: CD:CONQUER/R141.RES
    offset: 0x00..0x110782
  - build: BLD-GOG-EN
    file: CD:CONQUER/R151.LOW
    offset: 0x00..0x5E97A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R151.RES
    offset: 0x00..0x114E31
  - build: BLD-GOG-EN
    file: CD:CONQUER/R162.LOW
    offset: 0x00..0x56ACD
  - build: BLD-GOG-EN
    file: CD:CONQUER/R162.RES
    offset: 0x00..0xFBC4E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R172.LOW
    offset: 0x00..0x6EC35
  - build: BLD-GOG-EN
    file: CD:CONQUER/R172.RES
    offset: 0x00..0x154E34
  - build: BLD-GOG-EN
    file: CD:CONQUER/R182.LOW
    offset: 0x00..0x5FEF6
  - build: BLD-GOG-EN
    file: CD:CONQUER/R182.RES
    offset: 0x00..0x11E7C6
  - build: BLD-GOG-EN
    file: CD:CONQUER/R192.LOW
    offset: 0x00..0x5EB3E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R192.RES
    offset: 0x00..0x11D9B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/R194.LOW
    offset: 0x00..0x6440A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R194.RES
    offset: 0x00..0x136758
  - build: BLD-GOG-EN
    file: CD:CONQUER/R195.LOW
    offset: 0x00..0x4EE2A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R195.RES
    offset: 0x00..0xE67C9
  - build: BLD-GOG-EN
    file: CD:CONQUER/SKIRMISH.RES
    offset: 0x00..0xAFBF0
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x00047914`..`0x00047A84` builds map `i` of `count` maps. It computes the weight
`(count - i) / count` and one minus that weight in floating point, and for each palette entry `c`
blends each channel as `palette[c] * weight + palette[target] * (1 - weight)`, where `target` is
the fade colour, adds 0.5 and truncates through `0x00063EC0`. Helper `0x000476C0`..`0x00047737`
then scans the 256 palette entries from 0, starting with index 1 and distance `0x2FD`, and
replaces them only when an entry's sum of absolute channel differences is strictly smaller, so
the lowest index wins a tie and index 1 is returned when no entry is nearer than `0x2FD`.
With `SKIRMISH.PAL` from `CD:CONQUER/SKIRMISH.RES` as the palette and the
count and fade colour from each archive's `Scenario` (FND-VIEW-013), maps 0 to 31 it builds
equal `Pal0` to `Pal31` of the same archive byte for byte in all 90 archives, across fade colours
0, 10, 16 and 20.

## Interpretation

The stored first family is a saved copy of what the generator makes, and `SKIRMISH.PAL` is the
palette the scenes are drawn with.

## Alternatives

None known.

## How to reproduce

Open `0x00047914`; the two nested loops over 256 entries hold the blend and the nearest-colour
search. Run the procedure of RULE-VIEW-006 against `Pal0` to `Pal31` of any archive.
