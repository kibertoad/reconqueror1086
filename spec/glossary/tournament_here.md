# tournament_here

Whether this month's tournament is held at the place the player is visiting, `INT32`, kept in the global at `0x0009DED4` [FND-UI-004, FND-TOURNEY-001, FND-TALK-007]. The monthly reset stores 0 in it and the start of a session 1; the village exterior sets it on entry when it is 0; inn patrons other than partner 21 talk only while it is not 0.
