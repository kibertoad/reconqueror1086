# DEV-STRATEGY-002

- Departs from: BUG-STRATEGY-004, RULE-STRATEGY-004
- Reason: The original lowers the live hostile force count only in the removal routine, so forces that vanish on water or on a dropped route stay counted, and once five have vanished no hostile force starts again until the count is reset (BUG-STRATEGY-004). The rebuild counts the active slots, so a vanished force frees its place.
- Setting: None
- Default: mandatory
- Justification: In the original the map can run out of hostile forces for good partway through a session, with nothing on screen to say why, because some paths forget to lower a count. The rebuild keeps the rule's limit of five live forces and lets the generator keep starting forces for the whole session. Where forces meet water depends on routes and draws the player does not control, so no player can bring about the empty map on purpose. A setting would keep a count that disagrees with the forces on the map.
- Dropped: no
