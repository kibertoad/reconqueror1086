# BATTLE

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-BATTLE-001` | Field battle unit, one troop in an interactive field battle | supported | partial | None | None | supported | The unit class keeps every field, but its category names call 0 swordsmen and `0x78` halberdiers. |
| `FMT-BATTLE-002` | Pointer event, one entry of the pointer queue | supported | partial | None | None | supported | The queue keeps time, x, y and kind; `unk_10` is not kept. |
| `RULE-BATTLE-001` | Field battle resolution | supported | partial | None | `DEV-BATTLE-001` | supported | The automatic battle adds a third of each morale value. The choice screen, display mode and survey are host code. |
| `RULE-BATTLE-002` | Field battle units and formations | supported | partial | None | None | supported | Formations 0 to 2 leave the foe unplaced and make no draw, and the wedge uses the smallest row count. |
| `RULE-BATTLE-003` | Field battle pass | supported | partial | None | None | supported | The session composes the pass from the mapped pieces; the host supplies the clock gate and key input. |
| `RULE-BATTLE-004` | Field battle rectangle tests | supported | partial | None | None | supported | The neighbour probe stops at a miss only after a first-corner hit; the original stops after any dead hit. |
| `RULE-BATTLE-005` | Field battle contact | supported | partial | None | `DEV-BATTLE-001` | supported | Adds a third of the morale, returns at once on a filtered contact, and makes no sound draw. |
| `RULE-BATTLE-006` | Field battle turning | supported | complete | None | None | implemented | None |
| `RULE-BATTLE-007` | Field battle idle update | supported | partial | None | `DEV-BATTLE-002` | supported | The automatic branch writes the source's x into the chosen unit where the original copies the chosen unit's destination; acquisition stops at a miss only after a first-corner hit; a unit already at its destination on one axis probes instead of stopping. |
| `RULE-BATTLE-008` | Field battle pointer handling | supported | partial | None | None | supported | The retreat question is a host dialog answered with Enter or Escape. `FieldBattlePointerControls` gives the replacement overhead battle mouse selection and visible order buttons that do not follow the original's pointer handling. |
| `RULE-BATTLE-009` | Field battle keys | supported | partial | None | None | supported | The host maps its own keys to the original codes. |
| `RULE-BATTLE-010` | Battle clock | supported | partial | None | None | supported | Both clocks are derived from host time at the original rates. |
| `RULE-BATTLE-011` | Field battle drawing | supported | complete | None | None | implemented | None |
| `RULE-BATTLE-012` | Pointer events | supported | partial | None | None | supported | An unmatched release gives code 0, and a press is forgotten once released. |
