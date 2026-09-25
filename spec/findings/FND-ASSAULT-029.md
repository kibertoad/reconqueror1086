---
id: FND-ASSAULT-029
title: A movement tick rotates the step by the heading, tests the next cell only outside the band -0x59 to 0x59, and changes cell only beyond 0x80
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053395..0x00053D5F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053425..0x000535E5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053C13..0x00053C6C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053CC9..0x00053CDD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00044740
tool: Ghidra 12.1.3
environment: null
---

## Observation

For a moving effect, the scheduler rotates the descriptor's per-tick step (for the shipped
actors `(64, 0)`) by the actor's heading through `0x00044740` and adds each component to the
moving block's signed 8.8 offset (block `+0x24` for x and `+0x28` for y). For each axis, with
new offset `n`, `0x00053425`..`0x000535E5` reads the neighbouring map cell in that axis's
direction only when `n < -0x59` or `n > 0x59`, and rejects the step when that block has
behaviour bit `0x02`. An accepted `n` below `-0x80` or above `0x80` moves the actor to the next
cell and adds or subtracts `0x100`; `n` equal to `-0x80` or `0x80` stays in the current cell.

On a rejected step the effect's live flags decide. With flag `0x10`,
`0x00053CC9`..`0x00053CDD` zeroes the effect's steps and tick count, leaving the block's offset
and the actor's heading as they were, and the effect ends so that `0x00053D3F` rethinks the
actor. With flag `0x40`, `0x00053C13`..`0x00053C6C` sets the rejected axis's offset to 0 and
sets the heading to `(((heading + 0x20) & 0xC0) - 0x40) & 0xFF`, and the effect continues.

## Interpretation

From offset 0, the shipped step gives offsets 64, 128 and then 192, which crosses into the next
cell as -64, so the first cell change happens on the third tick (600 ms) and each later one
takes four ticks (800 ms). A direct mover stops where it is when blocked and tries again after
aiming anew; a wandering mover turns left to the next cardinal and keeps going.

## Alternatives

A single cell time of 600 ms was once assumed; the carried offset rules it out after the first
cell.

## How to reproduce

Open the scheduler at `0x00053395` and follow the per-axis branches. The two comparisons with
`0x59` and `0x80` and the test of behaviour bit 2 are in `0x00053425`..`0x000535E5`.
