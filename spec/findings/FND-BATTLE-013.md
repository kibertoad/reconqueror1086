---
id: FND-BATTLE-013
title: Units turn one octant at a time toward a target or destination
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002964C..0x000296B3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000296B4..0x0002979F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00027C5E..0x00027CE2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0002964C` takes a source point and a target point. When the x coordinates are equal or the target x
is -1 it returns 1 for a source y smaller than the target y and 5 otherwise. Next, when the y coordinates are
equal or the target y is -1, it returns 3 for a source x smaller than the target x and 7 otherwise. Next, for
a source y greater than the target y it returns 4 for a source x smaller than the target x and 6 otherwise. It
returns 2 when both source coordinates are smaller than the target's, and 0 otherwise. `0x000296B4` takes a
unit and a target unit and computes that octant between their positions. When it equals the unit's
`+0x14` it sets `+0x18` to 0 and `+0x1C` to `0x28`. Otherwise it sets `+0x18` to 0, computes
`u = abs(o - (h + 1)) % 8` and `d = abs(o - (h - 1)) % 8`, and sets `+0x14` to `(h + 1) % 8` when
`u < d` and to `h - 1` otherwise, with -1 becoming 7. State 0 turns toward its destination with the
same arithmetic at `0x00027C5E`.

## Interpretation

With y growing downward, 1 faces down the screen, 3 right, 5 up and 7 left. The distances are plain
differences, so a unit can turn the long way round: from 1 toward 6 it turns through 2 to 5.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0002964C..0x000296B3`, `0x000296B4..0x0002979F`, `0x00027C5E..0x00027CE2`.
