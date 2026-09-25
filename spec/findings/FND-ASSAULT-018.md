---
id: FND-ASSAULT-018
title: Opposite-side acquisition casts a centred ray at each enemy in record order and keeps the strictly nearest one it actually hits
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F98D..0x0004FB38
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CEA0
tool: Ghidra 12.1.3
environment: null
---

## Observation

Test `0x0004F98D` (modes 4, 6 and 10) visits the combatant records in order. It skips a record
that is not live, the actor itself, and any record on the actor's side (field `+0x30`). For
each other record it subtracts the actor's live position (fields `+0x0C` and `+0x10`) from the
candidate's, turns the difference into a heading with `0x000445C4`, and calls raycaster
`0x000470A8` from the actor's position along that heading through the centre column of the
view. It resolves the returned block to a combatant with `0x0004CEA0` and keeps the candidate
only when that combatant is the candidate itself. The best depth starts at `0x7FFF`; a kept
candidate replaces the current choice only when its depth is strictly smaller. The scan stops
as soon as a kept depth is below `0x154`. The test stores the chosen combatant in field `+0x24`
and returns true when it chose one.

## Interpretation

An actor acquires the nearest enemy it can see along a straight line, measured by the
raycaster's depth. An enemy hidden behind another object or actor is not acquired, even when
the object in front is another enemy.

## Alternatives

Earlier readings took visibility as a cell-by-cell line test and did not check the returned
identity. The call to `0x000470A8` and the identity comparison after `0x0004CEA0` rule both
out.

## How to reproduce

Open `0x0004F98D`; the constant `0x7FFF` initialises the best depth and `0x154` is compared
after each accepted hit.
