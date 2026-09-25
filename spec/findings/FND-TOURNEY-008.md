---
id: FND-TOURNEY-008
title: Strings for the tournament refusals and the joust opponents' portraits
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00095E30..0x00095E77
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00095F98..0x0009608C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00098AA0..0x00098BD4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00098C30
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The executable holds the portrait names `jsimonle.pcc`, `jrichard.pcc`, `jgerard.pcc` and
`jgilbert.pcc` at `0x00095E30`, `0x00095E40`, `0x00095E60` and `0x00095E6C`. It holds a joust
refusal for a player with no money at `0x00095FA4`, the parts of the joust offer at `0x0009601C` to
`0x0009608C`, the message that everyone is too tired to joust at `0x00098AA0`, a melee refusal for
a player with no money at `0x00098AE4`, the parts of the melee offer at `0x00098B60` to `0x00098BD4`,
and the message that everyone is too tired to melee at `0x00098C30`.

## Interpretation

The refusals and offers are the ones FND-TOURNEY-004 and FND-TOURNEY-005 show. Which code loads
the four portraits was not traced.

## Alternatives

None known.

## How to reproduce

Read the strings at the listed addresses.
