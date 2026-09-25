---
id: FND-PERSON-009
title: The tournament melee turns COLOR 3, 0 and 5 into colour index 0, 1 and 2 at 0x0009D498
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000587A9..0x000587E9
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0005877C` reads field 19 (COLOR) of row 0 and stores 0 in the dword at `0x0009D498` when it is
3, 1 when it is 0 and 2 when it is 5; any other value leaves the dword unchanged.

## Interpretation

The first-person code numbers the player's colour 0 to 2 in `0x0009D498`, in the order of shields
4, 3 and 5 of the options screen.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000587A9` to `0x000587E9`.
