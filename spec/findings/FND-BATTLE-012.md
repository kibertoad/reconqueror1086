---
id: FND-BATTLE-012
title: The unit pass completes deaths, probes contacts and deals damage by category
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026C81..0x00026D46
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026D4B..0x00026DC5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026DCB..0x000270B4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026FC2..0x0002704C
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The pass visits units in table order. State `+0x1C` `0x50` advances `+0x18` as `(p + 1) % 5` unless it
is 4; on reaching 4 it subtracts 1 from `0x000A9C70` or `0x000A9C94` by lane, sets `+0x20` to 0, and
rewrites `+0x00` `0xF0` to `0x78`. State `0x28` calls `0x00028408` with offsets 0 and 0 and the target
word `+0x30`; for a result of 0 or 1 it sets the state to 0 and `+0x30` to -1, and `+0x0C` and `+0x10`
to -1 when `+0x28` is 1. Otherwise it sets `+0x18` to `(p + 1) % 5`, and when that is 2 it applies
contact. Contact is skipped when `0x000A9C98` is 1 and the target's lane is 0, or it is -1 and the
target's lane is `0x168`. The damage is the remainder of a draw by 10 for the category pairs `0xF0` on
`0x78`, 0 on `0xF0` and `0x78` on 0, and by 7 otherwise; for a target in lane `0x168` it adds the
remainder of `0x000A9CA0` by 3. It subtracts the damage from the target's `+0x20` without a limit.
Skipped or not, it then draws again, takes the remainder by 5, and calls `0x0005B3B0` with the dword at
`0x000A9C80`, the entry of that remainder in `[4, 0xF8C, 0x1E6C, 0x3B8C, 0x5C85]`, 0 and `0x7FFF`. A
target left above 0 and below 20 whose state is not already `0x50` gets state `0x50` and phase 0. When the target is at 0 or below, the
attacker's `+0x30` becomes -1 and its state 0, and its `+0x0C` and `+0x10` become -1 when its `+0x28`
is 1.

## Interpretation

The old notes said the morale was divided by 3; the instruction keeps the remainder. A unit below 20
starts to die. A unit at 20 or more loses at most 11 a hit, so it always passes through the death
state, and hits on a dying unit leave its death running. Each contact plays one of five sounds from one
sample and makes one or two draws.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00026C81..0x00026D46`, `0x00026D4B..0x00026DC5`, `0x00026DCB..0x000270B4`, `0x00026FC2..0x0002704C`.
