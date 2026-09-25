---
id: FND-UI-001
title: Startup registers 25 screens, each a HAT file and a setup routine
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A558..0x0002A749
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059BD4..0x00059C0F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000595C0..0x0005965F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000596C0..0x0005975E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059760..0x000597D7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059C10..0x00059C49
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Startup calls `0x00059BD4(screen, name, setup)` for screens 0 to 24 in order at `0x0002A558` to
`0x0002A749`. `0x00059BD4` allocates 80 bytes at `0x000AFCC0 + 4 * screen`, copies the name into
them and stores the setup routine at `0x000AFB30 + 4 * screen`. The names, in screen order, are
`TITLE.HAT`, `GAMEOPTS.HAT`, `CGOPTS.HAT`, `CHARGEN.HAT`, `FWAR.HAT`, `CWAR.HAT`, `DKING.HAT`,
`PREGEN.HAT`, `TOPTS.HAT`, `TSTANDS.HAT`, `TTENTS.HAT`, `VOPTS.HAT`, `VINN.HAT`, `VSMITH.HAT`,
`VMONYLND.HAT`, `PRACTICE.HAT`, `FOPTS.HAT`, `FOVIEW.HAT`, `FWARPLAN.HAT`, `FCASTLE.HAT`,
`FVILLAGE.HAT`, `FFARM.HAT`, `FFOREST.HAT`, `ICONMAP.HAT` and `FFDIALOG.HAT`. The setup routines are
`0x0005B7C0`, `0x00037360`, `0x000140B0`, `0x00014730`, `0x00035920`, `0x00018B10`, `0x00019A50`,
`0x000424A0`, `0x0005BC80`, `0x0005BFF0`, `0x0005C680`, `0x00060610`, `0x0005FFA0`, `0x00060F10`,
`0x00060570`, `0x00041D80`, `0x000302A0`, `0x00030FD8`, `0x00035C60`, `0x0001CBDC`, `0x00032EB0`,
`0x0001F860`, `0x00022840`, `0x0003CA40` and `0x00021DFC`. The archive also holds `DBATH.HAT` and
`VSTABLES.HAT`, which no registration names.

`0x000595C0(screen, draw, mode)` and `0x000596C0(screen, draw, mode)` load the screen's HAT file
through `0x00059FA0(name, mode)` into the screen record the pointer at `0x000AFE50` points to, call
its setup routine, draw the background through `0x00024DB8(mode, -1, object)` when `draw` is not 0,
and call the routine at `+0xAC` of the screen object; `0x000596C0` first calls the routine at `+0xB4`
of the screen it replaces and frees it. `0x00059760` switches to the screen whose number the record
holds at `+0x04`. A setup routine
stores routines at `+0xAC`, `+0xB0` and `+0xB4` of the new screen object through `0x00059C10`,
`0x00059C24` and `0x00059C38`.

## Interpretation

Each game screen is one HAT layout with a setup routine that binds its regions. The routine at
`+0xAC` runs when the screen is entered and the one at `+0xB4` when it is left.

## Alternatives

When the routine at `+0xB0` runs was not traced; the conversation screen puts its talk there
(FND-TALK-007).

## How to reproduce

Disassemble `0x0002A558` to `0x0002A749`, `0x00059BD4`, `0x000595C0` to `0x000597D7` and `0x00059C10`
to `0x00059C49`, and read the strings the pushed object-2 offsets name.
