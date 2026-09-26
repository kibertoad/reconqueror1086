# STRATEGY

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-STRATEGY-001` | Strategic movement record, one force on the strategic map | supported | partial | None | None | supported | The records are classes with named fields; `unk_08`, `unk_4C`, `unk_114` and the image handle are not kept. |
| `FMT-STRATEGY-002` | Property record, one castle of the strategic map | supported | complete | None | None | implemented | None |
| `FMT-STRATEGY-003` | Person record, one character of the strategic map | supported | complete | None | None | implemented | None |
| `FMT-STRATEGY-004` | Route file, a list of route points | supported | complete | None | None | implemented | None |
| `FMT-STRATEGY-005` | Strategic terrain grid, icon.jp | supported | complete | None | None | implemented | None |
| `FMT-STRATEGY-006` | Brigand order, the descriptor of one brigand force | supported | partial | None | None | supported | Only the two raid descriptors exist; slot 0, the yearly brigand order, is never created. |
| `RULE-STRATEGY-001` | Strategic pass and the yearly orders | supported | partial | None | None | supported | The yearly brigand order, the order from the king and the planting notice are not issued. |
| `RULE-STRATEGY-002` | Hostile pass and arrival | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-003` | Hostile generator and reactive finder | supported | partial | None | None | supported | The rare pursuit branch runs only after a detection in the same pass, instead of testing the stale property (BUG-STRATEGY-001). |
| `RULE-STRATEGY-004` | Hostile force construction and removal | supported | partial | None | None | supported | The live count is the number of active slots, so forces lost to water or a dropped route free their capacity (BUG-STRATEGY-004 is not reproduced). |
| `RULE-STRATEGY-005` | Hostile force size | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-006` | Direct and pursuit movement | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-007` | Routed movement | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-008` | Retargeting after arrival | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-009` | Terrain speed, seasons and the speed setting | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-010` | Player records on the map | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-011` | Encounter with a hostile force | supported | partial | None | None | supported | BATTLE_WON, BATTLE_LOST and INTELLIGENCE are not changed, the lord rating does not drop, and the king's order is not carried out. |
| `RULE-STRATEGY-012` | New game, joining armies and field placement | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-013` | Map clicks and drawn routes | supported | complete | None | None | implemented | None |
| `RULE-STRATEGY-014` | Strategic grid, projection and camera | supported | partial | None | None | supported | The cell test is a symmetric diamond, so some points find another cell; panning moves both axes in one call and does not hold back the hostile pass. |
| `RULE-STRATEGY-015` | Terrain, markers and the route preview | supported | partial | None | None | supported | PLACEHOLDER: the markers are drawn with the campaign-shell palette; the palette the original installs for them is not traced. |
| `RULE-STRATEGY-016` | Brigand orders, raids and orders from the king | supported | partial | None | None | supported | Only the two raids are created; the yearly brigand order along the `br_` routes and the order from the king are missing. |
| `RULE-STRATEGY-017` | Brigand pass and brigand movement | supported | partial | None | None | supported | BATTLE_WON and BATTLE_LOST are not kept, and the honour penalty for an unfought yearly order never applies because that order is never created. |
| `RULE-STRATEGY-018` | Spies | supported | partial | None | None | supported | The spy is part of the dated campaign model and reports the first active force only; the rule reports every live force, needs a hostile force before it returns, and repeats the swordsmen count in all three places. |
| `RULE-STRATEGY-019` | Map events from conversation variables | supported | partial | None | None | supported | The raid requests from variables 43 and 93 are implemented; the bar scenes and the melee from variable 0x80 are not matched against the rule. |
