---
id: FND-STRATEGY-028
title: The terrain renderer 0x0003C770 draws the low word of each visible cell in scan order
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003C770
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E4B0
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003C770` draws the view from `(20, 7)` to `(403, 441)`. It starts at y -53 and steps by 20;
each line starts x at -20 or 20 by the parity of its wrapped column and steps rows by 80, wrapping
rows at 200 and columns at 400. For each position it draws the tile the cell's low 16 bits name from
the season's atlas, and keeps tiles that are only partly inside the view for clipping.

## Interpretation

The order matches the projection's scan (FND-STRATEGY-018).

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003C770`, `0x0006E4B0` in `CD:CONQUER.EXE`.
