---
id: FND-BATTLE-011
title: The neighbour probe tests four corners and stops early after a dead first hit
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00028408..0x000289C9
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00028408` saves the unit's rectangle, replaces it with `(width + 1, height + 1, 1, 1)` of the
field, and tests the points `(left + dx, top + dy)`, `(left + dx, top + h + dy)`,
`(left + w + dx, top + dy)` and `(left + w + dx, top + h + dy)` in that order. On any hit it stores
the zero-based index in the target word it was given. A hit on a unit with `+0x20 > 0` returns 1
for the same lane and 2 for the other. A hit on a dead unit goes on to the next corner. Once a corner
has hit a dead unit, a miss at a later corner returns 0 at once; misses before the first hit go on. It restores the rectangle before every return, and returns 0 after the last
corner.

## Interpretation

It is neither an overlap test nor a nearest-unit search.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00028408..0x000289C9`.
