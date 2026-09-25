---
id: FND-STRATEGY-007
title: The movement constructor 0x0003AC5C builds a direct or routed movement from a property
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003AC5C..0x0003AFB1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004A070..0x0004A2FC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043928..0x0004393B
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003AC5C(t, o, m, f)` returns 0 when the byte at `0x0009B8F9 + 15 * o` is 0, when no hostile
record is free, or when `m` is not 1 or 2. For `m == 1` it stores `o` in `+0x28`, the lord in
`+0x2C`, 1 in `+0x00` and `+0x34`, the marker word of `o` shifted left 3 in `+0x38`, the anchor of
person `t`'s words `+0x08` and `+0x0A` (read through `0x00043928`) in `+0x3C` and `+0x40`, the
anchor of property `o`'s grid as floats in `+0x5C` and `+0x60`, the property's grid in `+0x44` and
`+0x48`, and the destination minus the truncated position divided by the floating square root of
its squared length in `+0x64` and `+0x68`. For `m == 2` it calls `0x0004A070(record, o, t, f)`,
returns 0 when that does not return 1, and then stores the lord, the marker frame, `m` in `+0x34`,
`o` in `+0x28`, 1 in `+0x00`, the anchored property grid as floats in `+0x5C` and `+0x60` and the
grid in `+0x44` and `+0x48`. Both call `0x00038A8C` with the record index, add 1 to `0x0009AE60`
and return 1.

`0x0004A070(r, o, t, f)`: for `f == 1` it takes `g = 0x0004389C(t)` and the byte at
`0x0009C9D4 + 14 * o + g`. When that byte is 0 or `g` is outside 0 to 13 it stores 0 in `+0x6C` and
1 in `+0x0C` and returns 0. For byte 1 it builds `rt_<o + 1>_<g + 1>.rat` and stores 0 in `+0x10`;
otherwise `rt_<g + 1>_<o + 1>.rat` and 1 in `+0x10`. For `f == 0` it builds the name from the
string pointer at `0x0009CA98 + 4 * s`, with `s` the dword at `0x0009C9D0`, and stores 0 in
`+0x10`. It loads the named resource, copies `count` pairs of dwords from after its first dword
into a new buffer at `+0x6C`, and when `+0x10` is 1 copies them again in reverse pair order into a
second buffer that replaces the first. It stores 0 in `+0x30` and `+0x0C` and the count in `+0x18`,
and returns 1.

## Interpretation

Mode 1 walks straight from the property to the target person's cell; mode 2 follows a route file.
Flag 1 picks a property-to-property route by the target person's group, reversing the file when
the pair is stored the other way round; flag 0 picks the starting-home route chosen at the start of
the game (FND-STRATEGY-023). `+0x10` records that the route was reversed. The direct mode has no
guard for a zero length either.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003AC5C..0x0003AFB1`, `0x0004A070..0x0004A2FC`, `0x00043928..0x0004393B` in `CD:CONQUER.EXE`.
