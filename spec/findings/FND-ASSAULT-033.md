---
id: FND-ASSAULT-033
title: The player's combatant skill is strength plus dexterity plus twice sword experience, and health is strength plus stamina plus honour
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00058851..0x00058887
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x00058851`..`0x00058887` the assault setup writes the player's combatant field `+0x34` as
strength plus dexterity plus two times sword experience, and field `+0x40` as strength plus
stamina plus honour, reading the character's attributes. The ten templates' own `+0x34` and
their armour and health values are set by `0x000542F8`.

## Interpretation

The player's fighting skill comes from strength, dexterity and sword practice, and the player's
health in an assault from strength, stamina and honour.

## Alternatives

None known.

## How to reproduce

Open `0x00058851`; the two sums are stored into the record whose index is at `0x0009D4D0`.
