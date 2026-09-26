# UI

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-UI-001` | Screen layout, a HAT file | supported | complete | None | None | implemented | `HatLayout` takes the region count from the header and requires the file length to match it (a trailing DOS end-of-file byte is allowed), where the original counts regions from the file size. It keeps the 3-byte tag as `UnknownTag` and rejects duplicate ids and regions that miss the declared screen. |
| `FMT-UI-002` | Screen region, one record of a HAT file | supported | complete | None | None | implemented | `HatLayout` reads the region as `HatRegion`. |
| `FMT-UI-003` | Exterior catalogs, VILLAGE.DAT and TVILLAGE.DAT | supported | complete | None | `DEV-UI-001` | implemented | `VillageSceneCatalogDecoder` repairs the three malformed rows, so the record 37 lender keeps the numbers its line gives. It keeps the labels, which the original discards, and rejects any other malformed row. |
| `FMT-UI-004` | Store catalog, WEAPONS.DAT | supported | complete | None | None | implemented | `WeaponStoreDecoder` reads all 40 records; the original never reads record 39. |
| `RULE-UI-001` | Village exterior hot spots | supported | partial | None | None | supported | `OriginalVillageScenePresentation` picks the `VILLAGE.DAT` record from person byte `+0x0F` only for the new-game home; other places fall back to the default scene, and `TVILLAGE.DAT` is not chosen for tournament places. `VillagePresentationDefinitions` maps hot spots to actions by label, where the original maps them by row position. |
| `RULE-UI-002` | Village exterior actions | supported | partial | None | None | supported | The rebuild's village actions lead to its own tournament, inn, forge, lender and church screens; the map exit does not clear the tournament counts, and the tournament does not close when the calendar field changes. |
| `RULE-UI-003` | Store stock | supported | partial | None | None | supported | `Balance.StoreEquipment` offers every store item on every visit and lists owned items among them; there is no per-place residue or chance draw. |
| `RULE-UI-004` | Store purchase and sale | supported | partial | None | None | supported | `Campaign.BuyEquipment` refuses an item already held, and `Campaign.SellEquipment` pays `price * 3 / 4` rounded down, where the original pays `price - price / 4` (23 for a price of 30, against 22); the original also clears every copy on a sale. |
| `SCR-UI-001` | Title screen | supported | partial | None | None | supported | `ConquerorGame.DrawTitle` plays the title movie and draws the title background; its menu is the rebuild's own. |
| `SCR-UI-002` | Game options | supported | partial | None | None | supported | `OptionsHubDefinitions` uses the `OPTION.HAT` rectangles with fallbacks. The rebuild adds settings of its own. |
| `SCR-UI-003` | Character options | supported | partial | None | None | supported | `CharacterCreationDefinitions` uses the `CHAR_OPS.HAT` rectangles with fallbacks. |
| `SCR-UI-004` | Youth dilemma | supported | partial | None | None | supported | `YouthDilemmaPresentationDefinitions`. |
| `SCR-UI-005` | Pre-generated characters | supported | partial | None | None | supported | `CharacterCreationDefinitions.PregeneratedFrom` uses the enabled `PREGEN.HAT` regions and falls back to built-in rectangles. |
| `SCR-UI-006` | Village exterior | supported | partial | None | None | supported | `VillagePresentationDefinitions` and `OriginalVillageScenePresentation`; `OriginalCursorDefinitions` names the six `FFMOUSE.CSF` frames. See RULE-UI-001 and RULE-UI-002. |
| `SCR-UI-007` | Forge and store | supported | partial | None | None | supported | `BlacksmithPresentationDefinitions` and `ShopPresentationDefinitions`; the store lists `Balance.StoreEquipment` (RULE-UI-003). |
| `SCR-UI-008` | Practice grounds | supported | partial | None | None | supported | `PracticePresentationDefinitions` uses the `PRACTICE.HAT` regions in order. |
| `SCR-UI-009` | Castle office | supported | partial | None | None | supported | `HomePresentationDefinitions`. |
| `SCR-UI-010` | War planning | supported | partial | None | None | supported | `WarPlanningPresentationDefinitions`. |
| `SCR-UI-011` | Fief management tables | supported | partial | None | None | supported | `FarmPresentationDefinitions` covers the four sections with rows of its own. |
| `SCR-UI-012` | Estate map | supported | partial | None | None | supported | `EstatePresentationDefinitions`. |
| `SCR-UI-013` | Conversation | supported | partial | None | None | supported | `ConversationPresentationDefinitions`; the rebuild wraps text with its own font metrics. |
| `SCR-UI-014` | Tournament grounds | supported | partial | None | None | supported | The rebuild's `Tournament` screen joins the grounds, stands and tents into one screen of its own. |
| `SCR-UI-015` | Tournament stands | supported | partial | None | None | supported | `TournamentConversationDefinitions` names the ladies and their roots; the stands picture and its regions are not used. |
| `SCR-UI-016` | Tournament tents | supported | partial | None | None | supported | The tent actions run from the rebuild's `Tournament` screen. |
| `SCR-UI-017` | Inn | supported | partial | None | None | supported | `InnPresentationDefinitions` uses the `VINN.HAT` regions; the tournament gate on patrons is not applied (RULE-TALK-004). |
| `SCR-UI-018` | Fief overview | supported | partial | None | None | supported | The rebuild's `Overview` screen draws `f_over.pcx`; the three report pages are not shown. |
| `SCR-UI-019` | Dubbing | supported | missing | None | None | supported | The rebuild goes from character creation to its own briefing screen. |
| `SCR-UI-020` | Screen 5 | supported | missing | None | None | supported | None. |
