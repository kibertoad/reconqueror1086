---
id: FND-STRATEGY-015
title: The property and person tables hold 14 records of 15 bytes and 176 records of 18 bytes
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B8EC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009BA50
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00042E4C..0x00043000
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043004..0x00043047
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043164..0x000431CF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000431D0..0x00043247
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000435E8..0x000436DF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000436E0..0x00043940
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The executable holds 14 records of 15 bytes at `0x0009B8EC` and 176 records of 18 bytes at
`0x0009BA50`. The property helpers read and write the byte `+0x00` (`0x00043764`), the words
`+0x01` and `+0x03` (`0x000438C8`) and `+0x05` and `+0x07` (`0x000438F8`), the byte `+0x09`
(`0x0004377C`), the word `+0x0A` (`0x00043164`, `0x000431D0`), the byte `+0x0C` (`0x00043640`,
`0x00043658`), the byte `+0x0D` (`0x000435E8`, `0x00043614`, `0x00043628`) and the byte `+0x0E`
(`0x00042F38`, `0x00042F4C`). The person helpers read the dword `+0x00` (`0x000436E0`), the byte
`+0x04` (`0x0004389C`), the byte `+0x05` (`0x000436E0`, which indexes the string pointers at
`0x0009B9C0` with it), bits 0 and 1 of the byte `+0x06` (`0x000437EC`, `0x00043820`), the byte
`+0x07` (`0x00043868`), the words `+0x08` and `+0x0A` (`0x00043928`), the byte `+0x0C`
(`0x00043794`, `0x000437B0`) and the byte link `+0x0D` (`0x00043558`). The person helpers return 0
for index 0 and for indexes above 176. The dwords at `0x0009B8C0` and `0x0009B8C4` are the heads of
a property list linked through `+0x0A` and a person list linked through `+0x0D`.

`0x00043004(g)` counts the persons 1 to 175 whose flag bit 0 is set, whose byte `+0x04` is `g` and
whose byte `+0x07` is not 0. The save and load routine `0x00042E4C`..`0x00043000` sets both list
heads and every person's `+0x0D` to `0xFF`, and transfers four-byte slices of each person from
`+0x06` and `+0x07` and every whole property record. Every property's `+0x09` is the index of a
person whose name is the property's.

## Interpretation

The property record is a castle on the strategic map with its state, grid cell, route-space
position, lord, garrison and two alert bytes. The person record is a character of the map with
name, group, a county index, flags, assignment, grid cell and rating. The initial numbers are
designer content and are not reproduced here.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0009B8EC`, `0x0009BA50`, `0x00042E4C..0x00043000`, `0x00043004..0x00043047`, `0x00043164..0x000431CF`, `0x000431D0..0x00043247`, `0x000435E8..0x000436DF`, `0x000436E0..0x00043940` in `CD:CONQUER.EXE`.
