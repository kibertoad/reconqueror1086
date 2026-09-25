---
id: FND-ASSAULT-046
title: A combatant's armour is the dword at offset 0x3C, read beside its health at 0x40
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004EA04
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CE74
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x0004EA04` reads the defender combatant's dword at offset `0x3C` and its health at
offset `0x40`. It passes the dword at `0x3C` to `0x0004CE74`, which subtracts
`max(value - penetration, 0)` from the rolled damage and raises a negative result to 0, and then
subtracts the result from the health.

## Interpretation

Offset `0x3C` is the combatant's armour: the value the combat row's penetration is taken from
before the remainder reduces the damage.

## Alternatives

None known.

## How to reproduce

Open `0x0004EA04` and follow the loads relative to the combatant pointer: the dword at `+0x3C`
is the argument of the call to `0x0004CE74`, and the dword at `+0x40` receives the subtraction.
