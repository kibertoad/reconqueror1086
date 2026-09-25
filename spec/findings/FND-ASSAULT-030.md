---
id: FND-ASSAULT-030
title: When an actor changes cell, the scheduler puts back the block it covered and installs itself in the new cell before the next effect runs
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053624..0x00053694
tool: Ghidra 12.1.3
environment: null
---

## Observation

When a movement tick changes an actor's cell, `0x00053624`..`0x00053694` writes the block saved
in field `+0x28` of the moving record back to the old map cell, reads the new cell's block into
`+0x28`, copies it to the state target `+0x40` of the actor's live block, and writes the actor's
block (the record's field `+0x24`) into the new cell. This happens before the scheduler moves on
to the next effect.

## Interpretation

An actor occupies exactly one map cell, and because its block has behaviour bit `0x02`
(FND-ASSAULT-003) other movers cannot enter that cell. Several actors sent to the same cell do
not share it or spread to neighbouring cells: the first to arrive holds it and the others stop
against it and retry.

## Alternatives

None known.

## How to reproduce

Open the scheduler at `0x00053624`; the four map and record writes follow the cell change.
