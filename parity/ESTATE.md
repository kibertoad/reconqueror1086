# ESTATE

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `RULE-ESTATE-001` | War Planning armies, companies and prices | supported | partial | None | None | supported | Company clicks stage and commit, but raising a company does not charge its price or take population, prices scale by productivity instead of the FAME test, a 60-company limit is added, and buying a spy does not commit the screen. |
| `RULE-ESTATE-002` | Monthly estate pass and upkeep | supported | partial | None | None | supported | Upkeep is charged for every company, but the fief's list costs, the July guard and the months without money are not modelled, and revenue and growth follow the guide. |
| `RULE-ESTATE-003` | Loans and the July collection | supported | partial | None | None | supported | The debt of the loan plus half and the July demand match; a won fight clears the debt on both branches (BUG-ESTATE-003 is not kept) and refuses later loans, and loans below 20 are accepted. |
| `RULE-ESTATE-004` | Productivity from staff and buildings | sourced | partial | None | None | sourced | The guide's bonuses and July penalties are summed in any order. |
| `RULE-ESTATE-005` | Crop and forest revenue | sourced | partial | None | None | sourced | The guide's 50% revenues are scaled linearly by productivity. |
| `RULE-ESTATE-006` | Population, food, housing and taxes | sourced | partial | None | None | sourced | Growth uses the guide's bands scaled by productivity, and the overlord's 10% is not taken. |
