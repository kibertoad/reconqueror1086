---
id: FND-STRATEGY-037
title: Two conversation groups set variables 43 and 93 when the player accepts a raid
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The action groups of `ALL.TMI` and `ALL.TMB` hold group 2423, run by an accepted response of
conversation node 2439, which writes 1 to scope-0 variable 43, and group 2552, run by an accepted
response of node 2593, which writes 1 to variable 93.

## Interpretation

Accepting either task in conversation is what raises the raid (FND-STRATEGY-036). What makes the two
nodes reachable is not recorded.

## Alternatives

None known.

## How to reproduce

Decode `ALL.CIF`, `ALL.CBF`, `ALL.TMI` and `ALL.TMB` from `C1086.GOB` with `tools/Conqueror.Inspect` and read nodes 2439 and 2593 and groups 2423 and 2552.
