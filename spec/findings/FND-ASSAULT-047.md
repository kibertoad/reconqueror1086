---
id: FND-ASSAULT-047
title: Walking into a block with behaviour bit 0x40 uses it, and in the shipped scenes only the exit or gate cell of each melee has that bit
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00052DE0..0x00052F11
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000580D3..0x000580E0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005825F..0x0005826C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005838A..0x000583A7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00057695..0x000576AA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00051F1E..0x00051F4F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00051E10..0x00051E60
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000536A2
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
tool: Conqueror.Inspect disassembler and scene resource decoder (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00052DE0(x, y, heading)` has two paths. When `x` or `y` is -1 it rotates the offset
`(0, 0x100)` by `heading` through `0x00044740`, adds it to the position of the player's combatant
(`+0x0C`, `+0x10`) and takes the cell of that point; when the block in that cell has behaviour bit
`0x10` it calls `0x00051E60` for the cell and then `0x0004C900`, which writes the block's state
target into the cell. Otherwise it reads the block in cell `(x, y)` and, when that block has
behaviour bit `0x40` (test at `0x00052EC6`), calls the same two routines for the cell and stores 0
in the dwords at `0x000AFA18` and `0x000AFA1C`. A block with neither bit is left alone.

The key dispatch at `0x00057695` calls it with `(-1, -1)` and the dword at `0x000AFA08` shifted
right by 8. The player's movement calls it with a cell and 0 in two places: at `0x000580D3` and
`0x0005825F`, when one of the four cells the move tests has behaviour bit `0x02`, with the first
cell it tested; and at `0x00058397`, when the player's cell has changed, with the new cell.

`0x00051E60` reads the block's `interaction` word and jumps through the 20-entry table at
`0x00051E10`. The entry for 0 is `0x00052DB5`, the same address the range check at `0x00051F26`
jumps to for values above 19, so interaction 0 changes nothing before the cell is replaced.

The only other test of bit `0x40` in object 1 is at `0x000536A2`, where a moving effect record that
has just written its block into a new cell calls the same two routines when that block has the bit.

In the 42 archives of FND-ASSAULT-002, the only placed blocks with behaviour bit `0x40` are block
129 of each `MELEE*` archive, one placement each: labelled `exit` in the 24 archives of the
`MELEE0` and `MELEE1` families and `gate` in the 12 of the `MELEE2` family. All 36 have behaviour
`0x53`, kind 3, interaction 0 and a state target labelled `carpet` (exits) or `grass` (gates), each
with behaviour 1. No `DEFEND*` archive places a block with the bit. Another 40 blocks with behaviour `0x53`,
labelled `exit` or `Exit`, are defined but not placed: 36 in the `MELEE*` archives and 4 in the
`DEFEND*` archives.

## Interpretation

Bit `0x40` makes a block open when the player walks into it. An exit or gate is a see-through
wall that stops movement and, when the player bumps into it, turns into open floor, with no reward.
The action key uses the block one cell ahead when it is actionable, with no distance test.

## Alternatives

What the dwords at `0x000AFA18` and `0x000AFA1C` hold was not traced, and neither was what the
game does when the player then walks through the opened cell. The heading passed by the key
dispatch is assumed to be the player's view heading.

## How to reproduce

Run `tools/Conqueror.Inspect` with `--xref-block-flags=0x40 --executable-only` and
`--xref-code=0x52DE0 --executable-only`, disassemble the listed ranges with `--disassemble`, and read
the fixup for `0x00051E10` with `--fixup-source=0x51E10`. For the census, decode each archive's
`Map` (FMT-VIEW-002) and `Blocks` (FMT-VIEW-001) and group placed blocks by behaviour.
