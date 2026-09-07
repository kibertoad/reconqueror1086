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
| `SiegeDefinition` | map size, garrison scaling, champion health, healing and break chance | `SiegeSession` |
| `FieldBattleDefinition` | battlefield size, counter bonus, morale and withdrawal pressure | `FieldBattleSession` |
| `StrategicDefinition` | spy cost, interception chance and minimum field force | campaign travel and reconnaissance |
| `TournamentOpponentDefinition` | wager, joust tolerance and eight-man melee composition | joust and tournament skirmish settlement |

The remaining switches in `Model.cs` are state adapters: they map a typed equipment slot or building enum to serialized player fields. They contain no prices, balance coefficients, content names, reward order, or eligibility rules.

## Validation

The executable specifications reject duplicate equipment and courtship names, duplicate win rewards, mismatched building keys, and dragon requirements that cannot be earned from a defined reward ladder. Full campaign behavior tests then exercise the same generic interpreters used by the game.
