---
id: FND-ASSAULT-043
title: A missed contact of the player's weapon breaks it on a zero draw from 200 plus 50 times the row's penetration, except for row 0
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000551BF..0x00055222
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004CE4C
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x000551BF`..`0x00055222`, on the path the player's weapon takes when its contact misses, the
code skips the rest for combat row 0. For any other row it draws `random(200 + 50 * p)` through
`0x0004CE4C`, where `p` is the row's dword at offset `0x08`, and breaks the weapon only when the
draw is 0.

## Interpretation

Every weapon but the one in row 0 can break on a miss. A weapon with more armour penetration is
less likely to break: with a penetration of 6 the chance is 1 in 500.

## Alternatives

None known.

## How to reproduce

Open `0x000551BF`; the comparison with row 0 comes first, then the multiplication by 50 and the
call to the random routine.
