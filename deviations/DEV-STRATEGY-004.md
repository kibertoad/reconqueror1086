# DEV-STRATEGY-004

- Departs from: BUG-STRATEGY-002, RULE-STRATEGY-005
- Reason: A pursuit of three troops or fewer writes only the halberdiers count, so in the original the swordsmen and knights counts keep what the last force in the same record held (BUG-STRATEGY-002). The rebuild clears a record's troop counts before it builds a force there, so such a pursuit has 3 halberdiers and nothing else.
- Setting: None
- Default: mandatory
- Justification: In the original the size of a small pursuit depends on which force last used the record, a force that may have come from another property and been destroyed long before, so the pursuit can be far larger than the garrison and household the rule sizes it from. The rebuild gives the pursuit the size the rule computes for it. The player cannot see or choose which record a pursuit takes, so no player can plan around the leftover troops, and a setting would only keep a force whose size has no connection to where it came from.
- Dropped: no
