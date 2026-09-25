---
id: FND-STRATEGY-002
title: Routine 0x0003B9F4 issues a brigand order or an order from the king
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003B9F4..0x0003BB49
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00011250..0x0001127F
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003B9F4(kind, a, b, c, d)` handles two kinds. For kind 1 it fills a 32-byte block with
dword `+0x00 = 1`, `+0x04 = a`, `+0x08 = b`, `+0x10 = c` and `+0x1C = 0`, and calls `0x0003B0E4`
with it. For kind 2 it returns 1 at once when the dword at `0x000AA4AC` is not 0, and 0 when
`c == d`. Otherwise it stores 2 at `0x000AA498`, `a` at `0x000AA49C`, `b` at `0x000AA4A0`, `d` at
`0x000AA4A4`, `c` at `0x000AA4A8`, 1 at `0x000AA4AC` and 0 at `0x000AA4B0`, reads the name and two
other strings of the person `0x0004377C(d)` returns through `0x000436E0`, and shows a message box
titled with the string at object 2 `+0x56F4`, which reads as an order from the king. The message
names a month from the pointer table at `0x0009AEAC` indexed by `a`, and the year `b`. Other kinds
return 1.

At `0x00011250`..`0x0001127F` the new-game routine `0x000110E8` stores 1086 in the dwords at
`0x0009AE4C` and `0x0009AE50`, and 0 in those at `0x000AA4AC` and `0x000AA4B0`.

## Interpretation

Kind 1 creates brigand order 0 (FND-STRATEGY-032), with `a` its expiry month, `b` its expiry year
and `c` its origin property. Kind 2 records one pending order from the king: attack property `d`
from London (`c` is 7 at the only call) by month `a` of year `b`. `0x000AA4AC` marks the order
pending, and `0x000AA4B0` is set when it is carried out (FND-STRATEGY-021). The yearly counters
start at 1086, so the first orders come in the first year.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003B9F4..0x0003BB49`, `0x00011250..0x0001127F` in `CD:CONQUER.EXE`.
