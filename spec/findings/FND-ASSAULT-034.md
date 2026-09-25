---
id: FND-ASSAULT-034
title: The state copier puts one of an actor's four state blocks into its map cell, keeping its colour selector, and the effect it starts ends in a rethink
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E39C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000502E3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050436
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004EC59
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x0004E39C` copies the actor's base block, or the block at base + 1, + 2 or + 3, into
the actor's live block, replacing the texture base at `+0x2C` but keeping the actor's normalised
colour selector at `+0x08`. The strike handler (at `0x000502E3`), the hit handler (at
`0x00050436`) and the death path (at `0x0004EC59`) call it with + 1, + 2 and + 3 and then start
the copied block's effect, selected by its word `+0x2E`.

## Interpretation

An actor has four looks, walking, attacking, hit and dying, and changes between them by
swapping its block. The attack, hit and death looks are single states held for the length of
their effect, not frame sequences.

## Alternatives

The attack, hit and death textures were once read as animation frames played in order. The
state blocks select one base texture each, and the renderer picks among that state's textures
by viewing angle.

## How to reproduce

Open `0x0004E39C` and its three callers.
