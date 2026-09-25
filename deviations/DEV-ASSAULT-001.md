# DEV-ASSAULT-001

- Departs from: RULE-ASSAULT-006
- Reason: The original gives state decisions once per pass of an assault main loop that never waits, so how often actors think depends on the speed of the machine. The rebuild runs the thinker pass on its fixed simulation update instead.
- Setting: None
- Default: mandatory
- Justification: The original has no rate of its own to reproduce: every machine, and every DOSBox cycle setting, gives a different one, and none of them is the original's behaviour more than another. A fixed update keeps the order of decisions the rule gives (one test and one handler per visit, cursor order, a sixteenth of the combatants per pass) and removes only the dependency on host speed, which takes no outcome away from the player.
- Dropped: no
