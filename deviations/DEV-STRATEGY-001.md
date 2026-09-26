# DEV-STRATEGY-001

- Departs from: BUG-STRATEGY-001, RULE-STRATEGY-003
- Reason: The rare pursuit branch of the hostile generator tests the alert byte of whatever property the generator's local last held, or of the byte before the property table when that local is -1 (BUG-STRATEGY-001). The rebuild takes the branch only after a detection in the same pass and tests the property that detection found.
- Setting: None
- Default: mandatory
- Justification: The value the original tests is left over from earlier work of the generator, not a property the branch chose, and when the local is -1 it is a byte outside the table. The rebuild tests a property the generator has just detected, the one case in which the branch's test refers to a real property. The branch runs about once in 25 timed movements, and the player can neither see nor choose which property it reads, so no player can plan around it. The original's result also depends on what the local holds at the start of a session, which is not known, so a setting could not reproduce it.
- Dropped: no
