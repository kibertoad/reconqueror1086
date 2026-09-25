# DEV-ASSAULT-003

- Departs from: RULE-ASSAULT-028
- Reason: The original shows each of the blood effect's four frames for one pass of an assault main loop that never waits, so the effect lasts as long as four drawn frames take on the machine. The rebuild shows each frame for 70 ms of its simulation clock.
- Setting: None
- Default: mandatory
- Justification: The original has no duration of its own to reproduce: on a fast machine the effect is over before a player can see it, and on a slow one it lingers, and neither is more the original's behaviour than the other. Fixed steps keep the four frames, their order and the choice between the two effects, and remove only the dependency on host speed.
- Dropped: no
