# DEV-STRATEGY-003

- Departs from: BUG-STRATEGY-005, RULE-STRATEGY-018
- Reason: The original's spy report converts the three troop counts into one buffer before joining the text, so each line shows the swordsmen count three times (BUG-STRATEGY-005). The rebuild shows the swordsmen, halberdiers and knights counts of the force.
- Setting: None
- Default: mandatory
- Justification: The player pays for a spy to learn the strength of a hostile force, and the original shows one of its three numbers in all three places, so the report tells the player less than its labels claim. The rebuild shows the counts the force holds, and the forces are the same in both. The departure changes only text on a screen and no game state, so nothing a player could do in the original is lost. A setting would only keep a report that misstates the numbers it labels.
- Dropped: no
