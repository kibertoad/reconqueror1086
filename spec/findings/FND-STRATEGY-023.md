---
id: FND-STRATEGY-023
title: New-game setup 0x000110E8 places the player at one of seven homes and clears the strategic records
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000110E8..0x0001127F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043670..0x000436DF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00010FF0..0x000110E6
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000110E8` stores 0 in the dword at `0x0009AE5C` and 1 in the one at `0x0009A560`, and calls
`s = 0x00043670(&0x000AB0D4, &0x000AB0D0)`. `0x00043670` stores `0x00024C38(7)` in the dword at
`0x0009C9D0`, takes the person index at `0x0009B8C8 + 4 * sel`, writes that person's words `+0x08`
and `+0x0A` through its two pointers, stores 0 in its byte `+0x07`, sets the dword at `0x000AC180`
and returns the index. The setup stores `s` in `0x000AB0C4`, `0x0004389C(s)` in `0x000AB0CC`, and 12
through `0x000437B0` as person `s`'s byte `+0x0C`.

For each of the six player records it stores 0 in `+0x04`, `+0x08`, `+0x54`, `+0x14` and `+0x00`
and 1 in `+0x0C`, and stores 5 in `0x0009AE6C` and `0x0009AE64`. Record 5 gets 1 in `+0x00`, `+0x04`
and `+0x0C`, 0 in `+0x30`, `+0x18` and `+0x14`, the anchor of the home cell in `+0x3C`, `+0x40` and
as floats in `+0x5C`, `+0x60`, and the home cell in `+0x44`, `+0x48`. Each hostile record gets 0 in
`+0x00` and `+0x6C`, each brigand record 0 in `+0x00`, `+0x18`, `+0x0C` and `+0x6C`, and each brigand
order 0 in `+0x18` and `+0x14`. It stores 0 in `0x000AA4B0` and `0x000AA4AC`, 1086 in `0x0009AE4C`
and `0x0009AE50`, and 5000 in `0x0009AE54`.

A separate reset routine, `0x00010FF0`, stores 0 in the dwords at `0x0009AE5C`, `0x0009AE64`,
`0x0009AE68`, `0x0009AE58`, `0x0009AE60`, `0x0009DC38`, `0x0009DC3C` and `0x0009DC40`, 0 in each hostile record's `+0x00`, frees the hostile
and brigand routes, stores 1086 in `0x0009AE4C` and `0x0009AE50`, 5000 in `0x0009AE54`, 0 in
`0x0009AE84` and `0x0009AE80`, 1 in `0x0009AEEC` at `0x000110D7`, and 5 in `0x0009AE6C`.

## Interpretation

The home is one of seven fixed castles. The person living there becomes the fallback target of the
hostile movements and its group their fallback origin, and loses its assignment. The accumulator
starts full, so the first timed movement is tried on the first pass. `0x0009A560` asks the strategic
pass for the planting notice (FND-STRATEGY-001), and `0x0009AE5C` counts the field records
(FND-STRATEGY-025).

## Alternatives

None known.

## How to reproduce

Disassemble `0x000110E8..0x0001127F`, `0x00043670..0x000436DF`, `0x00010FF0..0x000110E6` in `CD:CONQUER.EXE`.
