---
id: FND-RES-007
title: The fifteen two-digit MELEE scenes are three families of five that share textures and differ in layout and colour maps
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE0.RES
    offset: 0x00..0xC207A
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE00.RES
    offset: 0x00..0xC2063
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE01.RES
    offset: 0x00..0xC21B3
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE02.RES
    offset: 0x00..0xC207A
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE03.RES
    offset: 0x00..0xC2288
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE04.RES
    offset: 0x00..0xC2916
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE1.RES
    offset: 0x00..0xB2D8E
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE10.RES
    offset: 0x00..0xB2D8A
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE11.RES
    offset: 0x00..0xB2D94
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE12.RES
    offset: 0x00..0xB2D8F
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE13.RES
    offset: 0x00..0xB2EAD
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE14.RES
    offset: 0x00..0xB2E87
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE2.RES
    offset: 0x00..0x910A4
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE20.RES
    offset: 0x00..0x910AD
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE21.RES
    offset: 0x00..0x910B5
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE22.RES
    offset: 0x00..0x910B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE23.RES
    offset: 0x00..0x912BA
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE24.RES
    offset: 0x00..0x912CA
tool: Container census written for this project
environment: null
---

## Observation

`MELEE0.RES`, `MELEE1.RES` and `MELEE2.RES` have 259, 257 and 226 entries. `MELEE00` to `MELEE04`
have the entry names of `MELEE0`, `MELEE10` to `MELEE14` those of `MELEE1`, and `MELEE20` to
`MELEE24` those of `MELEE2`, in the same order. Compared with its unsuffixed scene, entry by entry
after decoding, each member differs in these entries and in no others:

| Scene | Entries that differ from the unsuffixed scene |
|---|---|
| `MELEE00` | `Viewer`, `Map`, `Blocks`, `POV` |
| `MELEE01` | `Viewer`, `Map`, `Blocks`, `POV`, `Pal33` to `Pal127` except `Pal64` |
| `MELEE02` | `Viewer`, `POV` |
| `MELEE03` | `Viewer`, `Map`, `Blocks`, `SFXDEFS`, `POV`, `Pal33` to `Pal127` except `Pal64` |
| `MELEE04` | as `MELEE03`, and `TEX005` |
| `MELEE10`, `MELEE11` | `Viewer`, `Map`, `Blocks`, `SFXDEFS` |
| `MELEE12` | `Viewer`, `Blocks` |
| `MELEE13`, `MELEE14` | `Viewer`, `Map`, `Blocks`, `SFXDEFS`, `Pal96` to `Pal127` |
| `MELEE20`, `MELEE21` | `Viewer`, `Map`, `Blocks`, `SFXDEFS` |
| `MELEE22` | `Viewer`, `Blocks` |
| `MELEE23`, `MELEE24` | `Viewer`, `Map`, `Blocks`, `Pal33` to `Pal127` except `Pal64` |

The `.LOW` files pair the same way.

## Interpretation

Each family reuses one scene's textures and backdrop and changes where the player starts, the map,
the actors and, in some members, the later colour families. The unsuffixed scenes are separate
base scenes.

## Alternatives

None known.

## How to reproduce

Decode each scene's entries and compare them by name with the unsuffixed scene of the family.
