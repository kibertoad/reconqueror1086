# Data-driven gameplay architecture

Gameplay content is described in typed definition records in `Balance.cs` and interpreted by the campaign engine. Presentation code reads the same definitions for menus, so adding an item or courtship entry does not require another UI branch.

## Definition families

| Definition | Content controlled | Interpreter |
| --- | --- | --- |
| `CropBalance` | cost, labor, normal and harvest returns | `Campaign.Plant` / monthly settlement |
| `ForestBalance` | cost, labor and monthly return | `Campaign.DevelopForest` / monthly settlement |
| `UnitBalance` | 50%-100% purchase and upkeep curves | recruiting / monthly settlement |
| `EquipmentBalance` | price, power, armor, availability and equipment slot | blacksmith / inventory |
| `BuildingDefinition` | cost, productivity bonus and repeatability | `Campaign.Build` |
| `CourtshipDefinition` | eligibility, marriage threshold and win-indexed rewards | `Campaign.RequestColors` / jousting |
| `VictoryDefinition` | required destination, fiefs, army, strength and items | `Campaign.AttemptVictory` |
| `GrowthBand` | capacity threshold and population growth rate | monthly settlement |
| `WorldLocation` | map position, destination type, garrison and villages | travel / conquest |
| `Dilemma` | prompt, choices and stat/wealth/item effects | character creation |
| `YouthDilemmaDefinition` | imported prompt, scoring attribute, breakpoints, outcome prose and typed changes | `YouthDilemmaRules` / `Campaign.AnswerDilemma` |
| `YouthDilemmaPoolDefinition` | age range, variants per age and stable resource-number mapping | campaign youth selection |
| `YouthDilemmaPresentationDefinitions` | original HAT-derived choice hitboxes | dilemma mouse input and highlighting |
| `SiegeDefinition` | map size, garrison scaling, champion health, healing and break chance | `SiegeSession` |
| `SiegeLayout` / `SiegeSpawn` | imported collision tiles, viewer start/facing, and original defender positions | `SiegeSession` |
| `FieldBattleDefinition` | battlefield size, counter bonus, morale and withdrawal pressure | `FieldBattleSession` |
| `StrategicDefinition` | spy cost, interception chance and minimum field force | campaign travel and reconnaissance |
| `StrategicEnemyMovement` / `StrategicSpyReport` | persisted five-slot hostile movement and the one-shot report captured before movement advances | `Campaign.AdvanceDays` / enemy movement partial |
| `OriginalStrategicMovement` | executable movement/person/property layouts, all 14 initial numeric property rows, household eligibility/counts, generation gate, and lord/household force initializer; deliberately does not identify property rows with the runtime's named destinations | strategic movement recovery boundary |
| `TournamentOpponentDefinition` | wager, joust tolerance and eight-man melee composition | joust and tournament skirmish settlement |

The remaining switches in the core are state adapters: they map typed equipment slots, building kinds, and dilemma attributes to serialized player fields. They contain no prices, balance coefficients, content names, reward order, or eligibility rules. Imported resource names are translated at boundaries such as `ImportedDilemmaAdapter` and `ImportedSiegeLayouts`; screen code does not interpret raw resource records.

## Validation

The executable specifications reject duplicate equipment and courtship names, duplicate win rewards, mismatched building keys, and dragon requirements that cannot be earned from a defined reward ladder. Full campaign behavior tests then exercise the same generic interpreters used by the game.

## Runtime paths and packages

`GamePathResolver` keeps distributable content and writable player state separate. Imported `UserContent` is selected explicitly by `--user-content`, then `RECONQUEROR_USER_CONTENT`, an adjacent or package-root directory, a development checkout, and finally the platform's per-user application-data directory. Saves and settings always use the per-user `ReConquerorAD1086` state root, so an installed game never assumes its program directory is writable. Windows Inno Setup, Linux Debian, and macOS package builders all consume the same verified, self-contained portable publish layout.

`Directory.Build.targets` enforces a 1,000-line ceiling for every compiled C# source file in every project, making oversized responsibilities a local-build, test, CI, and release failure. `ConquerorGame` is divided into lifecycle, campaign-input, general-presentation, and location/combat-presentation partials; the large resource regression suite is divided along the same responsibility boundaries. The limit can be lowered with `MaximumSourceFileLines` for validation; disabling it requires the explicit `DisableSourceFileLineLimit=true` MSBuild property.
