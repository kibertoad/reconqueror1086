---
id: FND-STRATEGY-044
title: Route resources and nine home-selection table entries, of which eight are selectable
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
    kind: file-data
    offset: 0x000D5C28..0x000D5CEC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000D5CEC..0x000D5D10
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000D5C24..0x000D5C28
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000D4B1C..0x000D4B40
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000D04B4..0x000D0520
tool: Conqueror.Inspect resource reading retained from FND-STRATEGY-016; fingerprinted LE page and relocation reader for corrected tables
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
42 and the `br_` files 9 to 16. Nine consecutive relocated string pointers at `0x0009CA98` name `sc_0.rat`
to `sc_8.rat`. The corresponding nine dwords at `0x0009B8C8` are
17, 18, 24, 29, 86, 144, 166, 28 and 137. FND-STRATEGY-043 bounds the
selector to indices 0 through 7: index 7 has its own person entry 28 and route
`sc_7.rat`; index 8 is not selected by that draw. No new claim about the
meaning of either person's other fields is made here.

### Table verification

The nine person dwords occupy the listed 36-byte shipped-file interval. The
nine route slots each have a data-object LE relocation to offsets 0x7260,
0x726C, 0x7278, 0x7284, 0x7290, 0x729C, 0x72A8, 0x72B4 and 0x72C0.
Their target names occupy file offsets 0x000D04B4 through 0x000D0520.
A source-file search for the known first route name independently finds
0x000D04B4, providing the positive control before applying the object mapping.
The next word after the nine slots has no such relocation; this observation
does not establish the extent of every table consumer.

## Interpretation

The files are the route network: property-to-property roads, the eight selectable starting-home routes, the
brigand routes and the two fixed raid routes. Every route's points are in route-space units. The
`br_` files exist for exactly the groups recorded for the first seven homes, which are the origins a timed brigand
order can have. The executable table names both `sc_7.rat` and `sc_8.rat`; the latter lies
outside this selector's range. Other consumers remain under Q-STRATEGY-043.

## Alternatives

This supersedes FND-STRATEGY-016 in full. Its resource-format and network
observations are retained; the seven-entry and unnamed-route claims are
corrected by reading the shipped tables and LE fixups. No complete reading
of every home-index consumer is claimed.

## How to reproduce

Decode the named entries of `C1086.GOB`, check each length against its first dword, and read the tables at `0x0009C9D4`, `0x0009CA98` and `0x0009B8C8` of `CD:CONQUER.EXE`.


