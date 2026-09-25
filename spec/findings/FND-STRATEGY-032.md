---
id: FND-STRATEGY-032
title: Routine 0x0003B0E4 with a descriptor creates a brigand force on one of three route families
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003B0E4..0x0003B29F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003B97C..0x0003B9F0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049ED0..0x0004A06C
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003B0E4(d)` with `d` not null reads the slot `s` from the descriptor's `+0x1C`. When the byte
block at `0x000AA0F0 + 32 * s` has `+0x14` equal to 1 it returns 0. Otherwise it stores `s` in its
`+0x1C` and copies the descriptor's `+0x00`, `+0x04`, `+0x08` and `+0x10` into it, and calls
`0x00049ED0(&brigand, origin, s)` for the record at `0x000AA150 + 0x118 * s`. When that does not
return 1 it returns 0. It stores the lord of the origin in `+0x2C`, 2 in `+0x34`, 1 in `+0x00`, the
origin in `+0x28`, `0x00024C38(1)` in `+0x1C`, `0x00024C38(1) + 1` in `+0x20` and 0 in `+0x24`, and
stores 1 in the block's `+0x14` and 0 in its `+0x18`. For `s == 0` it shows an order from the
player's lord, worded one way when the block's year is 1086, with the month from `0x0009AEAC` and
the year. It returns 1.

`0x00049ED0(r, o, s)` builds `br_<o + 1><c>.rat` with `c` the letter `b` when `0x00024C38(2)` is 0
and `a` otherwise for `s == 0`, and names `scot.rat` for `s == 1` and `wales.rat` for `s == 2`. It
loads the pairs into a buffer at `+0x6C`, stores 0 in `+0x30`, `+0x10` and `+0x0C` and the count in
`+0x18`, stores the first point in `+0x5C`, `+0x60` and its cell in `+0x44`, `+0x48`, and returns 1.

`0x0003B97C` and `0x0003B9B4` call `0x0003B0E4` with the descriptor `+0x00 = 1`, `+0x04 = 1`,
`+0x08 = 2000`, `+0x10 = 7` and `+0x1C` 1 and 2.

## Interpretation

The three records are brigand forces. Slot 0 is the yearly brigand order (FND-STRATEGY-002) from a
random property's own route pair; slots 1 and 2 are the fixed Scottish and Welsh raids from London.
The descriptor's month and year are the date the order ends, and a slot runs one order at a time.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003B0E4..0x0003B29F`, `0x0003B97C..0x0003B9F0`, `0x00049ED0..0x0004A06C` in `CD:CONQUER.EXE`.
