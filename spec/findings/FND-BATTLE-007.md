---
id: FND-BATTLE-007
title: Category 0 is the halberdiers, 0x78 the swordsmen and 0xF0 the knights
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002814F..0x000283CC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026DCB..0x00026FC2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026D29..0x00026D46
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The keyboard dispatcher appends category 0 for the keys `H` and `h`, `0xF0` for `K` and `k`, and `0x78`
for `S` and `s`. Contact damage uses the larger die for the attacker and target categories `0xF0` on
`0x78`, 0 on `0xF0`, and `0x78` on 0. A knight (`0xF0`) whose death completes is rewritten to `0x78`.

## Interpretation

The keys name the category by its initial, and the larger die follows the usual triangle: halberdiers
beat knights, knights beat swordsmen, swordsmen beat halberdiers. A dismounted knight is counted as a
swordsman. With FND-BATTLE-001 this places the army's halberdiers in category 0, so the wrapper's
order of pools matches the categories.

## Alternatives

The old notes named category 0 the swordsmen and `0x78` the halberdiers.

## How to reproduce

Disassemble `0x0002814F..0x000283CC`, `0x00026DCB..0x00026FC2`, `0x00026D29..0x00026D46`.
