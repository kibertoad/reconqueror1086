# ReConqueror A.D. 1086

A clean-room MonoGame reimplementation of the 1991 strategy and role-playing
game *Conqueror: A.D. 1086*, built to make it comfortable to play on modern
computers.

This repository contains **no original game assets**. ReConqueror requires art,
music, speech, sound, video, dialogue, and other resources imported from a
legally owned copy and will stop with a setup error if that verified import is
missing, incomplete, damaged, or from an unsupported release. The DRM-free
[GOG release of *Conqueror: A.D. 1086*](https://www.gog.com/en/game/conqueror_ad_1086)
is the supported source. Import is read-only: it does not modify the original
installation, and imported files stay on your computer.

ReConqueror is a broad, playable pre-1.0 recreation. It is not yet a claim of
pixel-perfect or rule-perfect parity with the original executable.

## Quick start for players

### Windows installer

1. Buy and install a legal copy of
   [*Conqueror: A.D. 1086* from GOG](https://www.gog.com/en/game/conqueror_ad_1086).
2. Open [GitHub Releases](https://github.com/kibertoad/reconqueror1086/releases/latest)
   and, when a Windows installer is listed, download
   `ReConqueror1086-Setup-<version>.exe`.
3. Run Setup. It will look for the GOG installation; if it is not found, select
   the folder containing `game.gog`, `game.ins`, and `C1086.GOB`.
4. Finish Setup. Resource import is required, after which you can launch
   **ReConqueror A.D. 1086** from the Start menu or desktop shortcut.

The installer contains no original assets. It validates and imports the needed
resources from your copy without changing that copy. The Start-menu shortcut
**Import or Manage Original Conqueror Resources** can verify or repair the
local import later.

### Player setup from the current source archive

If the Releases page does not yet contain an installer:

1. Buy and install the [GOG release](https://www.gog.com/en/game/conqueror_ad_1086).
2. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
3. Download this repository with **Code → Download ZIP**, then extract the ZIP
   to a folder where you have write access.
4. Double-click `Install Original Resources.bat`. The script automatically
   uses GOG's default `C:\GOG Games\Conqueror AD1086` folder. If your copy is
   elsewhere, drag that installation folder onto the batch file or run it with
   the folder path as its first argument.
5. Double-click `Start Conqueror 1086.bat` whenever you want to play.

The source launcher performs a clean project compilation before starting. This
prevents an older `bin` assembly from being reused when an archive extraction,
Git update, or branch change restores source files with earlier timestamps.

Imported resources are stored in the local `UserContent` folder and are never
uploaded. The game intentionally does not provide an assetless or placeholder
mode.

## Project status

The campaign can be played from character creation through estate management,
tournaments, field battles, castle assaults, and either major ending. It is a
hybrid migration, not a parity claim: executable-confirmed subsystems coexist
with older recreation adapters while the remaining original paths are recovered
and integrated. Original media import, modern saves, controller input, and
cross-platform packaging are operational.

The tables below distinguish playable features from evidence-backed parity.
In particular, a strategic state, exact terrain grid, route/movement kernels,
seasonal terrain presentation, source marker passes, and basic map selection/
route controls are implemented. The daily campaign adapter, target-confirmation
dialogue, temporary-force producer, and deterministic strategic scheduler still
await their full replacement.

### Implemented and usable now

| Area | Available now |
|---|---|
| Character and campaign | Six named profiles, custom character generation, the six formative ages, March 1086 start, daily/monthly progression, and the age-30 deadline. Imported content activates all 30 original dilemma definitions and their executable-confirmed selection/outcome rules. |
| Estate and economy | Castle staffing, farm, village, forest, productivity, population, taxation, loans, harvest debt, army upkeep, shop transactions, and transactional management books. Original crop/forest tables, unit endpoints, shop prices, and the 75% resale rule are represented. |
| Strategy (legacy adapter) | Eighteen destinations, travel time, five persistent army divisions, recruitment, joining, spies, field orders, hostile garrisons, interception, conquest, and a moving tournament circuit. This is playable recreation behavior, not yet the executable-mapped strategic scheduler. |
| Strategic foundation | New campaigns create and save the original-shaped property/person/movement state, decoded `icon.jp` grid, camera, terrain profile, routes, mutations, and movement remainder. Core implements recovered route, terrain, profile, movement, marker-frame, and projection kernels. When that state is present, terrain uses the source tile order and clipped seasonal `ics`/`ica`/`icw` atlas; player markers use the recovered physical order, 8x persisted source color base, frame offsets, projection, and viewport clipping, followed by the five-slot movement-marker pass with its origin-property frame. The original shield controls map Red/Green/Blue to 0/3/5. The recovered scheduler now advances source-shaped new campaigns once per host 60 Hz fixed map update, rather than at processor speed; old migrated saves do not fabricate the unavailable source fallback globals. |
| Tournaments and quests | Wagered jousts and skirmishes (`parity/TOURNEY.md` lists how far they follow the original), courtship ladders, marriage, imported conversations, the dragon equipment quest, and crown/dragon/age endings. |
| Field and siege combat | Playable formation orders and counter-based field battles. First-person assaults include imported scene geometry, facing, doors, secret rooms, radar, melee, crossbows, ammunition, champions, clickable authored actors and objects, selected-retainer ground orders, the four retainer orders, campaign retainer losses, and placed-scene rewards. `parity/ASSAULT.md` lists how much of the recovered combat rules the rebuild implements. |
| Required original content | Read-only import with hash/provenance validation; decoded PCX/PCC/HAT/CSF scenes, conversations, `.666` audio, CD music, and direct Smacker playback. Startup verifies the supported GOG release and every manifest-owned file before the title screen. |
| Saves and input | Five atomic manual slots with backups, a separate autosave, schema migration, settings recovery, keyboard/mouse controls, controller navigation and virtual pointer, pause, reduced motion, and aspect/integer scaling. |

### Still missing or provisional before a parity claim

| Area | Remaining work |
|---|---|
| Exact gameplay parity | Remaining economy interpolation, construction costs, character edge cases, tournament records, world tables, political simulation, quest boundaries, and native random-number consumption still require executable-backed confirmation. |
| Combat parity | Critical-hit behavior, stair transitions, enemy food use, later formation choices, and some presentation sequencing remain open. `parity/ASSAULT.md` and `parity/VIEW.md` list the rules the rebuild implements only in part. |
| Strategic runtime | The map host now focuses new campaigns, applies source edge panning on its fixed update, and dispatches source marker-selection/route clicks; target-confirmation dialogue, the scheduler, contacts, generated hostiles, and property ownership must still be wired before the dated campaign adapter can be removed. Marker palette identity remains corroborated rather than confirmed. Generated routing, garrisons, tournament movement, field-battle coefficients, captain battle rules, and opponent behavior also still need fixed original-game traces. |
| Visual and audio parity | Some palettes, Sierra payloads, conversation entry points, `.666` events, Smacker triggers/seeking, font metrics, status fields, terrain composition, and exact hit regions remain unmapped. |
| Platform polish | Installers are unsigned. Native installer QA, macOS notarization, customizable bindings, and broader accessibility work remain. |
| Save compatibility | Recreation saves are versioned and migrated, but their final post-1.0 compatibility policy is not yet set. Original DOS save import/export is not supported. |

### Permanent scope boundaries

- Original copyrighted assets are never committed or distributed; players
  import them locally from a legal copy.
- The original installation is always treated as read-only.
- Reverse-engineering reports contain metadata, mappings, and formulas rather
  than proprietary executable bytes or extracted resources.
- Modern safety and platform requirements take precedence over reproducing
  unsafe legacy behavior.

## Quality-of-life additions

- Borderless fullscreen and aspect-fit/integer scaling on a centered 1024×768
  virtual canvas.
- Concurrent keyboard, mouse, and controller input with a controller-driven
  virtual pointer.
- Independent music, sound-effect, and speech levels plus reduced motion.
- Five recoverable manual save slots and a separate transition-triggered
  autosave.
- Verified local use of legally owned original presentation assets, with clear
  setup diagnostics if import or integrity checks fail.
- Visible Windows startup diagnostics under
  `%LOCALAPPDATA%\ReConquerorAD1086\Logs`.

## Controls

| Action | Keyboard | Controller / mouse |
|---|---|---|
| Navigate | Arrow keys; screen-specific shortcuts | D-pad or left stick; point and click |
| Select / confirm | Enter | A / primary click |
| Back | Escape | B / secondary click |
| Pause | Pause | Start |
| Load game | F9 | Back/View |
| Save active slot | F5 | Use the save-screen controls |
| Restore autosave | F8 on the load screen | Use the load-screen control |
| Scaling mode | F10 | — |
| Windowed / fullscreen | F11 | — |
| Pointer control | Mouse | Right stick; right trigger clicks, left trigger secondary-clicks |

Screen prompts expose travel, dialogue, shop, tournament, army, and
first-person commands. Keyboard, mouse, and controller input can be used at the
same time; moving the mouse immediately retakes pointer control.

## Crash reports

Windows startup failures show an error and write a local diagnostic log below
`%LOCALAPPDATA%\ReConquerorAD1086\Logs`. Nothing is uploaded automatically.

## For developers and researchers

The simulation lives in `Conqueror.Core`; the desktop runtime lives in
`Conqueror.Game`. To build from source, install the .NET 10 SDK and run:

```powershell
dotnet run --project src/Conqueror.Game
```

Run `Install Original Resources.bat` first, or pass the verified import with
`--user-content <path-to-UserContent>`.

On Windows, `Run Tests.bat` enforces the legal boundary, builds into an isolated
temporary directory, and runs both the xUnit suite and executable
specifications without requiring a graphics device. [Validation](docs/VALIDATION.md)
describes that gate, the documentation standard check, and what CI runs.
Packaging scripts for Windows, Linux, and macOS are under `tools`.

Technical claims are graded as Confirmed, Corroborated, or Provisional in the
[original findings register](docs/original-findings.md). The full delivery
sequence and proof gates are in the
[implementation plan](docs/implementation-plan.md), current limits are in the
[fidelity ledger](docs/fidelity.md), and decoded formats are documented in
[resource formats](docs/resource-formats.md). These records map each formula or
behavior to its original evidence and to the corresponding implementation and
tests where available.

The resource importer also supports `--verify`, `--repair`, and `--uninstall`.
See [original analysis](docs/original-analysis.md) for the read-only inspection
workflow.

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
resources. Players are expected to buy and own a legal copy, such as the
[GOG release](https://www.gog.com/en/game/conqueror_ad_1086), and import its
assets locally.

## License

Copyright (C) 2026 kibertoad.

The original code in this repository is licensed under the
[GNU General Public License v3.0](LICENSE). The license does not cover or grant
rights to the original *Conqueror: A.D. 1086* assets, which are not distributed
by this project.
