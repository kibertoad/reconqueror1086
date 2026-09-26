# SAVE

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-SAVE-001` | Saved game, SAVEGAME\CONQn.SAV | supported | missing | None | None | supported | The rebuild writes `campaign-{n}.json` with a backup copy instead of a resource container. |
| `FMT-SAVE-002` | Strategic map state, TROOPS.SAV | supported | missing | None | None | supported | The campaign JSON holds the rebuild's own state. |
| `FMT-SAVE-003` | Properties, persons, items and variables, PROPERTY.SAV | supported | missing | None | None | supported | The campaign JSON holds the rebuild's own state. |
| `FMT-SAVE-004` | Calendar block, ~~2.SAV | supported | missing | None | None | supported | The campaign JSON holds the campaign date. |
| `FMT-SAVE-005` | Home fief, ~~3.SAV | supported | missing | None | None | supported | No fief lists are saved. |
| `FMT-SAVE-006` | Armies, ~~4.SAV | supported | missing | None | None | supported | No 640-byte army records. |
| `FMT-SAVE-007` | Tournament, ~~5.SAV | supported | missing | None | None | supported | No tournament block is saved. |
| `FMT-SAVE-008` | Starting persons and properties, default.dat | supported | missing | None | None | supported | The rebuild starts a new campaign from its definitions and writes no `default.dat`. |
| `RULE-SAVE-001` | Load and save screens | supported | partial | None | None | supported | `CampaignSaveSlots` keeps five slots and lists them; there is no title editor, and the slot shows the player name and date. |
| `RULE-SAVE-002` | Writing a saved game | supported | partial | None | None | supported | `CampaignSaveSlots` writes one JSON file per slot with a backup, and a separate autosave the original lacks. |
| `RULE-SAVE-003` | Reading a saved game | supported | partial | None | `DEV-SAVE-001` | supported | A damaged slot falls back to its backup; the original stops the program. There is no version string. |
| `RULE-SAVE-004` | Starting values for a new game, default.dat | supported | missing | None | None | supported | A new campaign is built from the definitions. |
| `SCR-SAVE-001` | Load game | supported | missing | None | None | supported | No LOADGAME.PCX screen. |
| `SCR-SAVE-002` | Save game | supported | missing | None | None | supported | No SAVEGAME.PCX screen. |
