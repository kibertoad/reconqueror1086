# Conqueror: A.D. 1086 - MonoGame reimplementation

This is a clean C# / MonoGame reimplementation of the campaign systems in the owned DOS release. It does not modify or redistribute the original installation. The simulation and its balance tables live in `Conqueror.Core`; the desktop front end lives in `Conqueror.Game`.

Gameplay is definition-driven: buildings, equipment slots, population bands, unit counters, courtship ladders, victory requirements, world locations, and youth dilemmas are declared centrally and interpreted generically. See `docs/architecture.md`.

## Run

```powershell
dotnet run --project src/Conqueror.Game
```

The game opens at 1024x768. Press any key or click through the title to reach the original options hub, then choose New Game for the character-options screen, where you can set a name and heraldic color, generate a new character, or select one of six pre-generated characters. Keyboard navigation remains available. `F5` saves atomically to the active slot while retaining the previous valid generation as a recovery backup; `F9` opens the original five-slot load screen and automatically offers that backup when the primary is damaged. Escape or the Resume scroll returns without loading. Saves are versioned JSON with an explicit migration path; the former unversioned single `campaign.json` save remains readable as slot 1.

On Windows, double-click `Start Conqueror 1086.bat` in the repository root. It checks for the .NET SDK and builds/starts the game locally.

Owners of the original GOG release can first double-click `Install Original Resources.bat`. The importer reads the original installation without modifying it, installs exact resources into the Git-ignored `UserContent` directory, converts CDDA tracks losslessly to PCM WAV, and writes a provenance manifest. The launcher makes that catalog available to the game automatically. Imported CD music and the original shared interface click now play; every imported `.666` sample is decoded and converted once into a session cache at startup. Original title/options/character screens and sword pointer, animated dilemmas, seasonal estate tiles, maps, location screens, store art and controls, a tournament portrait, and all 30 youth dilemmas are active. Confirmed blacksmith hotspots and the executable-ordered ten-region Home catalog expose hover labels and actions; the Castle model and three distinct desk books open section-specific `FIEFMGMT` descriptor variants whose OK/Cancel controls commit or roll back pending management changes. Overview and War Planning use their original installed backgrounds. War Planning renders its decoded five-army controls through `FWARPLAN.HAT`, raises or disbands 100-serf companies, enforces the original 60-company limit and away-army restriction, tracks field/join state and recurring spies, and commits or rolls back the full plan. A joined division accompanies the player through travel, interception, field battle, siege, and retreat without falling back to Army 1. Unjoined fielded divisions can be selected and dispatched independently; their dated orders survive saves, their icons advance across the map, and their captains report remote battles. Built-in presentation and dilemma summaries remain the no-media fallback. Rerun the installer after importer upgrades; exact path-by-path army routing and captain battle rules, exact `JUMP!!` behavior, other `.666` event bindings, SMK video, and the remaining Sierra containers still require adapters.

The importer recognizes the supported GOG disc-image hash before extraction and warns—but continues bounded validation—for an unknown release. Before writing, it inventories the exact generated footprint, reports required and available disk space, and fails early if atomic installation cannot fit. It reports the current resource plus written/reused totals during long runs. It also supports `--verify [UserContent path]` to hash-check every manifest entry, `--repair [original installation] [UserContent path]` to rebuild and verify generated content from the read-only source installation, and `--uninstall [UserContent path]` to remove only manifest-owned files. Installs use atomic replacement and leave matching generated files untouched. Verification rejects missing, modified, duplicated, malformed, or root-escaping manifest entries; uninstall prevalidates every path and preserves unlisted files.

The importer also validates stored CSF files as indexed animation sequences. Their pixels and transparency are decoded, but they are not displayed until the correct original screen palettes are identified; rerun the installer after importer upgrades to refresh manifest classifications.

## Implemented systems

- Original six-attribute character model, descriptive ranks, templates, custom rolls, and six formative ages
- March 1086 start at age 18, daily/monthly simulation, and the age-30 loss condition
- Castle staffing, farm, village, forest, productivity and population systems
- All crop and forest development choices, adjustable taxation, and the full scrollable blacksmith catalog
- July bean, housing, servant-room, harvest and debt checks
- Original crop and forest costs/returns; original unit price/upkeep endpoints
- Swordsmen/halberdiers/knights counter triangle and attritional field battles
- Playable continuous field battles with per-formation hold, advance, flank, withdrawal, or captain-control orders
- Named England destinations, calendar-costed travel, a moving tournament circuit, and persistent per-castle conquest
- Persistent hostile field garrisons, 80-shilling spy reports, 98% stronghold interception, route fallback, and siege approach gating
- 50% harvest loan, church donations, recruiting, taxation, wealth and army upkeep
- Blacksmith purchases, automatic equipment, and the original 75% resale rule
- Three-joust/one-skirmish tournament limits, selectable ladies, individual eligibility rules, multi-stage reward ladders, and marriage
- First-person castle simulation with facing and movement, doors and secret rooms, melee reach, crossbows and ammunition, enemy pursuit, champions, armor checks, commandable retainers, food, treasure, breakable weapons, radar, retreat losses, and progression
- Equipment catalog with the original shop prices and armor-bar values
- Crown victory by conquering London's real garrison, dragon victory route, campaign defeat, journal, save/load

## Verification

On Windows, double-click `Run Tests.bat`. It first enforces the repository legal boundary, then builds into a unique directory under `%TEMP%`, disables the shared compiler, and runs both the xUnit suite and the broader executable specifications without touching the game's normal output files. This prevents a running game from locking test build outputs. The same sequence runs in GitHub Actions.

The legal-boundary policy rejects tracked `UserContent`, tracked `analysis/original` material, restricted original-media extensions outside explicitly approved clean-room or synthetic-fixture roots, and unreviewed files over 1 MiB. Its definitions live in `tools/repository-policy.json`; `tools/Verify-Repository.ps1` is the generic interpreter.

The xUnit project uses xUnit.net v3 4.0.0, and the desktop project uses MonoGame 3.8.5.1—the latest stable NuGet releases checked on 2026-09-07.

Both test paths check resource boundaries, externally documented balance values, and core campaign invariants without requiring a graphics device.
It reports every `PASS`/`FAIL`, prints one compact summary, and returns a normal nonzero process exit code on failure rather than throwing an application exception.
Unexpected fixture or parser errors are caught by the runner and reported as a concise failed check. The import and inspection utilities use the same clean command-line error boundary.

## Inspecting the owned original

The read-only `tools/Conqueror.Inspect` utility inventories the mixed-mode GOG CD image, extracts selected executables/configuration files to an ignored local directory, records hashes, and searches binary strings by byte offset. See [`docs/original-analysis.md`](docs/original-analysis.md) for usage and [`docs/original-findings.md`](docs/original-findings.md) for reviewed learnings with explicit confidence grades. Generated proprietary artifacts are never tracked.

## Original media

The project is playable without copyrighted media. When locally imported from an owned GOG installation, original screens, CD audio, and directly decoded Smacker title/credits/item movies are activated from ignored user content. `C1086.GOB`, the raw CD tracks, Smacker movies, and CD audio are intentionally not copied into this repository. See `docs/fidelity.md` for the fidelity ledger and remaining audiovisual work.

The complete phased backlog and acceptance criteria are tracked in [`docs/implementation-plan.md`](docs/implementation-plan.md).
