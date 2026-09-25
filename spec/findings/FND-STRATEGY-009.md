---
id: FND-STRATEGY-009
title: Mode 1 at 0x00038D78 steps straight toward the destination at 0.9 times the terrain speed
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038D78..0x0003908B
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00038D78(s, &done)` stores 0 in `done`, and compares `+0x3C` and `+0x40` with the truncation of
`+0x5C + +0x64` and `+0x60 + +0x68`. When a difference is above 0 while its direction float is below
0, or below 0 while the direction is above 0, it stores the truncated position in `+0x3C` and
`+0x40`, stores `0xFFFF` in `done` and returns. Otherwise it projects `(INT32(x) + INT32(dx),
INT32(y) + INT32(dy))` through `0x000629B0` into `+0x44` and `+0x48` and looks up the tile's kind
in the bytes at `0x0009AF78`. For kind 9 it stores the truncated position in `+0x3C` and `+0x40`,
0 in `+0x00`, and the origin's garrison byte plus the three troop counts through `0x00043658`, and
stores `0xFFFF` in `done`. Otherwise it reads the float `w` of that kind from the table the dword
at `0x0009B704 + 4 * profile` points to, with `profile` the dword at `0x0009B610`, adds
`dx * 0.9 * w * k` and `dy * 0.9 * w * k` to `+0x5C` and `+0x60` with `k` the dword at
`0x0009AEEC` and the double at `0x00094F97` (0.9), and projects the truncated new position into
`+0x44` and `+0x48`. Neither branch changes the live-record count at `0x0009AE60`.

## Interpretation

The direction stays normalized, so the terrain cell tested is almost always the current one. A
record that meets impassable terrain (kind 9) returns its troops to its origin and disappears; the
garrison byte wraps at 256.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00038D78..0x0003908B` in `CD:CONQUER.EXE`.
