# DEV-BATTLE-001

- Departs from: BUG-BATTLE-001, RULE-BATTLE-001, RULE-BATTLE-005
- Reason: The automatic battle and each strike on a foe unit add the remainder of the morale value divided by 3 (BUG-BATTLE-001). The rebuild adds a third of the morale value in both places.
- Setting: None
- Default: mandatory
- Justification: With the remainder, morale adds 0 to 2 whatever its size, and a morale of 3, 6 or 9 adds nothing, so a higher morale can give a smaller bonus than a lower one. Both places divide and then read EDX, the register that holds the remainder, where EAX holds the third, which is the shape of a register mix-up rather than of a chosen formula. With a third, the bonus grows as morale grows, so raising morale is worth something to the player. The remainder adds at most 2 points and no screen shows the bonus, so no strategy can depend on keeping morale off a multiple of 3, and a setting would only keep a bonus that ignores the value it reads.
- Dropped: no
