---
id: FND-TALK-001
title: Conversation nodes are read by length from the .CBF file through a sorted .CIF index
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019D88..0x00019E1A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000163B8..0x000165E5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000165EC..0x00016680
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00016684..0x00016760
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00019CC0` builds `"%s%s"` from its name argument and `.CBF`, then `.CIF`, opens both with `"rb"` and
takes the length of the `.CIF` file through `0x0006A1DC`. `0x000163B8(cbf, cif, cif_length, id)`
calls `0x000165EC`, a binary search over 8-byte records of the `.CIF` file between record 0 and
record `cif_length / 8 - 1`, comparing `id` with the first dword of the record read at each probe
and returning its second dword, or -1. On -1 it prints "ConnodeRetrieveFromBinary(): Can't find
connode index %d" and returns 0. Otherwise it seeks the `.CBF` file to the returned offset and
reads `0x348` bytes; a short read ends the program with exit code 1. `0x00016684` copies from that
header into a new node the byte at `+0x08`, the byte at `+0x48`, the dword at `+0x344`, the five
dwords at `+0x4C`, the five dwords at `+0x330`, the five runs of 30 dwords at `+0x60` (stride
`0x78`) and the 30 dwords at `+0x2B8`. `0x000163B8` then reads the strings: the dword at `+0x00`
gives the length of the first, the dword at `+0x04` the second, the dwords at `+0x0C` up to ten
more and the dwords at `+0x34` up to five more. Each run stops at the first length of 0 or less,
and each string is read as `length + 1` bytes into a buffer of that size. In the GOG archive
`ALL.CIF` is 10,488 bytes, 1,311 records, and `ALL.CBF` 1,514,658 bytes.

## Interpretation

A node file holds records of a fixed `0x348`-byte header followed by its strings, with no count of
strings: the lengths in the header say how many there are. The byte at `+0x08` is the number of
prompt variants and the byte at `+0x48` the number of responses. The index must be sorted by node
number for the binary search to work.

## Alternatives

The dwords at `+0x330` and `+0x344` are copied but no reader of them in the conversation loop was found.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
