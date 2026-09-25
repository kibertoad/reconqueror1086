# place_person

The index in `persons` of the person whose place the player is visiting, `INT32`, kept in the global at `0x000AC180` [FND-UI-004, FND-TOURNEY-005]. The village exterior takes the place's catalog record and name from it, and the tournament melee takes it modulo 3 as the digit of its scene name.
