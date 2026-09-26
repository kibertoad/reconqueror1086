# CONFIG

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-CONFIG-001` | CONQUER.INI, the settings file | supported | missing | None | None | supported | The rebuild never reads or writes `CONQUER.INI`; `GameSettingsStore` keeps its own JSON file. |
| `FMT-CONFIG-002` | Setting node, one key and value of the loaded CONQUER.INI | supported | missing | None | None | supported | No in-memory setting list. |
| `RULE-CONFIG-001` | Finding and loading CONQUER.INI | supported | missing | None | None | supported | The rebuild finds the installation through its own path resolver and starts without a settings file. |
| `RULE-CONFIG-002` | Reading and writing a setting | supported | partial | None | None | supported | `GameSettingsStore` loads and saves the CD music, sound effects, speech and animation switches, with a backup copy the original lacks; there is no key lookup and no movie, credits or MIDI switch. |
| `RULE-CONFIG-003` | View, controller and movie settings | supported | missing | None | None | supported | No `POVWINSIZE`, `SLOWMACHINE`, CyberMan or `FULL_MOVIE` handling. |
| `RULE-CONFIG-004` | Field battle display mode from WAR_MODE | supported | missing | None | None | supported | The battle is drawn by the host renderer; `OriginalStrategicInteractiveEncounterViewport` takes the resolved width as given. |
