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
- The original store table order, item-image indices, local descriptions, Fighter's Dagger price of 84 shillings, and `BUYSELL.CSF` view/transaction labels
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

The owned release contains a 35,361,714-byte `.RES`-signature `C1086.GOB` archive, a raw MODE1/2352 CD data track, thousands of Smacker (`.SMK`) sequences, Sierra `.RES/.LOW` resources, and five CD-audio tracks. `tools/Conqueror.Inspect` inventories that data track; `tools/Conqueror.Import` installs owned resources locally with a provenance manifest and lossless CDDA WAV conversion. Generated proprietary files remain ignored and are not redistributed. The bounded kind-1 archive decoder is validated across all 20,381 GOB and scene blocks, while the executable-confirmed kind-2 LZW decoder validates all five of its GOB entries and all four resulting 640x480 PCX images. All 26 `.666` sound banks decode into 102 bounded rate-tagged unsigned 8-bit mono samples. The shared interface sample is converted once at startup, retained as a playable buffer, and used for mouse activation when effects are enabled; other event bindings remain incomplete. The runtime currently plays imported CD audio and renders the decoded original screens, estate tiles, cursor, overlays, and selected portraits. All ten Home hotspot labels are bound in executable order and the original Overview and War Planning backgrounds are active; independent army dispatch, exact spy timing/cost, JUMP behavior, estate composition, SMK, broader palette associations, village/location mapping, full dialogue data, and other Sierra payload formats remain incomplete.

## Controls represented

The map contains 18 named destinations with selection and calendar-costed travel, and links the original high-level destinations (Home, Village, Tournament, Overview, field battle, siege, dragon, London). War Planning persists five named divisions, edits 100-serf companies transactionally, enforces the 60-company and home-territory rules, charges upkeep across all divisions, tracks player membership, and assigns recurring spies. The joined division—whichever of the five is selected—supplies travel interception, field-battle survivors, siege retainers, and retreat losses. Unjoined fielded divisions accept independent dated movement orders, render along their routes, persist in transit, and resolve remote interception with a captain's report; exact path selection and captain battle rules remain provisional. Castle assaults now include first-person movement/facing, radar, normal and secret doors, food, treasure, melee and crossbow combat, champions, retainers, equipment breakage and retreat; London uses the same siege system and awards the crown when captured. Tournaments expose joust timing, limited skirmishes and courtship; the tactical room exposes all productive and military categories. The village includes taxation and a scrollable catalog containing every documented shop item. Save/load are modern additions and do not alter simulation balance.
