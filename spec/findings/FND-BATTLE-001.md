---
id: FND-BATTLE-001
title: The field battle resolver takes the player's three counts, the foe's three, and two morale values
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000258FC..0x000259C3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00035ABB..0x00035ADC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00035B18..0x00035B48
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003B43C..0x0003B468
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000258FC` takes eight arguments. The first six are addresses of counts; it adds the first three
into `0x000A9C70` and the next three into `0x000A9C94`. It stores the seventh at `0x000A9CA0` and the
eighth at `0x000A9C78`, sets `0x000A9C7C` and `0x000A9CC0` to 1, and sets `0x000A9C8C`,
`0x000A9CA8`, `0x000A9CAC`, `0x000A9CC8`, `0x000A9C74` and `0x000A9C98` to 0. The encounter wrapper
calls it at `0x00035ADC` with the addresses of the army's pools 1, 0 and 2 (as `0x00029E58` numbers
them), then of the hostile record's dwords at `+0x1C`, `+0x20` and `+0x24`, then the morale value and
0. After the call it stores the three hostile counts back at `+0x1C`, `+0x20` and `+0x24`. The brigand
battle calls it at `0x0003B468` with the same order of addresses, pools 1, 0 and 2 then the brigand
record's `+0x1C`, `+0x20` and `+0x24`, and 0 and 0.

## Interpretation

The resolver's three categories are, in order, the army's pool 1, pool 0 and pool 2. Since
`0x00029DE0` stores the pools as swordsmen, halberdiers and knights, the resolver's first category
holds the army's halberdiers (see FND-BATTLE-007).

## Alternatives

The naming of the pools comes from the STRATEGY findings and has not been checked against the
estate screens.

## How to reproduce

Disassemble `0x000258FC..0x000259C3`, `0x00035ABB..0x00035ADC`, `0x00035B18..0x00035B48`, `0x0003B43C..0x0003B468`.
