# PERSON

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-PERSON-001` | Character table, CHARACTR.DAT and saved copies | supported | missing | None | None | supported | The rebuild does not read or write the character table; the player's attributes are named properties and saves are JSON. |
| `FMT-PERSON-002` | Youth dilemma, DILEM0.DAT to DILEM29.DAT | supported | complete | None | None | implemented | `DilemmaTextDecoder` reads every section by its marker and also reads the age and title from the comments, which the executable skips. |
| `RULE-PERSON-001` | Character attributes | supported | partial | None | None | supported | Only the player's attributes are kept, as named properties, and some writes do not apply the limits of set_attr. |
| `RULE-PERSON-002` | Loading and saving the character table | supported | missing | None | None | supported | There is no table and no load shift; a generated knight starts from random 2 to 12 values. |
| `RULE-PERSON-003` | Character generation, the pre-generated knights and dubbing | supported | partial | None | None | supported | The six knights are fixed templates, Sir Chaunce Norman included, with the dubbed wealth folded in; the generated knight starts at 240 shillings; COLOR uses 0, 3 and 5; dubbing does not age the other characters or give the five items as counts. |
| `RULE-PERSON-004` | Youth dilemmas | supported | partial | None | None | supported | Selection by age and the ordered breakpoints match; NONE scores 0 and changes nothing, and the handlers' item grants are not given. |
| `RULE-PERSON-005` | Courtship and marriage | sourced | partial | None | None | sourced | Colours need a per-lady honour minimum and piety maximum, rewards are indexed by wins and marriage follows a win count, all guesses in `Balance.Courtships`; imported lady conversations take over the rewards when present. |
| `RULE-PERSON-006` | Retirement at 30 | supported | partial | None | None | supported | The campaign ends when the yearly birthday takes AGE to 30 or more, without the movie. |
