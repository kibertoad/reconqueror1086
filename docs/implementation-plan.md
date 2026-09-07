# Conqueror: A.D. 1086 implementation plan

## Objective

Complete a clean-room MonoGame reimplementation of *Conqueror: A.D. 1086* that preserves the original campaign features and balance while remaining playable without proprietary media. Owners of a legal copy may use the local resource installer to enable original dialogue, artwork, animation, speech, sound effects, and music. No original asset may be committed to or distributed with this repository.

## Delivery principles

- Keep gameplay rules in typed definitions and generic interpreters rather than screen-specific conditionals.
- Keep simulation code independent of MonoGame so it remains deterministic and testable without a graphics device.
- Treat `docs/original-findings.md` as the evidence register for claims about the original.
- Label every recovered value as Confirmed, Corroborated, or Provisional.
- Do not promote a provisional value without executable/resource evidence, controlled observation, or corroborating documentation.
- Store imported resources only under Git-ignored `UserContent` and validate every manifest path and source hash.
- End every phase with a playable build, clean save migration, and passing automated tests.

## Current baseline

The repository currently provides:

- A MonoGame desktop application and data-driven campaign core.
- Character templates, representative youth dilemmas, economy, construction counters, army recruitment, travel, tournaments, courtship, equipment, field battles, sieges, crown victory, dragon victory, and the age limit.
- Persistent strategic garrisons, spying, interception, retreat, conquest, and JSON saves.
- A read-only original-disc inspector with ISO inventory, hashes, and printable-string searches.
- An end-user importer for raw original resources and lossless CDDA WAV conversion.
- A runtime imported-content catalog and CD music playback.
- Sixty-two executable specifications passing without a graphics device.

## Dependency map

```text
Resource container decoding
  +-- dialogue database ---> village/inn, courtship, orders, dragon quests
  +-- image decoding ------> original screen presentation and portraits
  +-- metadata tables -----> exact economy, opponents, map, loot and combat

SMK decoding -------------> cinematics, jousting, melee and endings

Exact world/economy data --> multi-fief simulation --> political missions

Exact combat tables -------> field battle + siege fidelity --> final balance pass

Importer hardening --------> end-user original-media mode --> packaged release
```

## Phase 1: Evidence and resource foundation

### 1.1 Sierra/Dynamix archive decoder

Implement bounded, read-only parsers for the resource formats used by this release.

Deliverables:

- Identify the exact `C1086.GOB`/`.RES` container variant.
- Parse directory entries, names, offsets, compressed sizes, decompressed sizes, and compression kinds.
- Implement supported RLE, LZW, and LH1/LZSS-family decompression as required by observed entries.
- Extract `.CSF`, `.PCC`, `.PCX`, `.LOW`, sound, palette, and nested resource entries.
- Reject corrupt lengths, overlapping extents, path traversal, decompression bombs, and unsupported variants cleanly.
- Extend `Conqueror.Inspect` with resource listings, targeted extraction, typed metadata, and evidence offsets.
- Extend `Conqueror.Import` to install decoded resources alongside untouched originals.

Acceptance criteria:

- Every archive entry is inventoried deterministically.
- Known entries reproduce stable hashes across repeated extraction.
- Synthetic malformed archives cannot write outside the selected output root or allocate unbounded memory.
- No extracted data is tracked by Git.

### 1.2 Dialogue extraction

Deliverables:

- Decode text encoding, speaker identifiers, conversation nodes, choices, conditions, and resource references.
- Generate a local, versioned dialogue manifest keyed by stable original identifiers.
- Add a runtime dialogue repository with built-in fallback summaries.
- Record short factual findings and confidence grades without committing complete copyrighted dialogue.

Acceptance criteria:

- Imported mode can display complete original dialogue from local files.
- Default mode remains fully playable without imported dialogue.
- Long original text never appears in tracked source, tests, snapshots, or logs.

### 1.3 Image and palette decoding

Deliverables:

- Decode the observed PCX/PCC/LOW/palette variants into runtime textures or locally generated PNG files.
- Preserve indexed palettes, transparency keys, dimensions, frame origins, and animation metadata.
- Add asset-role mappings for portraits, maps, rooms, inventory, cursors, and combat sprites.

Acceptance criteria:

- Pixel hashes or rendered comparisons validate representative assets.
- Imported textures load through the manifest without the MonoGame content pipeline.
- Missing or unsupported resources fall back without crashing.

### 1.4 Smacker playback

Deliverables:

- Select and document a legally redistributable SMK decoder strategy.
- Support frame timing, seeking, palette changes, embedded audio, skip input, and end-of-stream handling.
- Map original movie identifiers to scenes and events.
- Avoid loading complete long movies into memory.

Acceptance criteria:

- Representative SMK files play with synchronized video and audio.
- Playback remains stable at the game's target resolution and during skip/scene transitions.
- The repository contains decoder code/dependencies only, never original movies.

## Phase 2: Original-data recovery

### 2.1 Static executable analysis

Deliverables:

- Identify compiler/runtime, executable segments, overlays, relocation information, and useful symbol/string references.
- Add bounded table scanners and structured hex reports to `Conqueror.Inspect`.
- Locate candidate tables for buildings, unit prices/upkeep, equipment, opponents, wagers, map locations, garrisons, loot, and combat.
- Record code/data addresses, interpretation rationale, release hashes, and confidence grades.

### 2.2 Controlled behavioral observation

Deliverables:

- Define repeatable DOSBox scenarios for uncertain formulas.
- Capture inputs, initial saves, random-seed assumptions, outcomes, and sample sizes.
- Build small analysis commands for distributions and candidate-formula comparison.
- Keep original saves/captures local and commit only derived facts and test fixtures that contain no proprietary content.

### 2.3 Definition promotion

For each recovered table:

1. Update `docs/original-findings.md`.
2. Change the relevant typed definition.
3. Add an executable specification through the real interpreter.
4. Update `docs/fidelity.md`.
5. Remove or narrow the corresponding provisional statement.

Acceptance criteria:

- No value is described as exact without traceable evidence.
- Every exact balance value used by the simulation has a validation test.

## Phase 3: World, estates, and economy

### 3.1 Exact campaign map

Deliverables:

- Recover original destination names, coordinates, ownership, roads, terrain, travel rates, villages, garrisons, and tournament circuit.
- Replace provisional `WorldLocation` data.
- Support terrain- or route-dependent travel if confirmed.
- Render imported original map art when available.

Acceptance criteria:

- Known trips consume the original number of days.
- Tournament placement matches the original calendar for a multi-year fixture.
- Map selection, travel, save/load, and interception remain deterministic.

### 3.2 Buildable fief grid

Deliverables:

- Add terrain tiles, clear-land actions, roads, construction footprints, prerequisites, demolition, and occupancy.
- Define every confirmed farm, village, forest, castle, religious, storage, transport, and civic structure.
- Route construction menus and rendering through definitions.
- Preserve the existing aggregate-save fields through migration or replace them deterministically.

Acceptance criteria:

- Invalid placement is rejected with an original-compatible reason.
- Labor, costs, productivity, capacity, and income derive from placed structures.
- Representative fief layouts survive save/load exactly.

### 3.3 Multi-estate ownership

Deliverables:

- Model each conquered fort/castle and its villages separately.
- Track population, damage, garrison, production, revenue, expenses, and management eligibility per estate.
- Implement political and economic overview pages.
- Confirm which holdings can grow or be directly managed.

Acceptance criteria:

- Conquest creates the correct estate records without duplicating the home fief.
- Monthly settlement explains every income and expense by estate.
- Losing or damaging a holding updates political and economic summaries.

### 3.4 Exact economy pass

Deliverables:

- Recover intermediate productivity curves, construction costs, population bands, harvest rules, upkeep, taxation, and capacity penalties.
- Add an auditable monthly settlement breakdown.
- Verify July bean, housing, staffing, harvest, and debt behavior.

Acceptance criteria:

- Golden campaign fixtures match original month-end balances and populations.
- No economic coefficient remains embedded in UI or event code.

## Phase 4: People, dialogue, and quests

### 4.1 Complete character creation

Deliverables:

- Recover every premade character, randomization rule, starting item, and starting wealth rule.
- Recover the full youth-dilemma pools, conditional checks, success/failure branches, rewards, and stat changes.
- Implement reroll and selection presentation matching the original flow.

### 4.2 Village, inn, and rumors

Deliverables:

- Implement the complete patron roster, biographies, rumors, paid information, shops, church interactions, and location-specific availability.
- Add dialogue conditions driven by campaign flags and definitions.
- Model regional blacksmith inventory if confirmed.

### 4.3 Courtship and marriage

Deliverables:

- Recover complete conversations, visit progression, poetry, eligibility conditions, gifts, rewards, simultaneous courtships, and marriage effects.
- Implement Anna Lisa's father/wealth/dragon-lair branch and all other confirmed quest dependencies.
- Replace shortcut reward counters with dialogue/event state while migrating existing saves.

### 4.4 Orders and political missions

Deliverables:

- Implement orders from the overlord and King William.
- Add brigand suppression, assigned castle attacks, deadlines, completion, refusal, and failure consequences.
- Show current orders on map and overview pages.

Acceptance criteria for Phase 4:

- Major dialogue trees can be completed in both fallback and imported modes.
- Quest flags survive save/load and cannot be advanced by unrelated actions.
- Every reward is issued at most once.

## Phase 5: Tournament fidelity

### 5.1 Exact opponent definitions

Deliverables:

- Recover full opponent identities, records, wagers, joust characteristics, and melee armies.
- Replace provisional `20/35/50/65/80` assignments, tolerances, and unit mixes.
- Track tournament earnings separately from returned stakes.

### 5.2 First-person jousting

Deliverables:

- Implement lance movement, inertia/oscillation, approach timing, target regions, collision, misses, falls, and crash outcomes.
- Apply opponent-specific difficulty and experience effects.
- Integrate original animation, sound, speech, and portraits when installed.
- Preserve three-joust limits and courtship-color rewards.

### 5.3 Tournament melee

Deliverables:

- Present all confirmed opponent choices and wagers.
- Launch the tactical field-battle system with the correct eight-man forces.
- Apply original sword experience, injuries, earnings, and once-per-tournament limit.

Acceptance criteria:

- Stake settlement is correct for win, loss, refusal, and unavailable events.
- Controlled inputs reproduce expected hit/miss regions and opponent difficulty ordering.
- Tournament state survives save/load without duplicating wagers or rewards.

## Phase 6: Strategic warfare and field battles

### 6.1 Strategic campaign rules

Deliverables:

- Verify interception probability and when interception is bypassed.
- Recover reinforcement, recovery, raid, surrender, retreat, pursuit, and territorial-transfer rules.
- Add village raids and enemy attacks where confirmed.
- Persist partial enemy losses and mission-specific forces.

### 6.2 Battlefield terrain and formations

Deliverables:

- Recover original battlefield dimensions, terrain types, deployment, movement, facing, formation shapes, and camera behavior.
- Implement terrain movement/defense modifiers through definitions.
- Reproduce hold, advance, flank, captain-control, and withdrawal behavior.

### 6.3 Combat and morale balance

Deliverables:

- Recover exact counter bonuses, attack cadence, casualty calculations, morale changes, captain effects, and withdrawal pressure.
- Add animated feedback without coupling simulation timing to frame rate.
- Run deterministic balance simulations across large seed sets.

Acceptance criteria:

- Unit counter relationships and quantitative outcomes match controlled original observations.
- Battle results are deterministic for a given state, command sequence, and seed.
- No battle can remain permanently unresolved under captain control.

## Phase 7: Castle assaults and loot

### 7.1 Castle-specific layouts

Deliverables:

- Recover each fort/castle map, entrances, doors, secret passages, collision, enemy spawns, treasure, food, and exits.
- Replace generic generated keeps with data definitions and imported maps.

### 7.2 First-person combat

Deliverables:

- Recover weapon reach, damage, timing, stamina, dexterity, armor, shields, crossbows, ammunition, breakage, healing, and champion rules.
- Implement enemy behavior and collision faithful to the original.
- Add incapacitation, death, capture, and retreat consequences where confirmed.

### 7.3 Retainers and command UI

Deliverables:

- Recover retainer count scaling, formations, commands, pathfinding, survival, and post-battle army losses.
- Expose all commands and status through the first-person UI.

### 7.4 Loot and conquest

Deliverables:

- Recover castle-specific item and wealth tables.
- Track conquest spoils separately in the economics overview.
- Apply correct estate transfer, fame, experience, strength, and victory effects.

Acceptance criteria:

- Every castle is completable without debug tools.
- Item uniqueness and treasure collection persist correctly.
- London follows the confirmed crown route and cannot award victory twice.

## Phase 8: Moneylender and special combat

### 8.1 Drogo encounter

Deliverables:

- Trigger the enforcer encounter under confirmed unpaid-debt conditions.
- Reuse first-person melee with Drogo-specific definitions and consequences.
- Determine whether the annual recurrence is an original bug and expose a documented compatibility decision.

### 8.2 Dragon cave and alternate quest paths

Deliverables:

- Implement lair discovery, absent-dragon plunder, rewards, visits, and associated marriage/rumor branches.
- Prevent premature access without the required clue when confirmed.

### 8.3 Dragon battle

Deliverables:

- Replace the requirements check with the original first-person lance sequence.
- Implement target movement, lance oscillation, eye hit detection, equipment requirements, strength effects, damage/failure, and victory.
- Integrate imported audiovisual resources and ending sequence.

Acceptance criteria:

- Crown and dragon campaigns are both playable from character creation to ending.
- Required items and clue chains are neither bypassed nor consumed incorrectly.

## Phase 9: Presentation, controls, and accessibility

### 9.1 Original screen adapters

Deliverables:

- Map original backgrounds, portraits, heraldry, cursors, icons, transitions, and cinematics to runtime scenes.
- Preserve a clean-room fallback skin for installations without original media.
- Match original aspect ratio and confirmed `WAR_MODE` behavior while supporting modern scaling.

### 9.2 Input

Deliverables:

- Add mouse hotspots matching original screen interactions.
- Add remappable keyboard and controller bindings.
- Ensure jousting, field battles, and siege controls remain responsive at variable frame rates.

### 9.3 Settings and accessibility

Deliverables:

- Fullscreen/windowed mode, integer scaling, music/speech/effects volume, subtitle controls, pause, and reduced-motion options.
- Readable fallback text, keyboard-only navigation, visible focus, and configurable timing where it does not alter simulation balance.

## Phase 10: Saves, importer, and distribution

### 10.1 Save robustness

Deliverables:

- Add explicit save schema versions and migrations for every prior public schema.
- Add multiple slots, autosave, atomic writes, backups, corruption detection, and recovery.
- Investigate an original-save importer without committing original save files.

### 10.2 Importer hardening

Deliverables:

- Detect common GOG installation paths while retaining explicit path selection.
- Show required/free disk space, progress, current file, and a final format-support summary.
- Make installation incremental, resumable, hash-verified, and idempotent.
- Add repair, update, verify, and clean-uninstall commands limited to `UserContent`.
- Detect supported release hashes and warn clearly about unknown variants.
- Never delete or modify the source installation.

### 10.3 Packaging and continuous integration

Deliverables:

- Produce self-contained Windows builds that do not require the .NET SDK.
- Package the importer alongside the game without proprietary data.
- Add CI restore, build, specifications, importer fixture tests, and smoke launch.
- Add clean-room contribution guidance, third-party notices, and release checklists.

Acceptance criteria:

- A clean machine can launch fallback mode from a packaged build.
- A legal owner can install resources, verify them, launch imported mode, and uninstall only generated content.
- CI proves that no prohibited asset extensions or oversized unapproved binaries enter tracked history.

## Cross-cutting test plan

### Unit and parser tests

- ISO-9660 traversal, cue parsing, CDDA track boundaries, WAV headers, manifest hashing, and safe paths.
- Archive directories, every compression variant, malformed entries, and decompression limits.
- Dialogue graphs, quest preconditions, definition validation, and save migrations.

All parser fixtures must be synthetic or independently authored; never commit excerpts extracted from the original.

### Simulation tests

- Golden monthly settlements and population changes.
- Tournament wager and reward state machines.
- Strategic interception, retreat, reinforcement, conquest, and mission deadlines.
- Field/siege/dragon deterministic command sequences.
- Statistical comparisons for rules that are genuinely random.

### Runtime tests

- Headless catalog discovery and fallback behavior.
- MonoGame smoke launch with no imported assets and with a synthetic manifest.
- Render checks for every major screen at supported resolutions.
- Audio lifecycle, scene changes, missing/corrupt files, and device loss.

### Legal-boundary tests

- Fail CI if tracked files match proprietary extensions such as `.GOB`, `.SMK`, imported `.RES`, extracted `.CSF`, or CDDA `.WAV` outside approved synthetic fixtures.
- Fail CI if `UserContent` or `analysis/original` generated outputs are tracked.
- Review new binary files and unusually large files explicitly.

## Suggested milestone order

| Milestone | Phases | Playable result |
| --- | --- | --- |
| M1: Decoded content | 1-2 | Local original dialogue/images can be inspected and loaded with traceable confidence. |
| M2: Feudal simulation | 3-4 | Exact map, buildable home fief, multiple estates, NPCs, quests, and political orders. |
| M3: Knightly competition | 5 | Full wagered jousting and tournament melee. |
| M4: Conquest | 6-7 | Faithful strategic, field, and castle warfare with original layouts and loot. |
| M5: Two endings | 8 | Complete Drogo, dragon cave, dragon fight, crown route, and endings. |
| M6: Original presentation | 9 | Imported audiovisual presentation plus accessible fallback mode. |
| M7: Release | 10 | Robust saves/importer, CI, and self-contained Windows distribution. |

## Immediate next sprint

- [x] Move shared ISO/cue/CDDA/manifest primitives out of the inspector executable into `Conqueror.Resources`.
- [x] Add synthetic ISO, cue, WAV, archive, manifest, and path-safety tests.
- [x] Reverse-engineer and bounds-check the top-level `C1086.GOB`/scene-RES directory structure.
- [x] Inventory stored versus compressed entries without bulk decompression.
- [ ] Identify and decode one uncompressed text resource end to end.
- [ ] Decode one image/palette resource into runtime pixels end to end.
- [x] Add current findings with evidence and confidence grades.
- [x] Make the importer install byte-stored entries and expose them through `ImportedContentCatalog`.
- [ ] Prove and implement the compression method used by unequal-size entries.

The resource-decoding sprint is first because it unlocks exact dialogue, screen mappings, opponent identities, construction data, and balance tables needed by most later phases.
