# Fidelity ledger

The reimplementation separates verified original values from behavior inferred where the 1995 executable and manual do not expose an exact formula.
Binary-derived claims and their confidence levels are recorded in [`original-findings.md`](original-findings.md); provisional values must not be presented as original facts.

## Verified balance encoded verbatim

- Attribute ranges and qualitative ranks (0-20)
- Template values documented for Ronald, Hayward, Spencer, Mordred, and Simon
- Custom starting wealth of 240 shillings and baseline equipment
- Crop cost and monthly/harvest revenue at 50% productivity
- Forest cost, labor, and revenue at 50% productivity
- Army purchase and upkeep values at 50% and 100% productivity
- Swordsmen beat halberdiers, halberdiers beat knights, knights beat swordsmen
- Steward +10%, beadle +5%, priest +5%, church +15%, monastery +15%, woodward +5%
- One house and one food tile per 100 people; 25% bean requirement; July penalties
- 200-shilling loan ceiling with 50% harvest interest; spy price of 80
- Shop prices, 75% resale rule, armor bar values, and crossbow availability
- Three jousts and one skirmish per tournament, lance experience cap of 20
- Joust and melee wager dialogue, five selectable melee opponents, and the documented typical 20-80 shilling wager range
- Lady-specific colors, piety/fame restrictions, win-indexed courtship rewards, and simultaneous courtships
- Dragon route requirements: Jane's lance, Wendessa's Shield of St. George, Valetta's armor, and Mighty strength
- Thirty imported youth-dilemma definitions grouped five per age from 12 through 17, with executable-confirmed random selection and inclusive breakpoint outcome rules; six representative summaries remain as the no-media fallback
- Age 18 campaign start, age 30 deadline, crown and dragon endings

## Inferred and isolated

- Intermediate productivity interpolation for unit prices/upkeep is linear between the documented endpoints.
- Population growth follows the documented 1,200-population trial bands and scales by productivity.
- Field battle casualties preserve the documented counter triangle but use a seeded attrition calculation because exact executable coefficients are unavailable.
- Tactical field battles expose independent swordsman, halberdier and knight formations with live hold, advance, flank, captain-control and withdrawal orders. The exact original formation geometry remains under investigation.
- Castle assault hit rolls use weapon guide strength, dexterity, stamina and defined armor totals. The original executable's exact random tables remain unknown.
- Costs not given by the available manual/FAQ (castle staff and civic construction) are centralized in `Campaign.Build` pending executable observation.
- World coordinates, road-distance conversion, garrisons, and the tournament circuit are provisional pending extraction of the original map tables.
- Tournament opponent identifiers are partially recoverable from `CONQUER.EXE`; the current wager progression, difficulty tolerances, and individual eight-man compositions remain provisional pending disassembly or controlled observation.

## Media status

The owned release contains a 35,361,714-byte `.RES`-signature `C1086.GOB` archive, a raw MODE1/2352 CD data track, thousands of Smacker (`.SMK`) sequences, Sierra `.RES/.LOW` resources, and five CD-audio tracks. `tools/Conqueror.Inspect` inventories that data track; `tools/Conqueror.Import` installs owned resources locally with a provenance manifest and lossless CDDA WAV conversion. Generated proprietary files remain ignored and are not redistributed. The bounded kind-1 archive decoder is validated across all 20,381 GOB and scene blocks, while the executable-confirmed kind-2 LZW decoder validates all five of its GOB entries and all four resulting 640x480 PCX images. The runtime currently plays imported CD audio; renders the decoded original title, options, character, dilemma, England-map, load, and portrait art; and uses the morality-screen palette for dilemma CSF choices. SMK, broader palette-to-animation association, and other Sierra payload formats remain incomplete.

## Controls represented

The map contains 18 named destinations with selection and calendar-costed travel, and links the original high-level destinations (Home, Village, Tournament, Overview, field battle, siege, dragon, London). Castle assaults now include first-person movement/facing, radar, normal and secret doors, food, treasure, melee and crossbow combat, champions, retainers, equipment breakage and retreat; London uses the same siege system and awards the crown when captured. Tournaments expose joust timing, limited skirmishes and courtship; the tactical room exposes all productive and military categories. The village includes taxation and a scrollable catalog containing every documented shop item. Save/load are modern additions and do not alter simulation balance.
