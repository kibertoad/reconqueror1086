---
id: FND-STRATEGY-016
title: The route resources are a count followed by that many pairs of signed dwords
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009C9D4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009CA98
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009C9D0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009B8C8
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`C1086.GOB` holds 93 entries named `rt_<a>_<b>.rat`, nine named `sc_0.rat` to `sc_8.rat`, twelve
named `br_<n><a|b>.rat` for `n` of 1, 2, 5, 12, 13 and 14, and `scot.rat` and `wales.rat`. Each of
the 116 is `4 + 8 * count` bytes long: a signed dword count followed by `count` pairs of signed
dwords, with nothing after them. The 14 by 14 byte table at `0x0009C9D4` has 0 on its diagonal and
for the pair of groups 7 and 13 (counted from 1) in both directions, and 1 or 2 elsewhere; the 90
files it selects exist, together with three `rt_` files it never selects. The selected `rt_` files
hold 13 to 197 points and `sc_0.rat` to `sc_6.rat` 9 to 53. `scot.rat` holds 44 points, `wales.rat`
42 and the `br_` files 9 to 16. The seven string pointers at `0x0009CA98` name `sc_0.rat` to
`sc_6.rat`, and the seven dwords at `0x0009B8C8` are person indexes whose groups are 0, 0, 1, 1, 4,
12 and 13.

## Interpretation

The files are the route network: property-to-property roads, the seven starting-home routes, the
brigand routes and the two fixed raid routes. Every route's points are in route-space units. The
`br_` files exist for exactly the groups of the seven homes, which are the origins a timed brigand
order can have. Nothing names `sc_7.rat` or `sc_8.rat`.

## Alternatives

None known.

## How to reproduce

Decode the named entries of `C1086.GOB`, check each length against its first dword, and read the tables at `0x0009C9D4`, `0x0009CA98` and `0x0009B8C8` of `CD:CONQUER.EXE`.
