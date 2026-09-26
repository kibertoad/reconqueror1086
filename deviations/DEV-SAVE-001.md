# DEV-SAVE-001

- Departs from: RULE-SAVE-003
- Reason: When a saved game cannot be read, the original stops the program with a fatal error. The rebuild keeps the previous copy of each slot and loads that copy instead, and reports the damaged slot on the load screen.
- Setting: None
- Default: mandatory
- Justification: The departure changes only what happens where the original would stop the program, so there is nothing after it for a player to keep or a test to compare. Stopping the program on a damaged file is not worth a setting.
- Dropped: no
