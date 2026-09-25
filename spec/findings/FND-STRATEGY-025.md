---
id: FND-STRATEGY-025
title: Field records are placed round the home at six fixed offsets and removed with the selection passed on
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00012CAC..0x00012F25
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009A564
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00012CAC(i)` returns 0 when the dword at `0x0009AE5C` is above 5. Otherwise it adds 1 to it, and
for record `i` stores the anchor of the home cell plus the signed pair at `0x0009A564 + 8 * i` in
`+0x3C`, `+0x40` and as floats in `+0x5C`, `+0x60`, 0 in `+0x08` and `+0x54`, the home cell in `+0x44`
and `+0x48`, and 1 in `+0x00` and `+0x0C`. It stores 0 in `+0x18`, `+0x30`, `+0x14` and `+0x04` of
the record `0x0009AE64` names, `i` in `0x0009AE64` and 1 in record `i`'s `+0x04`. The six pairs are
`(-20, -30)`, `(20, -30)`, `(-30, 0)`, `(30, 0)`, `(0, 30)` and `(0, 0)`.

`0x00012DD0(i)` returns 0 when `0x0009AE5C` is 0 or less. When `i` is the selected record it selects
the first other active record from 0 to 5. When `i` is the dword at `0x0009AE6C` it stores 5 there,
clears the selection mark of the chosen record, and gives record 5 `+0x04` and `+0x00` of 1, record
`i`'s position and grid, and `+0x0C` of 1, and selects it. It stores 0 in record `i`'s `+0x54`,
`+0x00`, `+0x04` and `+0x08` and 1 in its `+0x0C`, subtracts 1 from `0x0009AE5C` and stores the
selection in `0x0009AE64`.

## Interpretation

`0x0009AE5C` counts the armies placed on the map. The first test lets six placements succeed, one
for each record including the avatar's, although the avatar is always present. A newly placed army
keeps whatever route and target fields the record held, and it is the previously selected record
whose route is cleared.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00012CAC..0x00012F25`, `0x0009A564` in `CD:CONQUER.EXE`.
