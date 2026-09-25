---
id: FND-ASSAULT-042
title: Scene loading reads 0x40-byte effect descriptors from SFXDEFS, and the constructor and scheduler use their fields
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050AAC..0x00050ACC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004C7F4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000531B0..0x0005335A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000536DE..0x00053C68
tool: Ghidra 12.1.3
environment: null
---

## Observation

Scene loading at `0x00050AAC`..`0x00050ACC` takes the dword at `Scenario` offset `0x1C` as a
count and reads `count * 0x40` bytes from the archive entry named `SFXDEFS`. Constructor
`0x0004C7F4` copies from a descriptor: `+0x0C` as the tick count, `+0x14` as the interval,
`+0x18` as the scheduler flags, `+0x1C` and `+0x20` as per-tick map coordinate steps, `+0x28`
as -1 or a scene block selector, `+0x2C` as a per-tick block index step, `+0x30` as the block
to loop back to, `+0x34` as a per-tick surface index step, `+0x38` as the last surface, and
`+0x3C` as a per-tick heading step. The scheduler's writes at `0x000531B0`..`0x0005335A` and
`0x000536DE`..`0x00053C68` use each of those fields as named. The constructor keeps at most 64
live effect records of `0x40` bytes.

## Interpretation

Every animation and movement in a scene is an effect made from one of the scene's descriptors.
Its duration is its tick count times its interval, plus the scheduler's strict comparison
(FND-ASSAULT-028).

## Alternatives

None known.

## How to reproduce

Open `0x00050AAC`; the read of `Scenario + 0x1C` and the multiplication by `0x40` precede the
read of `SFXDEFS`. Compare the constructor's copies with the scheduler's uses.
