---
id: FND-ESTATE-001
title: The fief-management label catalogs hold 17 castle, 17 village, 4 farm and 7 forest entries
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00093C78..0x00093E2E
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

From `0x00093C78` to `0x00093E2E` the executable holds 45 NUL-terminated strings, each starting on a
four-byte boundary, with the bytes between a terminator and the next boundary not cleared. In order
they are 17 castle entries (Wall, Tower, Great Hall, Servant Room, Guardhouse, Gate House,
Storehouse, Chapel, Well, Stable, Steward, Beadle, Guard Captain, Guard, Priest, Mason, Serf), 17
village entries (Clear Land, Road, Mill, Tavern, Bakery, Inn, Carpenter, Smith, Tanner, Merchant,
Church, Monastery, Barber, Houses, Livestock, Horses, Granary), four farm entries (Grain, Beans,
Vegetables, Fruit) and seven forest entries (Cut Timber, Iron Mine, Woodward, Coal Mine, Gold Mine,
Silver Mine, Prospector).

Two shorter lists in the same form hold the castle entries followed by Clear Land and Road, at
`0x00090664..0x0009071B`, and the forest entries in another order followed by Serf and Road, at
`0x00094360..0x000943BC`.

## Interpretation

These are the row labels of the four fief-management screens, in the order the screens list them.
The 19-entry castle list and the 9-entry forest list have the row counts of the castle and forest
lists of the fief record (FND-ESTATE-003), which suggests that they label those rows.

## Alternatives

Which code reads each list has not been traced, so the match between the shorter lists and the fief
record's rows rests on their counts only.

## How to reproduce

Read the strings at object-2 offsets `0x3C78` to `0x3E2E`, `0x0664` to `0x071B` and `0x4360` to
`0x43BC`.
