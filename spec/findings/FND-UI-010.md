---
id: FND-UI-010
title: The HAT backgrounds and the control sprite sets decode to the sizes the layouts expect
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
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

In `C1086.GOB`, every background a registered HAT names decodes as a 640 by 480 PCX of 8 bits a
pixel, except `BATH.PCX` (`FWAR.HAT`) and `CHRCH1.PCX` (`CWAR.HAT`), which the archive lacks. `option.CSF`
holds five frames: two of 23 by 22, two of 24 by 24, and one of 80 by 40, the size of region 11 of
`GAMEOPTS.HAT`. `warplan.CSF` holds 22 frames: fifteen of 40 by 38, the size of regions 0 to 4 of
`FWARPLAN.HAT`, three of 198 by 38 (region 7), two of 114 by 38 (region 5) and two of 138 by 38
(region 6). The dilemma pictures are 99 by 149, inside the 100 by 150 regions 0 to 2 of
`CHARGEN.HAT`. `ica.CSF`, `ics.CSF` and `icw.CSF` each hold 337 frames of 80 by 80.

## Interpretation

The sprite sets supply the control images the layouts place: the five army selectors in three
states each, the join, field and spy controls, and the options screen's switches and its resume
control.

## Alternatives

Which frame each state uses was read from rendered pictures only.

## How to reproduce

Decode the named entries of `C1086.GOB` and compare the sizes with the HAT regions.
