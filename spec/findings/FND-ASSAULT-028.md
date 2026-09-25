---
id: FND-ASSAULT-028
title: The effect scheduler advances an effect only after strictly more than its interval, applies strike damage at the last tick, and rethinks the actor at once
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004C7F4..0x0004C8FC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000530D0..0x00053D5F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005310D..0x0005313C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053365
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053D3F..0x00053D5F
tool: Ghidra 12.1.3
environment: null
---

## Observation

Constructor `0x0004C7F4` stores the current clock value in the new effect's field `+0x10`.
At `0x0005310D`..`0x0005313C`, scheduler `0x000530D0` advances an effect one tick only when
`now - deadline > interval`, and moves the deadline on by the interval, keeping any overrun.
At `0x00053365` it tests whether the effect's tick counter equals its tick count, whether the
owning combatant's mode (field `+0x18`) is 11, and whether the health (field `+0x40`) of the
combatant in its field `+0x24` is above 0; when all three hold it calls damage routine
`0x0004F070`. When an actor's effect ends, `0x00053D3F`..`0x00053D5F` stores -1 in the
combatant's effect field `+0x08` and calls state routine `0x0004F49C` for that combatant.

## Interpretation

A tick of an effect with interval `i` happens at the first scheduler call more than `i`
milliseconds after the previous one was due, so a two-tick effect of 200 ms lasts a little over
400 ms. The strike lands when its effect ends, and only if the actor is still striking and its
target still alive. The actor then decides its next state immediately rather than waiting for
the thinker.

## Alternatives

None known.

## How to reproduce

Open `0x000530D0`. The subtraction and strict comparison at `0x0005310D`, the three-part test
before the call to `0x0004F070` at `0x00053365`, and the store of -1 before the call to
`0x0004F49C` at `0x00053D47` are in that order.
