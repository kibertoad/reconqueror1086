---
id: FND-BATTLE-016
title: The keyboard dispatcher selects by category, pauses, exits, and has two test switches
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00027ED2..0x000283CC
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Values `0x48`/`0x68`, `0x4B`/`0x6B` and `0x53`/`0x73` append every living lane-0 unit of category 0,
`0xF0` and `0x78` that is not yet selected, in table order. `0x50`/`0x70` toggle `0x000A9C7C` by XOR
with 1 when `0x000A9C8C` is not 0. `0x12D` and `0x16B` set `0x0009ADC0` to 1 and return 1. `0x61`
writes `+0x28 = 1` to the selected units and `0x41` to all living units. `0x57`, `0x17` and `0x111`, when
the bytes `0x000B0C36`, `0x000B0C2A` and `0x000B0C38` are all nonzero, set every living foe's `+0x20`
to 20 and `0x000A9C98` to 1; `0x4C`, `0x0C` and `0x126` under the same test set `0x000A9C98` to -1.
The values come from `0x00065388` after `0x00065380` returns nonzero; when it returns 0 the dispatcher
reads nothing. Other values do nothing.

## Interpretation

The values follow the BIOS keyboard codes, with 0x100 plus the scan code for an Alt key: `0x12D` is
Alt+X, `0x16B` Alt+F4, `0x111` Alt+W and `0x126` Alt+L, and `0x17` and `0x0C` are Ctrl+W and Ctrl+L.
The three bytes are likely shift states held down, which would make W a way to win (the foe drops to
20 and the player cannot be hit) and L a way to lose (the foe cannot be hit).

## Alternatives

What the three bytes hold was not traced.

## How to reproduce

Disassemble `0x00027ED2..0x000283CC`.
