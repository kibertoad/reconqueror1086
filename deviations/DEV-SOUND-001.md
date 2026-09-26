# DEV-SOUND-001

- Departs from: BUG-SOUND-001, RULE-SOUND-002
- Reason: The original plays the five fief management samples tagged 11,050 Hz with whatever rate step their voice last used, or a step of 0 on a voice never used (BUG-SOUND-001). The rebuild plays every sample at the rate its own header gives.
- Setting: None
- Default: mandatory
- Justification: The original has no fixed speed for these samples: they play at the intended speed after an 11,025 Hz sample on the same voice, twice as fast after a 22,050 Hz one, and with a step of 0 on a fresh voice, and which voice a sample gets depends on the ten-voice allocation of RULE-SOUND-002. The rebuild gives them the speed they have whenever the original gets it right. The departure changes only how a sound plays and no game state, so no player loses an outcome. The host mixer has no ten-voice table, so a setting could not reproduce the original's result in any case.
- Dropped: no
