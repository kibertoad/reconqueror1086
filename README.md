# ReConqueror A.D. 1086

A clean-room C# and MonoGame reimplementation of *Conqueror: A.D. 1086*.
This repository contains no original copyrighted game assets. It is playable
with generated fallback presentation, while owners of a legal GOG copy can use
the included importer to enable the original artwork, animation, dialogue,
speech, sound effects, and music from their own installation.

The simulation lives in `Conqueror.Core`; the desktop runtime lives in
`Conqueror.Game`. This is a broad, playable pre-1.0 recreation, not yet a claim
of pixel-perfect or rule-perfect parity with the original executable.

## Quick start

1. Install the .NET SDK, then clone this repository.
2. On Windows, double-click `Start Conqueror 1086.bat`, or run:

   ```powershell
   dotnet run --project src/Conqueror.Game
   ```

3. To use media from a legal original installation, double-click
   `Install Original Resources.bat`. The importer reads the source without
   modifying it and installs validated resources under the Git-ignored
   `UserContent` directory.

Windows, Linux, and macOS packaging scripts are available under `tools`.
Published installers, when available, are listed on
[GitHub Releases](https://github.com/kibertoad/reconqueror1086/releases/latest).
No package includes original game resources.

## Project status

The campaign can be played from character creation through estate management,
tournaments, field battles, castle assaults, and either major ending. Original
media import, modern saves, controller input, and cross-platform packaging are
operational. Exact rules, AI, presentation timing, and some original resource
bindings remain active reverse-engineering work.

Technical claims are graded as Confirmed, Corroborated, or Provisional in the
[original findings register](docs/original-findings.md). The complete delivery
sequence and proof gates are in the [implementation plan](docs/implementation-plan.md),
and the current parity limits are summarized in the [fidelity ledger](docs/fidelity.md).

### Implemented

| Area | Available now |
| --- | --- |
| Character and campaign | Six named profiles, custom character generation, the six formative ages, March 1086 start, daily/monthly progression, and the age-30 deadline. Imported content activates all 30 original dilemma definitions and their executable-confirmed selection/outcome rules. |
| Estate and economy | Castle staffing, farm, village, forest, productivity, population, taxation, loans, harvest debt, army upkeep, shop transactions, and transactional management books. Original crop/forest tables, unit endpoints, shop prices, and the 75% resale rule are represented. |
| Strategy | Eighteen destinations, travel time, five persistent army divisions, recruitment, joining, spies, field orders, hostile garrisons, interception, conquest, and a moving tournament circuit. |
| Tournaments and quests | Three jousts and one skirmish per tournament, wagers, courtship ladders, marriage, imported conversations, the dragon equipment quest, and crown/dragon/age endings. |
| Field and siege combat | Playable formation orders and counter-based field battles. First-person assaults include imported scene geometry, facing, doors, secret rooms, radar, melee, crossbows, ammunition, champions, retainers, food, treasure, equipment breakage, retreat, and progression. Executable-confirmed combat rows now drive reach, hit eligibility, damage, armor penetration, combatant attributes, and player health. |
| Original-content mode | Read-only import with hash/provenance validation; decoded PCX/PCC/HAT/CSF scenes, conversations, `.666` audio, CD music, and direct Smacker playback. The title, options, character flow, dilemmas, estate, map, village, tournament, shop, combat, dragon, credits, and endings use owned media where bindings are known. |
| Persistence and input | Five atomic manual slots with backups, a separate autosave, schema migration, settings recovery, keyboard/mouse controls, controller navigation and virtual pointer, pause, reduced motion, and aspect/integer scaling. |
| Engineering baseline | Headless deterministic core, bounded parsers, strict legal-boundary checks, isolated xUnit and executable-specification suites, a 1,000-line compiled-source ceiling, and self-contained Windows/Linux/macOS packaging automation. |

### Still missing or provisional

| Area | Remaining work |
| --- | --- |
| Exact gameplay parity | Recover remaining economy interpolation, construction costs, character edge cases, tournament records, world tables, political simulation, quest boundaries, and native RNG consumption. The FAQ consistency audit identifies known provisional matches and conflicts. |
| Combat parity | Confirm foreground and door cadence, critical-hit behavior, sub-cell movement, enemy/retainer AI, stair transitions, enemy food use, pickup reward payloads, loot, siege consequences, and the remaining campaign-scene callback. Explicit behavior-19 pickup interaction/range and the data-driven actor state-completion gate are executable-confirmed; sequential playback of every adjacent actor texture remains provisional. |
| Strategic AI | Replace generated routing, garrisons, tournament movement, field-battle coefficients, captain battle rules, and opponent behavior with executable-backed logic and fixed reference traces. |
| Visual and audio parity | Bind remaining palettes, Sierra payloads, conversation entry points, `.666` events, Smacker triggers/seeking, original font metrics, status fields, terrain composition, and exact hit regions. |
| Platform polish | Installers are unsigned. Native installer QA, macOS notarization, customizable bindings, and broader accessibility work remain. |
| Compatibility policy | Recreation saves are versioned and migrated, but their final post-1.0 compatibility policy is not yet set. Importing or exporting original DOS save files is not currently supported. |

### Permanent scope boundaries

- Original copyrighted assets are never committed or distributed; users import
  them locally from a legal copy.
- The original installation is treated as read-only.
- Reverse-engineering reports contain metadata, mappings, and formulas rather
  than proprietary executable bytes or extracted resources.
- Modern safety and platform requirements take precedence over reproducing
  unsafe legacy behavior.

## Quality-of-life additions

- Borderless fullscreen and aspect-fit/integer scaling on a centered 1024x768
  virtual canvas.
- Concurrent keyboard, mouse, and controller input with a controller-driven
  virtual pointer.
- Independent music, sound-effect, and speech levels plus reduced motion.
- Five recoverable manual save slots and a separate transition-triggered
  autosave.
- Generated presentation and summaries when original media is unavailable.
- Visible Windows startup diagnostics under
  `%LOCALAPPDATA%\ReConquerorAD1086\Logs`.

## Controls

| Action | Keyboard | Controller / mouse |
| --- | --- | --- |
| Navigate | Arrow keys; screen-specific shortcuts | D-pad or left stick; point and click |
| Select / confirm | Enter | A / primary click |
| Back | Escape | B / secondary click |
| Pause | Pause | Start |
| Load game | F9 | Back/View |
| Save active slot | F5 | Use the save screen controls |
| Restore autosave | F8 on the load screen | Use the load screen control |
| Scaling mode | F10 | — |
| Windowed / fullscreen | F11 | — |
| Pointer control | Mouse | Right stick; right trigger clicks, left trigger secondary-clicks |

Screen-specific prompts expose travel, dialogue, shop, tournament, army, and
first-person commands. Keyboard, mouse, and controller input can be used
concurrently; moving the mouse immediately retakes pointer control.

## Verification

On Windows, double-click `Run Tests.bat`. It enforces the repository legal
boundary, builds into an isolated temporary directory, and runs both the xUnit
suite and broader executable specifications without requiring a graphics
device or touching normal game output files.

The resource importer also supports `--verify`, `--repair`, and `--uninstall`.
See [original analysis](docs/original-analysis.md) for the read-only inspection
workflow and [resource formats](docs/resource-formats.md) for the clean-room
technical specifications.

## Crash reports

Windows startup failures produce a visible error and a local diagnostic log
below `%LOCALAPPDATA%\ReConquerorAD1086\Logs`. Nothing is uploaded
automatically.

## Acknowledgements

First and foremost, thank you to the developers, artists, writers, musicians,
and publishers of the original *Conqueror: A.D. 1086*. Their unusually rich
blend of strategy, role-playing, simulation, and first-person action is the
reason this recreation exists.

Special thanks to **mikel123456** for the extensive
[*Conqueror 1086 A.D.* FAQ](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730).
It is an important, generally trusted starting point for mechanics, routes, and
observed behavior. Its claims are still corroborated against the owned
executable, decoded resources, original documentation, or controlled play
before exact formulas and values are labeled Confirmed. The current comparison
is maintained in the
[GameFAQs consistency audit](docs/original-findings.md#gamefaqs-secondary-source-consistency-audit).

This project copies no original source code and redistributes no copyrighted
resources. Players are expected to own a legal copy and import its assets
locally.

## License

Copyright (C) 2026 kibertoad.

The original code in this repository is licensed under the
[GNU General Public License v3.0](LICENSE). The license does not cover or grant
rights to the original *Conqueror: A.D. 1086* assets, which are not distributed
by this project.
