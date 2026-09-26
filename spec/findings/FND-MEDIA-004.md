---
id: FND-MEDIA-004
title: Text is drawn with CONFONT.CSF: each byte selects its frame, drawn in the caller colour, and moves x by the frame width
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000644D4..0x00064523
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00064524..0x000645AB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E0D0..0x0006E0E1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006E0B0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A756..0x0002A765
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x21A0F58..0x21A2E65
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B0CFA6..0x1B0EFA7
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000644D4(x, y, colour, text)` walks the NUL-terminated `text`. For each byte `b` it takes frame
`b` from the frame array at `+0x0C` of the record in `0x0009E03C` (the loaded `CONFONT.CSF`,
FND-MEDIA-001), draws it through `0x0007CEB0(x, y, colour, frame)`, and adds to `x` the value of
`0x0006E0D0(b, font)`, the signed `UINT16` width at the start of that frame. `0x000644D4` has 338
direct callers; among the colours they pass are `0xF7`, `0xF9` and `0xB2`. `0x00064524` is a
variant that also calls `0x0006E0B0` and `0x000633D0` for each byte.

`CONFONT.CSF` is a kind-1 entry of `C1086.GOB` with 256 frames, one per byte value, all 13 rows high
and 2 to 10 wide. `font.CSF` is another 256-frame file, every frame 11x13.

## Interpretation

`CONFONT.CSF` is the game's proportional text face. A glyph's own pixel values do not matter, only
which pixels are covered; the text colour is a palette index chosen by each caller. There is no
kerning and no line breaking in the routine.

## Alternatives

`font.CSF` may be an older fixed-width face; no load of it was found.

## How to reproduce

Disassemble `0x000644D4`, `0x00064524`, `0x0006E0D0` and `0x0002A756` in `CD:CONQUER.EXE`; decode
`CONFONT.CSF` from `C1086.GOB`.
