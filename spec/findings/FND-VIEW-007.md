---
id: FND-VIEW-007
title: The traversal walks at most 64 wrapped map steps, probing cells in a fixed order and following state targets of pass-through blocks
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00045158
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004527D..0x0004566D
tool: Ghidra 12.1.3
environment: null
---

## Observation

Traversal `0x00045158` normalises the ray so that its larger component becomes a signed `0x100`
and the smaller is scaled by signed division, then adds that step to the origin up to `0x40`
times. It takes each cell as `(position >> 8) & 0x7F`, so the map wraps. At
`0x0004527D`..`0x0004566D`, when both cell coordinates change it probes `(new_x, old_y)`, then
`(old_x, new_y)`, then `(new_x, new_y)`; when one changes it probes the entered cell and then
its two neighbours across the other axis, the lower one first. For each probed cell it passes
the cell's block to the candidate helper (FND-VIEW-008). A block whose behaviour bit 0 is clear
stops the walk once it has produced a candidate. For a block with behaviour bit 0 set that is
of kind 4 or has behaviour bit 3, it continues with the block named by the state target
`+0x40`, in the same cell, while that block's kind is not 0.

## Interpretation

A ray can see up to 64 cells, through blocks marked as see-through (behaviour bit 0), and
through the open state of doors and the like.

## Alternatives

None known.

## How to reproduce

Open `0x00045158`; the loop counter compared with `0x40` and the masks with `0x7F` follow the
normalisation.
