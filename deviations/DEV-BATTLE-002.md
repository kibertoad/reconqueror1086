# DEV-BATTLE-002

- Departs from: BUG-BATTLE-002, RULE-BATTLE-007
- Reason: In the idle update, the original turns a unit toward the unit whose index its corner search left in EAX, the last unit hit or 0 after a miss, where the stored target belongs (BUG-BATTLE-002). The rebuild turns the unit toward the target it keeps.
- Setting: None
- Default: mandatory
- Justification: The original's unit can turn away from the foe it met toward an unrelated unit, often the first unit of the player's own army, because of a value a search left in a register. The facing routine exists to turn a unit toward its target, and the rebuild does that on every call. Which unit the original picks depends on the order of an internal corner search that the player neither sees nor controls, so no player can bring about the wrong turn on purpose, and the rebuild takes away no outcome a player could aim for. A setting would only keep units spending turns facing their own side.
- Dropped: no
