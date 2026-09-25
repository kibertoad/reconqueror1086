---
id: FND-STRATEGY-017
title: The strategic terrain is resource 292, icon.jp, a 200 by 400 grid of dwords
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
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E660..0x0003E6CB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003EE50
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E6CC..0x0003E707
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E708..0x0003E72B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E72C..0x0003E74B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E928
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E1D0..0x0006E317
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E320
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E928
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E980
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The new-game path `0x0003E660` allocates 200 row pointers of 400 dwords through `0x0006E1D0` and
loads resource 292 of `C1086.GOB` through `0x0003EE50`. That entry, `icon.jp`, is 320,016 bytes: the
signed dwords 80, 80, 200 and 400, then 80,000 dwords in column-major order. `0x0003E72C` returns
the low 16 bits of a cell and `0x0003E708` its bits 16 to 23. The exit path `0x0003E6CC` writes the
grid to `temp.jap` through `0x0006E980`, the reload path `0x0003E928` reads it back through
`0x0006E320`, and `0x0006E928` frees the rows.

## Interpretation

The header is the cell width and height and the row and column counts. The low word is the tile,
which also selects the terrain kind (FND-STRATEGY-013), and bits 16 to 23 hold the index of the
person whose cell it is (FND-STRATEGY-003). The top byte is kept but never read by these helpers.
`temp.jap` carries the grid, with any changes, while the strategic screen is left.

## Alternatives

None known.

## How to reproduce

Disassemble the listed ranges of `CD:CONQUER.EXE`, and decode entry 292 of `C1086.GOB`.
