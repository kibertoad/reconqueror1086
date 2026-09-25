---
id: FND-STRATEGY-036
title: The strategic action hook 0x00021EEC turns conversation variables into map events
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021DFC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021EEC..0x0002229F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021E28
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021E88..0x00021EEA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001074C..0x000109AA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059C24
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059BC7
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00021DFC` registers `0x00021EEC` through `0x00059C24` in the engine callback at `+0xB0`, which is
called only at `0x00059BC7`. Near its end `0x00021EEC` reads conversation variables through
`0x00062828` with the dword at `0x0009A928` and writes them through `0x000628AC`. When variable
`0x1D` is not 0 it writes 1 to `0x1E` and 0 to `0x1D`, runs `0x00021E28("BAR0.RES")` and calls
`0x0005B0D4(0x18C, 1)`; for `0x4B` it writes 1 to `0x4C` and 0 to `0x4B` and runs
`0x00021E28("BAR2.RES")`; for `0x80` it writes 1 to `0x81` and 0 to `0x80`, runs
`0x00021E88("MELEE0.RES")` and calls `0x0005B0D4(0x18B, 1)`. It stores 1 in `0x0009A92C` when
variable `0x2B` is not 0, at `0x0002225C`, and 1 in `0x0009A930` when variable `0x5D` is not 0, at
`0x00022280`, then calls `0x00024CA0` and `0x00059760(1, 1)`.

`0x00021E88(name)` calls `0x0005B2C0`, `0x0005B0D4(0x17C, 1)` and `0x00049478(0)`, runs
`r = 0x0005877C(name, 1, 1, 2)`, reloads the map resources through `0x00049200` and returns `r`.
Its other two callers, at `0x0001082D` and `0x000108D6`, pass `MONEY.RES` and test for 1.

## Interpretation

Variables `0x2B` (43) and `0x5D` (93) start the Scottish and Welsh brigand raids on the next strategic
pass. Variable `0x80` starts a melee in `MELEE0.RES` from the map.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00021DFC`, `0x00021EEC..0x0002229F`, `0x00021E28`, `0x00021E88..0x00021EEA`, `0x0001074C..0x000109AA`, `0x00059C24`, `0x00059BC7` in `CD:CONQUER.EXE`.
