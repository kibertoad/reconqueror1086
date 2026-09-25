---
id: FND-ASSAULT-019
title: Same-side acquisition casts a centred ray at each friend in record order and keeps the strictly nearest one it hits, stopping below 0x200
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004FC34..0x0004FD7A
tool: Ghidra 12.1.3
environment: null
---

## Observation

Test `0x0004FC34` (modes 3, 5 and 9) visits the combatant records in order, skips records that
are not live, the actor itself and records on the other side, and otherwise works as
`0x0004F98D` does (FND-ASSAULT-018): heading through `0x000445C4`, centred ray through
`0x000470A8`, identity through `0x0004CEA0`, best depth starting at `0x7FFF` and replaced only
by a strictly smaller depth. It stops as soon as a kept depth is below `0x200`. It stores the
chosen combatant in field `+0x24` and returns true when it chose one.

## Interpretation

An actor regrouping looks for the nearest friend it can see.

## Alternatives

None known.

## How to reproduce

Open `0x0004FC34`; its structure matches `0x0004F98D` with the side comparison reversed and
`0x200` as the early exit.
