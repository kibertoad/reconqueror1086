---
id: FND-TALK-007
title: The conversation partner picks the root node, set from the stands, the inn and the blacksmith
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00021F42..0x00021F56
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C270..0x0005C2AA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00060444..0x000604A9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00061174..0x00061189
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A738..0x0002A742
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005FFC7..0x000600CF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00021EEC` calls `0x00019CC0(root, "all", 0)` with `root` the dword at `0x0009A968 + 4 *` the
dword at `0x000A9B20`. `0x0005C270` stores the dword at `0x0009DC0C + 4 * r` in `0x000A9B20`, `r`
being the result of `0x00059D50`, and calls `0x000596C0(0x18, 0, 1)`. `0x00060444`, which
`0x0005FFA0` registers for the regions of screen 12 (`VINN.HAT`), stores the dword at `0x0009DEAC +
4 * r` in `0x000A9B20`; only when the dword at `0x0009DED4` is not 0 or the partner is 21 does it
store 1 in the byte at `0x0009DEA8` and call `0x000596C0(0x18, 0, 1)`. `0x00061174` stores 7 in
`0x000A9B20` and calls `0x000596C0(0x18, 0, 1)`. `0x0002A738` registers screen `0x18` with
`FFDIALOG.HAT` and `0x00021DFC`, which puts `0x00021EEC` in the callback at `+0xB0`.

## Interpretation

The partner is an index into a 29-entry table of root nodes. Clicking a lady in the stands, an
inn patron or the blacksmith sets it and opens the dialogue screen, whose hook runs the conversation.
Inn patrons talk only while `0x0009DED4` is set, apart from partner 21.

## Alternatives

Other writers of `0x000A9B20`, at `0x00060D85` and `0x00060DE5`, were not traced.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
