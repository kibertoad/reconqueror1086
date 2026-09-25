---
id: FND-ASSAULT-024
title: The hit and death handlers compare the stored target's health with the actor's reduced health before starting the hit effect
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000503EC..0x00050411
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000504C0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000504F2..0x00050517
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E5F0
tool: Ghidra 12.1.3
environment: null
---

## Observation

Handler `0x000503EC` (mode 14) compares the health (field `+0x40`) of the combatant stored in
the actor's field `+0x24` with the actor's own health after the damage just applied, before it
copies state base + 2 and starts that state's effect. Handler `0x000504C0` (mode 15) makes the
same comparison at `0x000504F2`..`0x00050517`. Both modes' table entries in the kind 0, 2 and 4
tables point at transition body `0x0004E77E` (success 11, failure 13). When the actor's state
changes, helper `0x0004E5F0` ends the actor's previous live effect.

## Interpretation

A hit actor whose stored target has no more health than itself goes back to striking (mode 11).
One whose target is stronger goes to mode 13, the health check. The previous effect, such as an
unfinished strike, is ended when the state changes.

## Alternatives

The comparison was once attributed to the mode-11 strike, as a strike being refused. It sits in
the hit and death handlers.

## How to reproduce

Open `0x000503EC` from the handler table; the comparison precedes the state copy through
`0x0004E39C`.
