# DEV-ESTATE-001

- Departs from: BUG-ESTATE-003, RULE-ESTATE-003
- Reason: When the player could pay the July demand, refuses and then kills Drogo, the original keeps the debt, and Drogo returns the next July for it (BUG-ESTATE-003). The rebuild clears the debt after every won fight.
- Setting: None
- Default: mandatory
- Justification: After the fight both branches of the original show the same message, that the moneylender will not come again, but only the branch for a player who cannot pay clears the debt. The rebuild makes the refusal branch do what the message tells the player and what the other branch already does. A player who fought Drogo on the strength of that message has no way to learn that the debt stays until Drogo comes back. A setting would keep a message that the game then contradicts, which is no choice the design offers the player.
- Dropped: no
