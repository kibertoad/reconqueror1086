---
id: FND-RES-003
title: Kind 1 is a sequence of length-prefixed blocks, each stored or compressed with literals, back-references and runs
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00048148..0x0004819B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00047EA8..0x0004808D
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B1
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
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect) and a container census written for this project
environment: null
---

## Observation

`0x00048148(src, out, stored)` reads a `UINT16LE` length, passes the next `length` bytes to
`0x00047EA8(block, out, length)`, adds the number of bytes that call returns to `out`, and repeats
until it has consumed `stored` bytes. It returns the total.

`0x00047EA8` looks at the first byte of the block. When it is `0x80` it copies the other
`length - 1` bytes to `out` and returns `length - 1`. For any other value it treats the block as
compressed: it never reads byte 1, takes a 16-bit control word from bytes 2 and 3 (byte 2 high),
and reads tokens from byte 4 until its input position reaches `length`. For each token it tests
the top bit of the control word, then shifts the word left; after 16 tokens it reads the next two
bytes as the next control word. A clear bit copies one byte. A set bit reads two bytes `a` and `b`:
the distance is `(a << 4) | (b >> 4)`. A distance other than 0 copies `(b & 0x0F) + 3` bytes, one at
a time, from `distance` bytes before the current output position. A distance of 0 reads two more
bytes `c` and `v` and writes `v` `(b << 8) + c + 16` times. It returns the number of bytes written.
The output position is a 16-bit count from the start of the block's output.

Decoding every kind-1 entry of the 100 containers (FND-RES-001) this way gives each entry's
expanded size exactly. The 22,730 scene and 471 GOB kind-1 entries hold 32,091 blocks: 2,940 with
marker `0x40` and 23 with `0x80` in the GOB, and 29,120 and 8 in the scene files. No block has
another marker. Every block but the last of an entry expands to 16,384 bytes, and a `0x80` block
holds exactly its output. The blocks hold 28,572,490 literals, 22,381,296 copies and 1,432,996
runs; distances reach 4,095 and runs reach 4,095 bytes. No copy reaches back past the start of its
own block, and no compressed block has bytes left after its last token. Byte 1 of the `0x40` blocks
is 0 in 23,222 of them and takes other values, 131 and 130 the most common, in the rest.

The `Pal102` entries of `BAR0.RES` and `BAR0.LOW` have kind 1 and 256 bytes stored and expanded;
each is one `0x40` block that decodes to 256 bytes.

## Interpretation

Kind 1 is an LZ77 variant with a 4,095-byte window and 3 to 18 byte copies, plus runs of 16 to
4,111 bytes, cut into blocks of 16,384 output bytes. The encoder stores a block as `0x80` when
compressing would not shrink it. Byte 1 of a compressed block carries nothing the game uses.

## Alternatives

The executable does not require blocks of 16,384 bytes or copies that stay inside their block;
the shipped files simply never do otherwise.

## How to reproduce

Disassemble the two ranges, then decode every kind-1 entry as described and compare the output
length with the record's expanded size.
