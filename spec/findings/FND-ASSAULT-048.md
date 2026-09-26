---
id: FND-ASSAULT-048
title: The moneylender's scene places the player and four hostile thugs
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER/MONEY.LOW
    offset: 0x00..0x3BBFF
  - build: BLD-GOG-EN
    file: CD:CONQUER/MONEY.RES
    offset: 0x00..0xA47A0
tool: Conqueror.Inspect scene resource decoder (tools/Conqueror.Inspect)
environment: null
---

## Observation

Decoding the `Map` and `Blocks` resources of `CD:CONQUER/MONEY.RES` and `CD:CONQUER/MONEY.LOW` and
applying the actor predicate of FND-ASSAULT-001 gives the same five placed actors in both:

| Block | Label | Template | Combat row | Placements |
|---:|---|---:|---:|---:|
| 114 | `player` | 0 | 9 | 1 |
| 97 | `thug` | 9 | 16 | 1 |
| 101 | `thug` | 8 | 17 | 1 |
| 134 | `thug` | 3 | 17 | 2 |

Blocks 98, 99 and 100 are further `thug` actor blocks that are not placed. No placed block has
behaviour bit `0x40`.

## Interpretation

The fight with Drogo (RULE-ESTATE-003) is the player alone against four hostile combatants, one
from each of templates 9 and 8 and two from template 3. The scene has no exit cell of the kind
FND-ASSAULT-047 describes.

## Alternatives

None known.

## How to reproduce

Decode each archive's `Map` (FMT-VIEW-002) and `Blocks` (FMT-VIEW-001), count map cells whose
block has behaviour bit `0x80` and interaction 1, and group them by block.
