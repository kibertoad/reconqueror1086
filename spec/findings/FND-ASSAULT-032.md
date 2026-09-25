---
id: FND-ASSAULT-032
title: Combat rows are 25 records of 28 bytes in the executable's data, whose first dwords are dice, sides, penetration, a swing divisor and a reach
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009CE14..0x0009D0D0
tool: Ghidra 12.1.3
environment: null
---

## Observation

The damage routine, the actor tests and the foreground weapon routine index a table at
`0x0009CE14` (object 2 offset `0xCE14`) as `0x0009CE14 + 28 * row`, for rows 0 to 24. They read
the signed dwords at record offsets `0x00` (dice count), `0x04` (die sides), `0x08` (armour
penetration), `0x0C` (the divisor of the foreground swing, FND-ASSAULT-040) and `0x10`
(contact reach). The divisor at `0x0C` lies between 380 and 1,000 in all 25 rows.

## Interpretation

Each weapon has a combat row. The row gives its damage dice, how much armour it ignores, how
fast the player's swing animates, and how far it reaches.

## Alternatives

None known.

## How to reproduce

Follow the reads of `0x0009CE24` in `0x0004F7A2` and `0x0004F070` back to the table base.
