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
    address: 0x00048148..0x0004819C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00047EA8..0x0004808E
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR0.LOW
    offset: 0x00..0x327D1
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR0.RES
    offset: 0x00..0x8B258
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR1.LOW
    offset: 0x00..0x335A7
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR1.RES
    offset: 0x00..0x8F27E
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR2.LOW
    offset: 0x00..0x2FD4D
  - build: BLD-GOG-EN
    file: CD:CONQUER/BAR2.RES
    offset: 0x00..0x84E8E
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND0.LOW
    offset: 0x00..0x47700
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND0.RES
    offset: 0x00..0xA2F6C
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND1.LOW
    offset: 0x00..0x4D507
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND1.RES
    offset: 0x00..0xB2D06
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND2.LOW
    offset: 0x00..0x4A565
  - build: BLD-GOG-EN
    file: CD:CONQUER/DEFEND2.RES
    offset: 0x00..0xA457F
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE0.LOW
    offset: 0x00..0x500EE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE0.RES
    offset: 0x00..0xC207B
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE00.LOW
    offset: 0x00..0x5010A
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE00.RES
    offset: 0x00..0xC2064
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE01.LOW
    offset: 0x00..0x50255
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE01.RES
    offset: 0x00..0xC21B4
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE02.LOW
    offset: 0x00..0x500EE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE02.RES
    offset: 0x00..0xC207B
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE03.LOW
    offset: 0x00..0x50322
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE03.RES
    offset: 0x00..0xC2289
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE04.LOW
    offset: 0x00..0x5031C
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE04.RES
    offset: 0x00..0xC2917
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE1.LOW
    offset: 0x00..0x440C4
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE1.RES
    offset: 0x00..0xB2D8F
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE10.LOW
    offset: 0x00..0x440AF
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE10.RES
    offset: 0x00..0xB2D8B
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE11.LOW
    offset: 0x00..0x440D6
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE11.RES
    offset: 0x00..0xB2D95
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE12.LOW
    offset: 0x00..0x440CC
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE12.RES
    offset: 0x00..0xB2D90
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE13.LOW
    offset: 0x00..0x441CF
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE13.RES
    offset: 0x00..0xB2EAE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE14.LOW
    offset: 0x00..0x441B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE14.RES
    offset: 0x00..0xB2E88
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE2.LOW
    offset: 0x00..0x38D25
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE2.RES
    offset: 0x00..0x910A5
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE20.LOW
    offset: 0x00..0x38D40
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE20.RES
    offset: 0x00..0x910AE
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE21.LOW
    offset: 0x00..0x38D53
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE21.RES
    offset: 0x00..0x910B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE22.LOW
    offset: 0x00..0x38D34
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE22.RES
    offset: 0x00..0x910B7
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE23.LOW
    offset: 0x00..0x38F39
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE23.RES
    offset: 0x00..0x912BB
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE24.LOW
    offset: 0x00..0x38F47
  - build: BLD-GOG-EN
    file: CD:CONQUER/MELEE24.RES
    offset: 0x00..0x912CB
  - build: BLD-GOG-EN
    file: CD:CONQUER/MONEY.LOW
    offset: 0x00..0x3BBFF
  - build: BLD-GOG-EN
    file: CD:CONQUER/MONEY.RES
    offset: 0x00..0xA47A0
  - build: BLD-GOG-EN
    file: CD:CONQUER/OFFICE.LOW
    offset: 0x00..0x68836
  - build: BLD-GOG-EN
    file: CD:CONQUER/OFFICE.RES
    offset: 0x00..0x10FC2E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R001.LOW
    offset: 0x00..0x63A18
  - build: BLD-GOG-EN
    file: CD:CONQUER/R001.RES
    offset: 0x00..0x122C4B
  - build: BLD-GOG-EN
    file: CD:CONQUER/R011.LOW
    offset: 0x00..0x620A1
  - build: BLD-GOG-EN
    file: CD:CONQUER/R011.RES
    offset: 0x00..0x125552
  - build: BLD-GOG-EN
    file: CD:CONQUER/R022.LOW
    offset: 0x00..0x65556
  - build: BLD-GOG-EN
    file: CD:CONQUER/R022.RES
    offset: 0x00..0x13381E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R030.LOW
    offset: 0x00..0x4F5E9
  - build: BLD-GOG-EN
    file: CD:CONQUER/R030.RES
    offset: 0x00..0xE3216
  - build: BLD-GOG-EN
    file: CD:CONQUER/R040.LOW
    offset: 0x00..0x4FC6E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R040.RES
    offset: 0x00..0xE7527
  - build: BLD-GOG-EN
    file: CD:CONQUER/R050.LOW
    offset: 0x00..0x51867
  - build: BLD-GOG-EN
    file: CD:CONQUER/R050.RES
    offset: 0x00..0xE9D51
  - build: BLD-GOG-EN
    file: CD:CONQUER/R060.LOW
    offset: 0x00..0x423DD
  - build: BLD-GOG-EN
    file: CD:CONQUER/R060.RES
    offset: 0x00..0xB9E09
  - build: BLD-GOG-EN
    file: CD:CONQUER/R070.LOW
    offset: 0x00..0x5582E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R070.RES
    offset: 0x00..0xF0E0B
  - build: BLD-GOG-EN
    file: CD:CONQUER/R080.LOW
    offset: 0x00..0x5595D
  - build: BLD-GOG-EN
    file: CD:CONQUER/R080.RES
    offset: 0x00..0xF78B7
  - build: BLD-GOG-EN
    file: CD:CONQUER/R090.LOW
    offset: 0x00..0x5644E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R090.RES
    offset: 0x00..0xF3E60
  - build: BLD-GOG-EN
    file: CD:CONQUER/R100.LOW
    offset: 0x00..0x589CB
  - build: BLD-GOG-EN
    file: CD:CONQUER/R100.RES
    offset: 0x00..0x101685
  - build: BLD-GOG-EN
    file: CD:CONQUER/R110.LOW
    offset: 0x00..0x568E8
  - build: BLD-GOG-EN
    file: CD:CONQUER/R110.RES
    offset: 0x00..0xFDA9A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R121.LOW
    offset: 0x00..0x4FF29
  - build: BLD-GOG-EN
    file: CD:CONQUER/R121.RES
    offset: 0x00..0xE4E06
  - build: BLD-GOG-EN
    file: CD:CONQUER/R131.LOW
    offset: 0x00..0x58052
  - build: BLD-GOG-EN
    file: CD:CONQUER/R131.RES
    offset: 0x00..0xFFF8C
  - build: BLD-GOG-EN
    file: CD:CONQUER/R141.LOW
    offset: 0x00..0x5C2E9
  - build: BLD-GOG-EN
    file: CD:CONQUER/R141.RES
    offset: 0x00..0x110782
  - build: BLD-GOG-EN
    file: CD:CONQUER/R151.LOW
    offset: 0x00..0x5E97A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R151.RES
    offset: 0x00..0x114E31
  - build: BLD-GOG-EN
    file: CD:CONQUER/R162.LOW
    offset: 0x00..0x56ACD
  - build: BLD-GOG-EN
    file: CD:CONQUER/R162.RES
    offset: 0x00..0xFBC4E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R172.LOW
    offset: 0x00..0x6EC35
  - build: BLD-GOG-EN
    file: CD:CONQUER/R172.RES
    offset: 0x00..0x154E34
  - build: BLD-GOG-EN
    file: CD:CONQUER/R182.LOW
    offset: 0x00..0x5FEF6
  - build: BLD-GOG-EN
    file: CD:CONQUER/R182.RES
    offset: 0x00..0x11E7C6
  - build: BLD-GOG-EN
    file: CD:CONQUER/R192.LOW
    offset: 0x00..0x5EB3E
  - build: BLD-GOG-EN
    file: CD:CONQUER/R192.RES
    offset: 0x00..0x11D9B6
  - build: BLD-GOG-EN
    file: CD:CONQUER/R193.LOW
    offset: 0x00..0x61EAF
  - build: BLD-GOG-EN
    file: CD:CONQUER/R193.RES
    offset: 0x00..0x112ED8
  - build: BLD-GOG-EN
    file: CD:CONQUER/R194.LOW
    offset: 0x00..0x6440A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R194.RES
    offset: 0x00..0x136758
  - build: BLD-GOG-EN
    file: CD:CONQUER/R195.LOW
    offset: 0x00..0x4EE2A
  - build: BLD-GOG-EN
    file: CD:CONQUER/R195.RES
    offset: 0x00..0xE67C9
  - build: BLD-GOG-EN
    file: CD:CONQUER/SKIRMISH.RES
    offset: 0x00..0xAFBF0
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

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.

Whole-resource exclusive bounds follow FND-RES-070; complete-file identities
and sizes follow BLD-GOG-EN and the listing comparison in FND-RES-069.
