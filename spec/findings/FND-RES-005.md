---
id: FND-RES-005
title: The game can write containers: an entry is compressed with its kind and falls back to kind 0 when compression fails
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000497BC..0x00049AB6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000497AC..0x000497BB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00048090..0x00048147
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004830C..0x00048657
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049124..0x000491D3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000491D4..0x000491FE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00048D18..0x00049123
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004AA99..0x0004AB3C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050887..0x000508E3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050960..0x000509C7
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR0.LOW
    offset: 0x00..0x327D0
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR0.RES
    offset: 0x00..0x8B257
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR1.LOW
    offset: 0x00..0x335A6
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR1.RES
    offset: 0x00..0x8F27D
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR2.LOW
    offset: 0x00..0x2FD4C
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR2.RES
    offset: 0x00..0x84E8D
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND0.LOW
    offset: 0x00..0x476FF
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND0.RES
    offset: 0x00..0xA2F6B
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND1.LOW
    offset: 0x00..0x4D506
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND1.RES
    offset: 0x00..0xB2D05
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND2.LOW
    offset: 0x00..0x4A564
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND2.RES
    offset: 0x00..0xA457E
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE0.LOW
    offset: 0x00..0x500ED
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE0.RES
    offset: 0x00..0xC207A
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE00.LOW
    offset: 0x00..0x50109
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE00.RES
    offset: 0x00..0xC2063
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE01.LOW
    offset: 0x00..0x50254
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE01.RES
    offset: 0x00..0xC21B3
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE02.LOW
    offset: 0x00..0x500ED
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE02.RES
    offset: 0x00..0xC207A
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE03.LOW
    offset: 0x00..0x50321
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE03.RES
    offset: 0x00..0xC2288
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE04.LOW
    offset: 0x00..0x5031B
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE04.RES
    offset: 0x00..0xC2916
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE1.LOW
    offset: 0x00..0x440C3
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE1.RES
    offset: 0x00..0xB2D8E
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE10.LOW
    offset: 0x00..0x440AE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE10.RES
    offset: 0x00..0xB2D8A
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE11.LOW
    offset: 0x00..0x440D5
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE11.RES
    offset: 0x00..0xB2D94
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE12.LOW
    offset: 0x00..0x440CB
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE12.RES
    offset: 0x00..0xB2D8F
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE13.LOW
    offset: 0x00..0x441CE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE13.RES
    offset: 0x00..0xB2EAD
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE14.LOW
    offset: 0x00..0x441B5
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE14.RES
    offset: 0x00..0xB2E87
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE2.LOW
    offset: 0x00..0x38D24
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE2.RES
    offset: 0x00..0x910A4
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE20.LOW
    offset: 0x00..0x38D3F
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE20.RES
    offset: 0x00..0x910AD
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE21.LOW
    offset: 0x00..0x38D52
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE21.RES
    offset: 0x00..0x910B5
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE22.LOW
    offset: 0x00..0x38D33
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE22.RES
    offset: 0x00..0x910B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE23.LOW
    offset: 0x00..0x38F38
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE23.RES
    offset: 0x00..0x912BA
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE24.LOW
    offset: 0x00..0x38F46
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE24.RES
    offset: 0x00..0x912CA
  - build: BLD-GOG-EN
    file: CD:CONQUER/MONEY.LOW
    offset: 0x00..0x3BBFE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MONEY.RES
    offset: 0x00..0xA479F
  - build: BLD-GOG-EN
    file: CD:CONQUER/OFFICE.LOW
    offset: 0x00..0x68835
  - build: BLD-GOG-EN
    file: CD:CONQUER/OFFICE.RES
    offset: 0x00..0x10FC2D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R001.LOW
    offset: 0x00..0x63A17
  - build: BLD-GOG-EN
    file: CD:CONQUER/R001.RES
    offset: 0x00..0x122C4A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R011.LOW
    offset: 0x00..0x620A0
  - build: BLD-GOG-EN
    file: CD:CONQUER/R011.RES
    offset: 0x00..0x125551
  - build: BLD-GOG-EN
    file: CD:CONQUER/R022.LOW
    offset: 0x00..0x65555
  - build: BLD-GOG-EN
    file: CD:CONQUER/R022.RES
    offset: 0x00..0x13381D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R030.LOW
    offset: 0x00..0x4F5E8
  - build: BLD-GOG-EN
    file: CD:CONQUER/R030.RES
    offset: 0x00..0xE3215
  - build: BLD-GOG-EN
    file: CD:CONQUER/R040.LOW
    offset: 0x00..0x4FC6D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R040.RES
    offset: 0x00..0xE7526
  - build: BLD-GOG-EN
    file: CD:CONQUER/R050.LOW
    offset: 0x00..0x51866
  - build: BLD-GOG-EN
    file: CD:CONQUER/R050.RES
    offset: 0x00..0xE9D50
  - build: BLD-GOG-EN
    file: CD:CONQUER/R060.LOW
    offset: 0x00..0x423DC
  - build: BLD-GOG-EN
    file: CD:CONQUER/R060.RES
    offset: 0x00..0xB9E08
  - build: BLD-GOG-EN
    file: CD:CONQUER/R070.LOW
    offset: 0x00..0x5582D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R070.RES
    offset: 0x00..0xF0E0A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R080.LOW
    offset: 0x00..0x5595C
  - build: BLD-GOG-EN
    file: CD:CONQUER/R080.RES
    offset: 0x00..0xF78B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/R090.LOW
    offset: 0x00..0x5644D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R090.RES
    offset: 0x00..0xF3E5F
  - build: BLD-GOG-EN
    file: CD:CONQUER/R100.LOW
    offset: 0x00..0x589CA
  - build: BLD-GOG-EN
    file: CD:CONQUER/R100.RES
    offset: 0x00..0x101684
  - build: BLD-GOG-EN
    file: CD:CONQUER/R110.LOW
    offset: 0x00..0x568E7
  - build: BLD-GOG-EN
    file: CD:CONQUER/R110.RES
    offset: 0x00..0xFDA99
  - build: BLD-GOG-EN
    file: CD:CONQUER/R121.LOW
    offset: 0x00..0x4FF28
  - build: BLD-GOG-EN
    file: CD:CONQUER/R121.RES
    offset: 0x00..0xE4E05
  - build: BLD-GOG-EN
    file: CD:CONQUER/R131.LOW
    offset: 0x00..0x58051
  - build: BLD-GOG-EN
    file: CD:CONQUER/R131.RES
    offset: 0x00..0xFFF8B
  - build: BLD-GOG-EN
    file: CD:CONQUER/R141.LOW
    offset: 0x00..0x5C2E8
  - build: BLD-GOG-EN
    file: CD:CONQUER/R141.RES
    offset: 0x00..0x110781
  - build: BLD-GOG-EN
    file: CD:CONQUER/R151.LOW
    offset: 0x00..0x5E979
  - build: BLD-GOG-EN
    file: CD:CONQUER/R151.RES
    offset: 0x00..0x114E30
  - build: BLD-GOG-EN
    file: CD:CONQUER/R162.LOW
    offset: 0x00..0x56ACC
  - build: BLD-GOG-EN
    file: CD:CONQUER/R162.RES
    offset: 0x00..0xFBC4D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R172.LOW
    offset: 0x00..0x6EC34
  - build: BLD-GOG-EN
    file: CD:CONQUER/R172.RES
    offset: 0x00..0x154E33
  - build: BLD-GOG-EN
    file: CD:CONQUER/R182.LOW
    offset: 0x00..0x5FEF5
  - build: BLD-GOG-EN
    file: CD:CONQUER/R182.RES
    offset: 0x00..0x11E7C5
  - build: BLD-GOG-EN
    file: CD:CONQUER/R192.LOW
    offset: 0x00..0x5EB3D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R192.RES
    offset: 0x00..0x11D9B5
  - build: BLD-GOG-EN
    file: CD:CONQUER/R193.LOW
    offset: 0x00..0x61EAE
  - build: BLD-GOG-EN
    file: CD:CONQUER/R193.RES
    offset: 0x00..0x112ED7
  - build: BLD-GOG-EN
    file: CD:CONQUER/R194.LOW
    offset: 0x00..0x64409
  - build: BLD-GOG-EN
    file: CD:CONQUER/R194.RES
    offset: 0x00..0x136757
  - build: BLD-GOG-EN
    file: CD:CONQUER/R195.LOW
    offset: 0x00..0x4EE29
  - build: BLD-GOG-EN
    file: CD:CONQUER/R195.RES
    offset: 0x00..0xE67C8
  - build: BLD-GOG-EN
    file: CD:CONQUER/SKIRMISH.RES
    offset: 0x00..0xAFBEF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000497BC(name, kind, field24, size, data)` returns 0 when `0x0004957C` already finds `name`.
Otherwise it adds a record at the end of the directory, copies at most 31 characters of `name` and
a NUL, stores `kind`, `field24`, the expanded size `size` and the current data end as the offset,
and seeks there. It dispatches on `kind` through the table at `0x000497AC`. Kind 0 writes `data`
as it is. Kinds 1, 2 and 3 allocate `size + 256` bytes and call `0x00048090`, `0x0004830C` or
`0x00049124` with `data`, the buffer and `size`; when that returns a length it writes that many
bytes, and when it returns 0 it sets the kind to 0 and writes `data` as it is. It stores the length
written as the stored size, adds it to the data end and sets the changed flag. A kind above 3 or a
failed write returns 0.

Kind 3 is read by `0x000491D4`, which calls `0x000488F0`, then `0x00048D18(src, &stored, out,
expanded)` and `0x00048954`. `0x00048D18` reads the input a byte at a time into a bit buffer and runs
a seven-state machine through the table at `0x00048CEC`.

Two writers use the routine. The save routine opens a file with `w+b` through `0x00049200` and adds
`TITLE` (40 bytes) and `VERSION` (4 bytes) with kind 2 and `field24` 0. The scene writer at
`0x00050887` opens a file with `w+b` and adds `Viewer` (108 bytes) and `Scenario` (568 bytes) with
kind 1 and `field24` 0, then further resources. It clears the dword at `0x0009CB7C` first and adds
to it the size of each image it writes.

No shipped entry has kind 3. The scene files' kind-0 entries are the ones compression would not
shrink: 3,292 colour maps, 98 `POV`, 97 `Scenario`, 46 `Backdrop`, 8 textures and the 5 palettes of
`SKIRMISH.RES`.

## Interpretation

Kind 0 is what the writer stores when an encoder reports no gain. Field `+0x24` is a value the
writer's caller supplies; every writer found passes 0, which matches the shipped files. Kind 3 is a
third codec, probably a Huffman or arithmetic coder, that the shipped files do not use.

## Alternatives

What kind 3 encodes was not worked out. Whether the scene writer ever runs in the shipped game, or
was left from the scene editor, was not traced.

## How to reproduce

Disassemble the listed ranges and follow the pushes before each call to `0x000497BC`.
