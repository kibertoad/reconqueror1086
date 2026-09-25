---
id: FND-UI-003
title: The setup routines bind four callback slots per region and name the screens a click switches to
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B7C0..0x0005B80B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00037360..0x0003762B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000140B0..0x0001420B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014730..0x000148BF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000424A0..0x000425FB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00060610..0x00060769
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00060F10..0x00061037
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041D80..0x00041EA7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000302A0..0x00030493
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00035C60..0x00035F97
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001CBDC..0x0001D1AB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00032EB0..0x000333B3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001F860..0x0001FBCB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00022840..0x00022B67
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003CA40..0x0003CC23
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Each setup routine calls `0x00059C4C(index, slot, routine)` for its regions. The routines in slot 0
of the village exterior draw a label and select cursor frame 5, those in slot 1 select frame 0 and
redraw the place name, and those in slot 2 carry out the action (FND-UI-004). Slot 3 is bound only
on some screens. Of the click routines in slot 2, the following call `0x000596C0(n, ...)` for a
fixed screen `n`, or `0x00059760`:

- screen 0: region 0 to screen 1;
- screen 2: region 0 to screen 3 and region 1 to screen 7; regions 2 to 5 have click routines that
  switch nothing;
- screen 11: region 1 to screen 23, region 3 to screen 13;
- screen 13: region 1 to screen 11; region 2 opens the store (FND-UI-007);
- screen 15: region 3 through `0x00059760`;
- screen 16: regions 0 and 8 to screen 17, 1 to 19, 2 to 21, 3 to 20, 4 to 22, 5 to 18 and 6 to
  23; region 9 has a click routine that switches nothing; region 7 has no routine in any slot;
- screen 18: regions 11 and 12 through `0x00059760`;
- screens 19 to 22: the two regions at `(30, 435)` and `(70, 435)` through `0x00059760`;
- screen 23: region 12 to screen 11 and region 11 to screen 16.

Screen 1 binds slots 0 to 2 for all 13 of its regions, screen 3 for all seven, screen 7 for all six
with one click routine `0x000426B8`, and screen 15 for all five. Screen 18 binds slot 3 on regions
0 to 4 and 8 to 10; screens 19 to 22 bind slot 3 on each row region and on the region at
`(383, 20)`, and screen 20 also on region 16; screen 23 binds slot 3 on regions 10 and 14 and binds no hover routines on regions 0 and
10 and no routines at all on regions 1 to 9. Region 0 of screen 13 starts a conversation
(FND-TALK-007).

## Interpretation

Slot 0 runs when the pointer enters a region, slot 1 when it leaves, and slot 2 on a click. Region 7
of the castle office is shown and hovered over nothing: it carries no routine, so its label is never
shown and clicking it does nothing.

## Alternatives

Slot 3 may be the second button. The routines that switch nothing may change state on the same
screen, or switch screens through a variable.

## How to reproduce

Disassemble each setup routine, collect its `0x00059C4C` calls, and disassemble each slot-2 routine
to its first return.
