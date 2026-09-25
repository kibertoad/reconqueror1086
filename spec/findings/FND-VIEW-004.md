---
id: FND-VIEW-004
title: Sine, cosine and rotation use a 64-entry signed 1.15 quarter-wave table and round each product with (p + 0x3FFF) >> 15
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000446BC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004472C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00044740
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000447C4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009C904..0x0009C984
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routines `0x000446BC` (sine) and `0x0004472C` (cosine) index the 64 signed 16-bit values at
`0x0009C904` (object 2 offset `0xC904`) by the low 6 bits of a byte heading, reflecting the
index in the second and fourth quarters and negating the value in the third and fourth. The
values equal `round(sin(i * pi / 126) * 32767)` for `i` from 0 to 63. Cosine is the sine of
`heading + 0x40`. The rotation routines `0x00044740` and `0x000447C4` multiply a coordinate
pair by the sine and cosine of a heading and round each product separately as
`(product + 0x3FFF) >> 15`, giving `(sin * x + cos * y, -cos * x + sin * y)`.

## Interpretation

All of the actor and ray arithmetic uses these integer helpers instead of floating point, so
results are exact integers that depend on this table and rounding.

## Alternatives

None known.

## How to reproduce

Read the table through the fixup in `0x000446BC` that names `0x0009C904`; compare its 64 values
with the formula.
