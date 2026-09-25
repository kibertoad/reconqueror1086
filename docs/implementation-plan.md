# Conqueror: A.D. 1086 implementation plan

## Objective

Complete a clean-room MonoGame reimplementation of *Conqueror: A.D. 1086* that preserves the original campaign features and balance while requiring a verified local extraction from the supported official GOG release. The project never distributes original assets: a legal owner supplies them through the local resource importer, and startup fails clearly when they are missing, incomplete, damaged, or unsupported.

## Delivery principles

- Keep gameplay rules in typed definitions and generic interpreters rather than screen-specific conditionals.
- Keep simulation code independent of MonoGame so it remains deterministic and testable without a graphics device.
- Treat `docs/original-findings.md` as the evidence register for claims about the original.
- Maintain `docs/resource-formats.md` as the byte-level technical specification for every inspected container, compression stream, and decoded resource family.
- Label every recovered value as Confirmed, Corroborated, or Provisional.
- Do not promote a provisional value without executable/resource evidence, controlled observation, or corroborating documentation.
- Store imported resources only under Git-ignored `UserContent` and validate every manifest path and source hash.
- End every phase with a playable build, clean save migration, and passing automated tests.
- Keep every compiled C# source file at or below the 1,000-line ceiling enforced by `Directory.Build.targets`; split responsibilities before they exceed the bound.

## Evidence sources

- The owned installation, decoded resources, executable control flow, and repeatable controlled observations are authoritative for this exact release.
- The [GameFAQs Conqueror 1086 A.D. FAQ by mikel123456](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730) is a useful secondary source for game mechanics, screen flow, strategies, and general information. Treat its claims as Corroborated until resource, executable, manual, or controlled-observation evidence confirms them; do not use it alone to label exact formulas or numeric tables Confirmed.
- Maintain the FAQ consistency audit in `docs/original-findings.md`. Review it whenever provisional gameplay is added or promoted, and preserve disagreements as explicit executable-analysis targets rather than silently choosing either behavior.

## Current baseline

Strategic temporary-force contact recovery: executable `0x3B0E4` scans active patrol descriptors in slot order, then the six player records, before the player movement pass. Both axis differences must be strictly below 30. Avatar record 5 uses a 120-pass notice cooldown and continues the route; army records enter the shared six-counter resolver with direct force counters and caller-specific result settlement. The rules are RULE-STRATEGY-017. The fixed map pass now performs pre-route contact, emits the avatar notice, and hands army encounters to the direct six-counter tactical/automatic resolver and source-variable settlement.

Strategic temporary-force producers: decoded `all.cbf/all.cif` conversation responses and `all.tmb/all.tmi` groups identify Albert's accepted Scottish campaign as scope-zero variable 43 and Valletta's accepted Welsh task as variable 93. The executable UI polls those same nonzero values in that order and the main pass invokes the corresponding `scot.rat` and `wales.rat` creators. The desktop fixed map pass now forwards the persisted conversation variables into those source-backed creator actions, with active-descriptor replay suppression. Patrol encounter resolution now uses the mapped direct counters; original conversation availability remains open. Details and confidence grades are in `docs/original-findings.md`.

Hostile combat mode-9 update: supported kinds 2/4/7 use same-side authored-order ray acquisition. Success selects mode 6, failure selects no-effect mode 1, and the thinker invokes only that selected mode's handler. The runtime now implements both edges; forced-state tests cover templates 3, 5, and 9. None of the official supported placements starts in mode 9, and ordinary-play entry remains **Provisional**. The transition and handler fixups are recorded in `docs/original-findings.md`.

Friendly combat mode-9 update: both kind tables send mode 9 through the authored-order same-side ray, retain mode 9 on success, or enter mode 6 on failure. The current handler shares mode 10's scaled non-cardinal escape motion. Both states are now implemented and tested in forced-state conditions; no supported placed actor starts in mode 9, and an ordinary-play inbound edge remains **Provisional**. The executable fixups and runtime types are mapped in `docs/original-findings.md`.

Strategic tactical cadence correction: setup `0x183E8` registers counter callback `0x183C0` at 250 Hz. Helper `0x18420` multiplies that counter by four, and resolver `0x26C3A` tests a strict 200-unit threshold. The default host timer now quantizes to four-ms steps, so a pass first becomes eligible at 204 ms from an aligned sample and a delayed pass resets its baseline without catch-up. This registration and nominal cadence are **Confirmed** from executable control flow; hardware phase remains **Provisional**. See the address-level map in `docs/original-findings.md`.

Strategic tactical input continuation: queue pop `0x63098` and classifier `0x63114` now map the original 20-byte pointer event to eight press/release codes. The resolver's primary selection/control and secondary destination routes receive those identities from live host button transitions. The executable initializes both hold/repeat thresholds to four timer units; the host now advances their timestamp through the recovered IRQ0-to-INT 1Ch accumulator, nominally reaching four units at 220 ms from an aligned zero phase. The bounded host queue now retains every same-callback edge in low-bit order with its event-time coordinates, consuming one per stable update; tests cover strict repeat boundaries, independent buttons, simultaneous edges, coordinate snapshots, and the 30-entry cap. This replaces the earlier screen-location-based synthetic code selection.

Dragon encounter continuation: the run follows RULE-JOUST-001 and RULE-JOUST-003 with one pass per movie frame (DEV-JOUST-001). The event classifier's wall-clock throughput and cursor presentation remain to recover.

The repository currently provides:

- A MonoGame desktop application and data-driven campaign core that requires verified owner-imported official assets.
- Character templates, the executable-confirmed original 30-dilemma selection/outcome interpreter, economy, construction counters, army recruitment, travel, tournaments, courtship, equipment, field battles, sieges, crown victory, dragon victory, and the age limit.
- Persistent strategic garrisons, spying, interception, retreat, conquest, and JSON saves.
- A read-only original-disc inspector with ISO inventory, hashes, executable-string offsets, archive/compression reports, CSF previews, and HAT layout reports.
- Bounded parsers for GOB/RES directories, kind-1 LZ/RLE blocks, indexed PCX/PCC images, headerless indexed screen planes, CSF animation frames, raw RGB palettes, HAT screen descriptors, and rate-tagged `.666` sound banks.
- An end-user importer that installs byte-stored, kind-1, and kind-2 owned resources plus lossless CDDA WAV files under ignored local storage.
- A runtime imported-content catalog, CD music playback, and definition-driven original art for the title, options hub, character options, pre-generated characters, campaign briefing, animated youth dilemmas, estate/travel shell, England map, load screen, Home/farm/blacksmith flows, and a tournament portrait.
- An executable-confirmed `TITLE.HAT`/`FFTITLE.PCX` title load followed by a corroborated character-options flow whose exact geometry comes from installed `CGOPTS.HAT` and `PREGEN.HAT`.
- Self-contained Windows x64, Linux x64, macOS arm64, and macOS x64 packaging automation, with a smart ownership-aware Windows installer and a pinned four-artifact release workflow.
- A repository-wide 1,000-line compiled-source ceiling; the game shell and resource regression suite are split into focused partial modules so the limit passes without exemptions.
- 540 xUnit test cases and 146 broader executable specifications passing without a graphics device through the isolated Windows test launcher.

Practice joust: the replacement follows RULE-JOUST-001 and RULE-JOUST-002, advancing once per movie frame (DEV-JOUST-001); parity/JOUST.md lists what remains.

## Dependency map

```text
Resource container decoding
  +-- dialogue database ---> village/inn, courtship, orders, dragon quests
  +-- image decoding ------> original screen presentation and portraits
  +-- metadata tables -----> exact economy, opponents, map, loot and combat

SMK decoding -------------> cinematics, jousting, melee and endings

Exact world/economy data --> multi-fief simulation --> political missions

Exact combat tables -------> field battle + siege fidelity --> final balance pass

Importer hardening --------> required verified official assets --> packaged release
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

### 1.1a Technical format documentation

Maintain a reviewable clean-room specification alongside the implementation rather than leaving format knowledge implicit in decoder code.

Deliverables:

- Document byte layouts, endianness, offsets, length semantics, compression framing, known signatures, and validation rules in `docs/resource-formats.md`.
- Record sample-population totals and the exact hashed release to which each observation applies.
- Label every field and codec claim Confirmed, Corroborated, Provisional, or Disproved.
- Link format claims to the inspector report that reproduces them and to synthetic specifications covering malformed input.
- Update the specification whenever support is added for a new archive, image, palette, dialogue, audio, animation, save, or metadata structure.
- Preserve unsuccessful hypotheses when they prevent future contributors from repeating the same false identification.

Acceptance criteria:

- A contributor can implement an independent parser from the documentation without consulting proprietary files.
- Every implemented field has a documented bounds rule and confidence grade.
- Generated reports and original bytes remain ignored; only compact facts, layouts, hashes, and independently authored fixtures are tracked.
- Format documentation and decoder changes are reviewed and committed together.

### 1.2 Dialogue extraction

Deliverables:

- Decode text encoding, speaker identifiers, conversation nodes, choices, conditions, and resource references.
- Generate a local, versioned dialogue manifest keyed by stable original identifiers.
- Add a runtime dialogue repository backed by the required local official-asset extraction.
- Record short factual findings and confidence grades without committing complete copyrighted dialogue.

Acceptance criteria:

- The runtime displays complete original dialogue from verified local files.
- Startup rejects an extraction that lacks the required dialogue database or youth-dilemma resources.
- Long original text never appears in tracked source, tests, snapshots, or logs.

### 1.3 Image and palette decoding

Deliverables:

- Decode the observed PCX/PCC/palette variants into runtime textures or locally generated PNG files; treat `.LOW` files as the confirmed Dynamix scene archives they are rather than as images.
- Preserve indexed palettes, transparency keys, dimensions, frame origins, and animation metadata.
- Add asset-role mappings for portraits, maps, rooms, inventory, cursors, and combat sprites.

Acceptance criteria:

- Pixel hashes or rendered comparisons validate representative assets.
- Imported textures load through the manifest without the MonoGame content pipeline.
- Missing, damaged, incomplete, or unsupported resources produce a clear startup failure before the graphics loop.

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

- Major dialogue trees can be completed from the verified owner-imported databases.
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

- Retainer count scaling, authored actor materialization, command identities, shell hit regions, selected-or-all dispatch, friendly Defend formation states, and post-battle army losses are recovered; continue with remaining direct-target state integration, pathfinding, and survival behavior.
- Expose all commands and status through the first-person UI.

### 7.4 Loot and conquest

Deliverables:

- Recover castle-specific item and wealth tables.
- Track conquest spoils separately in the economics overview.
- Apply correct estate transfer, fame, experience, strength, and victory effects.

Current progress: bounded `Viewer`, `Scenario`, `Map`, `Blocks`, `Backdrop`, `BackImage`, and `Pal0`-`Pal127` data from the owned scene archives drive first-person geometry, presentation, interaction, and combat. Executable tracing assigns `MELEE00`-`MELEE24` to tournament melee, a random unsuffixed `MELEE0`-`MELEE2` to practice, and common `MELEE0.RES` to campaign castle and London assaults. The original `SKIRMISH.PCX`, `SKIRMISH.PAL`, and 53-frame `SKIRMISH.CSF` supply the viewport shell, wall shading, actors, weapon foregrounds, and hit effects. The desktop runtime requires a fully verified extraction from the supported official GOG release and fails before graphics initialization when any mapped dependency is missing, damaged, incomplete, or unsupported; placeholder and no-media gameplay are outside the supported design. What the rebuild implements of the first-person combat rules is tracked in `parity/ASSAULT.md` and `parity/VIEW.md`.

Acceptance criteria:

- Every castle is completable without debug tools.
- Item uniqueness and treasure collection persist correctly.
- London follows the confirmed crown route and cannot award victory twice.

## Phase 8: Moneylender and special combat

### 8.1 Drogo encounter

Deliverables:

- Trigger the enforcer encounter under confirmed unpaid-debt conditions.
- Reuse first-person melee with Drogo-specific definitions and consequences.
- Preserve the confirmed annual July recurrence while debt remains.

### 8.2 Dragon cave and alternate quest paths

Deliverables:

- Implement lair discovery, direct arrival-to-encounter flow, rewards, visits, and associated marriage/rumor branches. Decoded dialogue attributes absent-dragon plunder only to Sir Frederick; do not invent a player plunder loop unless stronger executable or controlled-observation evidence appears.
- Prevent premature access without the required clue when confirmed.

### 8.3 Dragon battle

Deliverables:

- Replace the requirements check with the original first-person lance sequence.
- Implement target movement, lance oscillation, the live item/lance-experience score, strict per-axis success, damage/failure, and victory. The former independent strength/full-equipment gate is Disproved by the executable route.
- Integrate imported audiovisual resources and ending sequence.

Acceptance criteria:

- Crown and dragon campaigns are both playable from character creation to ending.
- Required items and clue chains are neither bypassed nor consumed incorrectly.

## Phase 9: Presentation, controls, and accessibility

### 9.1 Original screen adapters

Deliverables:

- Map original backgrounds, portraits, heraldry, cursors, icons, transitions, and cinematics to runtime scenes.
- Require verified locally imported official presentation assets and fail clearly when any mapped dependency is unavailable.
- Match original aspect ratio and confirmed `WAR_MODE` behavior while supporting modern scaling.
- Apply the owner-supplied dilemma, estate/travel, options-hub, village, Home/farm, and blacksmith observations and follow-ups recorded in [`screenshot-findings.md`](screenshot-findings.md).
- Correct the startup flow to include the `OPTFIN.PCX` options hub, and separate the estate/fief presentation from the England-map navigation role.

### 9.2 Input

Deliverables:

- Add mouse hotspots matching original screen interactions.
- Add remappable keyboard and controller bindings.
- Ensure jousting, field battles, and siege controls remain responsive at variable frame rates.

### 9.3 Settings and accessibility

Deliverables:

- Fullscreen/windowed mode, integer scaling, music/speech/effects volume, subtitle controls, pause, and reduced-motion options.
- Readable text, keyboard-only navigation, visible focus, and configurable timing where it does not alter simulation balance.

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

- Produce self-contained Windows x64, Linux x64, macOS arm64, and macOS x64 builds that do not require the .NET SDK.
- Package the importer alongside the game without proprietary data.
- Add cross-platform CI restore, build, specifications, importer fixture tests, publish smoke launches, installer-layout checks, and pinned workflow security checks.
- Add clean-room contribution guidance, third-party notices, and release checklists.

Acceptance criteria:

- A clean machine receives a clear ownership/import diagnostic rather than entering placeholder gameplay.
- A legal owner can install resources, verify them, launch the game, and uninstall only generated content.
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

- Required-catalog validation for missing, incomplete, damaged, and unsupported imports.
- MonoGame platform smoke launch with a verified official extraction; CLI-only binary smoke remains asset-independent.
- Render checks for every major screen at supported resolutions.
- Audio lifecycle, scene changes, missing/corrupt files, and device loss.

### Legal-boundary tests

- Fail CI if tracked files match proprietary extensions such as `.GOB`, `.SMK`, imported `.RES`, extracted `.CSF`, or CDDA `.WAV` outside approved synthetic fixtures.
- Fail CI if `UserContent` or `analysis/original` generated outputs are tracked.
- Review new binary files and unusually large files explicitly.

## Milestone status

Status is conservative: “prototype” means the route is playable but substantial original behavior or balance remains provisional; only acceptance-criteria completion can mark a milestone complete.

| Milestone | Phases | Status (2026-09-10) | Remaining completion gate |
| --- | --- | --- | --- |
| M1: Decoded content | 1-2 | In progress: archive kinds 1 and 2, all `.RES`/`.LOW` scene archives, raw `TEX` dimensions, PCX/PCC, CSF, palettes, HAT layouts, dilemma text, the 1,311-node `ALL.CIF`/`ALL.CBF` conversation graph, `.666` sound-bank framing, complete owned-release SMK codecs, collision-free installer, and evidence reports work. Scene decoding now includes Viewer/Scenario metadata, maps, typed cardinal face and billboard references, backdrop images, all 128 indexed color-remap tables, and executable-confirmed map generation/selection parameters. | Decode remaining conversation conditions/mutations and scene/text structures, bind remaining audio events, and complete executable table recovery and controlled observations. |
| M2: Feudal simulation | 3-4 | Playable prototype: the original estate shell and typed panel/navigation layout are active, and farm commands/help share one definition registry. | Exact terrain sprites and map/economy data, tile-based fief construction, multiple managed estates, full dialogue/quests, and political orders. |
| M3: Knightly competition | 5 | Playable prototype. | Exact opponent tables plus faithful first-person jousting and tactical tournament melee. |
| M4: Conquest | 6-7 | Playable prototype: the manual-confirmed five-division War Planning roster, company editing, membership, field state, one-live one-report spies, joined-division combat, persisted captain-commanded movement, persisted five-slot autonomous enemy movement with report-before-advance spy binding, and original scene-map/start/spawn ingestion are active. The original movement record layout, strict pre-add 5,000-unit generation clock and roll, bounded 1-15 strategic speed multiplier, complete timed/reactive branch gates, first-free-slot construction and mode fallback, route flags, authored-order eligible-property and pursuit selection, ordinary lord/household force initializer, exact 14-row named property table, linked lord inputs, person-record layout, initial eligible household counts, 90 canonical pair routes, all seven starting-home routes, exact bounded route decoder, all three movement handlers, named calendar terrain profiles, exact 331-byte tile-kind lookup, exact staggered world-to-grid inversion with inclusive camera-ordered edges, two exact 30-entry terrain-speed tables, mode-1 0.9 scale, mode-2 strict displacement gate, mode-3 target/fallback path, five-cell completion probe, and encounter/transfer/retarget precedence are mapped and regression-tested. GOB resource 292 `icon.jp` is confirmed as the 200-by-400 strategic grid and drives the new slot-ordered motion kernel alongside all 97 executable-selected paths; the playable campaign still isolates its daily dated-travel adapter until generation/contact integration is complete. The non-bijective schema-1-to-2 settlement and exact replacement state are specified in `architecture.md`. Campaign assaults use the confirmed common `MELEE0.RES`, while practice randomly selects the three executable-confirmed unsuffixed base scenes and tournament's separate two-digit family is no longer misused. Imported first-person scenes render inside the original combat shell and viewport with cropped connected maps, panoramas, executable-cardinal and distance-shaded wall faces, explicit door state targets, direction-aware actor state templates with imported completion gates, exactly bound equipped-weapon foreground attacks, original per-weapon break rolls, and executable-dispatched fatal/wounding hit effects with occlusion. | Complete the fixed-update scheduler shell around the strategic motion kernel, activate schema 2, and remove the dated adapter; captain battle rules, remaining original strategic rules, status/control rules, exact foreground presentation, combat balance and AI, retainers, and loot. |
| M5: Two endings | 8 | Both routes are playable as prototypes. The dragon moor stays hidden until original global variable 2 is raised by Anna Lisa's decoded secret-lair branch; clean-room mode mirrors that disclosure after her second successful courtship joust, and older saves already at the moor migrate safely. The dated named-location route proceeds from the owned `TRANDRAG.SMK` approach into the timed encounter after lair discovery; the executable five-cell strategic-map trigger enters the dragon worker directly. Lance experience and live equipment set the strict two-axis score threshold. `DRJSTRUN.SMK` plays during the encounter; the worker's zero/nonzero result plays `DRJSTWIN.SMK` or `DRJSTLSE.SMK` respectively. Host withdrawal remains possible and returns to the map without a fabricated outcome movie; its transient trigger suppression lets later player records and hostile scheduling resume until the distinguished record leaves the region. Dragon victory then continues into the executable-confirmed `CHAMPL30.SMK` King's Champion investiture, while taking London plays the confirmed `CROWNL30.SMK` throne ceremony. Reaching age 30 without either victory plays the confirmed `AVG_END.SMK` sequence, distinct from fatal dragon and Drogo outcomes. The imported `DRJSTWIN.PCX` supplies the dragon battle background. The confirmed `0x13328 -> 0x1BB5C -> 0x1B250` dragon route loads the 25-frame `lance1.CSF`, now bound as the live foreground through its recovered frame selector and the recovered local motion/paint origin with stable one-pass-per-frame cadence; host cursor presentation and interaction timing remain to recover. Decoded dialogue's absent-dragon plunder belongs to Sir Frederick's history rather than a supported player mechanic. Imported courtship sessions now defer prize mutations to each lady's decoded action tree rather than applying a synthetic ladder. Unpaid harvest debt now raises a persisted Drogo demand with exact-debt payment or a fatal first-person fight; killing him permanently disables the moneylender; refusal requires the original imported four-hostile `MONEY.RES` scene and its template stats. | Recover original pointer-event timing and cursor presentation, and recover crown politics plus any remaining fatal transitions. |
| M6: Original presentation | 9 | In progress: file-backed direct title/credits/item-movie playback, the confirmed campaign briefing, title fallback, options hub with original state widgets and persistent channel volumes/reduced motion, pause, a centered aspect-correct virtual canvas with windowed/borderless-fullscreen and integer-scaling modes, character screens, estate/map panels, all ten executable-ordered Home hotspots, Overview/War Planning backgrounds and controls, four descriptor-driven fief-management variants, the populated inn with ten named patrons and portraits, all six tournament lady selector conversations, original blacksmith node `3201`, ordinary parish selector `3100`, Cambridge armor-quest selector `3149`, CD audio, presentation descriptors, edge-triggered controller navigation, context-sensitive dialogue/map/shop/tournament/combat actions, and a right-stick virtual pointer for all original hotspots are active. | Exact army-path behavior, exact estate tiles, regional mapping for priest roots `5000`-`5500`, remaining conversation entry points and typed-state bridges, remaining CSF/SMK event bindings and seeking, fonts, cursors, subtitles, and input remapping. |
| M7: Release | 10 | In progress: launch/test/import utilities, five atomic save slots plus a separate transition-triggered autosave with explicit schema migration, previous-generation recovery, and user-visible generation-specific corruption diagnostics; supported-release detection; exact disk-space preflight; progress-reported atomic/resumable content writes; manifest-wide verify/repair and manifest-scoped uninstall commands; a data-driven legal-boundary check; four-platform CI; self-contained Windows/Linux/macOS packages; a manual four-installer release workflow; and an ownership-aware Windows installer with smart GOG discovery, shortcut validation, and automatic extraction are implemented. | Signed/notarized packages, clean-machine verification, third-party notices, and release checklist. |

M5 correction (2026-09-22): the desktop Drogo-refusal route requires and decodes the owned `MONEY.RES` scene, then passes its layout through the layout-required `Campaign.CreateDrogoBattle(SiegeLayout)` with retainers explicitly excluded. Its confirmed scene census has one template-0 player actor and four hostile thug placements (templates 9, 8, 3, and 3), so no invented one-on-one production fallback remains. This uses imported map, collision, actor, backdrop, color-map, texture data, and the exact executable template stats. Executable calendar pass `0x2B060` calls debt branch `0x1074C` only at zero-based month 6 with nonzero debt; both refusal/combat branches resolve object-2 `+0xA4` (`MONEY.RES`) before common first-person setup `0x1070C`. The original moneylender-scene dispatch and annual month-six recurrence while debt remains are therefore Confirmed.

M6 correction (2026-09-22): executable initialization loads `CONFONT.CSF`, the 256-frame proportional 2..10-by-13 ASCII face, into the global byte-text renderer. That renderer advances by each selected frame's stored signed width and supplies a caller-selected palette index to the glyph-mask blitter; `OriginalUiFont` now preserves those relative advances, alpha-mask shapes, and caller color control while fitting the existing host canvas. Recover the active screen-palette mapping and remaining vertical/layout rules plus cursor timing/hourglass behavior; the prior broad M6 “fonts, cursors” item is superseded.

## Schema-2 replacement-state checkpoint: 2026-09-17

`OriginalStrategicCampaignState` provides the non-aliased mutable replacement specified by `architecture.md`: all 14 properties, all 176 people, five exact enemy slots, six exact player records, route identities/cursors, integer and floating movement state, generator/reactive counters, bounded speed, month-derived profile, persisted camera, list heads, and sparse full-dword terrain mutations. `StrategicSchemaTwoMigration.Prepare` implements deterministic slot-ordered schema-1 settlement, returning complete columns only to valid still-hostile origins and journaling all return/dispersal outcomes before clearing the dated roster. Complete JSON round-trip and structural rejection tests pass. The decoded resource provider and both movement shells are now implemented; schema activation still waits for exact player command construction, application fixed-update/presentation wiring, and dated-adapter replacement.

## Strategic motion-kernel checkpoint: 2026-09-17

The decoded provider now drives a mutable physical-slot-order pass for all three original movement modes. `AdvanceMovementPass` reproduces direct mode's crossing test, exact object-2 `+0x4F97 = 0.9` terrain scale, post-step grid refresh, and modulo-byte origin return; routed mode consumes the real forward/reverse cursor with strict tolerance/crossing, integer normalization, prior-direction validation, prospective terrain lookup, kind-9 teardown, and strict object-2 `+0x7389 = 50.0` horizontal application gate; pursuit mode reads live target slot `+0x14`, signals target disappearance, recomputes normalized direction, and switches to direct origin return with one unscaled step on kind 9. Sparse saved terrain mutations override `icon.jp` during the same lookup. Six focused tests cover physical slot order, exact-boundary crossing, byte wrap, route tolerance/completion, corrupt-direction teardown, live pursuit, target disappearance, and fallback. Schema and dated campaign behavior remain unchanged; next integrate generator-first dispatch, completion contacts/encounters, and live player-army targets before activating fixed updates and schema 2.

## Strategic scheduler-shell checkpoint: 2026-09-17

`AdvanceSchedulerPass` now surrounds the motion kernel with the executable's generator-first, first-free-slot construction and per-slot completion interleaving. It implements both timed and reactive branches, ordinary and pursuit constructors, the corrected ordinary force expression `household + floor(floor(lord rating / 4) / 3)`, reactive detachment/support arithmetic, direct grid-person lookup, five-probe contact precedence, encounter handoff, byte-garrison reinforcement, teardown, and exact first-nearby-army/player/origin retargeting. Tests prove a newly generated record can move in the same pass and that an encounter stops later physical slots. The original's uninitialized finder-output read is documented and replaced with an explicit-current-detection safety policy. The next checkpoint supplies these inputs from live campaign state, preserves spy reporting before the pass, wires the scheduler to the fixed update and encounter presentation, activates schema 2, and removes the dated adapter.

Strategic contact handoff correction: executable player pass `0x13168` calls encounter wrapper `0x39428` synchronously before caller `0x3C290` reaches the hostile scheduler. The replacement's interactive resolver opens on a later UI update, so a pass that emits a new player/enemy encounter now defers its hostile scheduler and preserves the captured combatants for the modal's live-snapshot validation. Existing modal and unavailable-fallback guards still apply. The source ordering is Confirmed; the deferred host boundary is a documented compatibility adaptation with a focused regression.

## Optional post-fidelity rendering enhancements

These are future opt-in presentation features, not compatibility requirements and not substitutes for completing the original rendering path. The default compatibility profile must retain the recovered 64-cell ray traversal and its gameplay consequences. In every enhanced profile, AI acquisition, combat range, pointer picking, interaction, and gameplay occlusion must continue to use the original gameplay ray and thresholds.

- Render the original virtual canvas at a higher internal resolution, then offer integer or carefully selected filtered scaling.
- Offer a modestly increased *visual-only* draw distance, capped before the 128x128 wrapping source maps reveal conspicuous repeated geometry or unintended views around the world.
- Add optional atmospheric distance fade or fog to integrate distant geometry and conceal map repetition without changing collision or visibility decisions.
- Permit reduced-detail distant scenery where it improves legibility, while actors and actionable objects remain visible, targetable, and selectable only when accepted by the original gameplay path.

The owned art contains no established distant LOD asset tiers, so any reduced-detail treatment must be derived presentation work and clearly separated from the preservation profile. Evaluate these options only after the faithful renderer is complete, using visual regression captures to ensure the original profile remains unchanged.

## Current resource and startup sprint

Local analysis reference: the complete owned installation is available read-only at `C:\GOG Games\Conqueror AD1086`. This machine-specific path is for inspection/import tooling only; no proprietary file from it may be committed, packaged, or redistributed.

Local static-analysis tool: Ghidra 12.1.3 is installed user-wide at `C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC`, with Temurin JDK 21.0.12.1 at `C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1`. `GHIDRA_HOME`, `JAVA_HOME`, and the user `PATH` contain these locations. Temporary Ghidra projects and derived disassembly must remain outside Git just like other original-binary analysis artifacts. [`ghidra.md`](ghidra.md) documents setup, isolated-Codex environment variables, LE executable handling, and the reproducible headless workflow.

- [x] Move shared ISO/cue/CDDA/manifest primitives out of the inspector executable into `Conqueror.Resources`.
- [x] Add synthetic ISO, cue, WAV, archive, manifest, and path-safety tests.
- [x] Add an xUnit.net v3 4.0.0 suite and an isolated Windows test launcher that cannot collide with a running game's build outputs.
- [x] Reverse-engineer and bounds-check the top-level `C1086.GOB`/scene-RES directory structure.
- [x] Inventory stored versus compressed entries without bulk decompression.
- [x] Identify and decode the marker-delimited `DILEM*.DAT` text resources end to end; all 30 validate through a bounded ASCII parser and are exposed by stable number through the runtime dialogue repository.
- [x] Decode `ALL.CIF`/`ALL.CBF` conversation framing end to end; all 1,311 indexed nodes, 2,062 prompt variants, 2,696 terminal/linked responses, and 63 zero-response continuation slots validate through a bounded graph parser and runtime catalog adapter. Confirm the populated inn's ten selector roots and first named nodes against the owned database.
- [x] Preserve all 423 node-level and 2,318 response-level action references from the fixed conversation headers, with exact 30-slot bounds and metadata-only reporting.
- [x] Decode the indexed `ALL.TMI`/`ALL.TMB` structure with bounded recursive traversal; validate all 689 groups, 2,267 actions, 10,373 expressions, and 13,352 values and report the lone unresolved original reference `5011`.
- [x] Recover action function IDs 3-9, the relocated operator jump table, and the 190-value `ALL.VTB` initializer; execute the confirmed expression/branch grammar, persistent script variables, dialogue redirects, raw item counters, and wealth mutations from original inn conversations.
- [x] Map the original scope-1 selectors for wealth, honor, fame, piety, strength, stamina, and intelligence onto typed campaign fields; map produced/consumed item IDs 0-23 onto their executable/dialogue-confirmed visible inventory names while retaining raw counters for save compatibility and the producer-less original `161` shield-gate outlier.
- [x] Decode one byte-stored image/palette resource into runtime RGBA pixels end to end.
- [x] Decode all five byte-stored CSF indexed-animation sequences into bounded palette indices and alpha masks.
- [x] Validate and classify the five stored 256-color RGB palette resources.
- [x] Add current findings with evidence and confidence grades.
- [x] Make the importer install byte-stored entries and expose them through `ImportedContentCatalog`.
- [x] Prove and implement the kind-1 compression method used by unequal-size entries.
  - [x] Confirm kind-1 block framing across all 471 entries in the hashed GOB.
  - [x] Confirm 16 KiB output slices and the `0x40` compressed/`0x80` verbatim block markers across the GOB and all 50 scene containers.
  - [x] Separate the five kind-2 entries and disprove raw inner-chunk LZW and independently reset classic LH1 for kind 1.
  - [x] Identify the kind-1 LZ/RLE token codec and validate all 20,381 blocks plus 188 decoded PCX-compatible images.
  - [x] Recover kind 2 from the LE executable as MSB-first adaptive 9-to-14-bit LZW; validate exact expansion of all five GOB entries and strict 640x480 PCX decoding of all four image entries.
- [x] Activate decoded `fftitle.pcx` and `engmap1.pcx` through definition-driven title and map roles.
- [x] Correct the startup flow from executable/resource evidence: static `FFTITLE.PCX` title, then interactive `CHAR_OPS.PCX`, with `PREGEN.PCX` and `LOADGAME.PCX` registered as distinct screen roles.
- [x] Decode HAT screen descriptors and use installed `CGOPTS.HAT`/`PREGEN.HAT` geometry at runtime; verified official assets are now a launch requirement.
- [x] Split Home, farm management, blacksmith workshop, and blacksmith inventory into distinct runtime scenes using screenshot-verified `TACTICAL.PCX`, `FIEFMGMT.PCX`, `FORGESMI.PCX`, and `SWDTEMP.PCX` roles.
- [x] Activate screenshot-confirmed `INNPEOPL.PCX`; bind its ten patrons, Exit, and footer through `VINN.HAT`; and use the executable-ordered name catalog plus matching PCC portraits in the shared conversation frame.
- [x] Activate the populated inn's original conversation roots from the startup-decoded database: preload referenced portraits, select prompt variants, render all declared response rows, accept mouse/number selection, execute node/response action programs, follow action redirects plus response and zero-choice continuation edges, and terminate on target zero.
- [x] Replace character-slicing fallback text wrapping with word-boundary wrapping; preserve explicit paragraph breaks and keep a single oversized token intact, matching the original text control's explicit `WORDWRAP` behavior at the level supported by the fixed-width fallback font.
- [x] Decode the 40-record `WEAPONS.DAT` store table and bind its prices, local descriptions, item order, and `SWORDS.CSF` frame indices to typed equipment definitions and the original inventory shell.
- [x] Decode and palette-verify all four `BUYSELL.CSF` overlays, then select blank/View and Sell/Purchase states from store metadata and current ownership through typed presentation definitions.
- [x] Identify `ICONTEMP.PCX`/`ICONMAP.HAT` as the estate/travel shell and activate its exact viewport, inset-map, tab, information, and navigation regions.
- [x] Replace farm input branching and separately maintained help strings with one typed command/action registry.
- [x] Add shared data-driven visual-scene hover labels and confirm the separate Blacksmith/Buy-Sell `VSMITH.HAT` targets. Correct the Home descriptor from `FCASTLE.HAT` to `FOPTS.HAT` and activate its seven visually/evidence-correlated office objects; three ambiguous targets remain disabled.
- [x] Route the Home Castle model and Farm/Village/Forest books into a shared section-aware management screen, import `FCASTLE.HAT`, `FVILLAGE.HAT`, `FFARM.HAT`, and `FFOREST.HAT`, and source each variant's exact terrain/fullscreen region IDs from its descriptor. Row semantics remain deliberately unassigned where evidence is incomplete.
- [x] Activate descriptor-driven fief-management OK/Cancel controls with transactional commit/rollback; prevent pending edits from leaking into saves, and restore economy, development, recruitment, and journal state on cancellation.
- [x] Recover the four contiguous fief-management label catalogs from executable data, render them through each HAT's exact row rectangles, add bounded Village scrolling, activate only supported row mutations, and correct the shared upper-right region from a presumed wealth field to the visible Full Screen control.
- [x] Bind all ten `FOPTS.HAT` Home regions in the executable's contiguous label order, correct War Planning/Orders region identities, activate both exit regions, and register the original `F_OVER.PCX`/`WARPLAN.PCX` destinations with their descriptors.
- [x] Decode the 22-frame `WARPLAN.CSF` state catalog and map its five army selectors, Field Army, Join/Leave, Send Out Spy, unit rows, army name, and footer through `FWARPLAN.HAT`.
- [x] Apply the owned manual's War Planning rules: persist five named divisions, edit 100-serf companies with a 60-company cap and home-territory restriction, stage field/join/spy state transactionally, charge all-division upkeep, preserve the confirmed 80-shilling single-live one-report spy lifetime through the first active slot of a persisted five-record movement roster, and make OK/Cancel commit or restore every pending change. The former monthly report proxy is removed. Preserve the exact named property rows, linked lord inputs, person-record layout, and initial household counts; exact movement scheduling, routes, and runtime index migration remain provisional.
- [x] Decode and classify all 26 `.666` sound banks with bounded length/rate validation; the ignored population report accounts for 102 samples and zero rejects.
- [x] Confirm unsigned 8-bit mono PCM from waveform centering, identify the identical shared UI sample across 17 screen banks, and activate it through a startup cache that decodes each referenced bank and converts each registered sample only once.
- [x] Decode and classify all six `FFMOUSE.CSF` cursor frames, activate contextual travel/talk/target/pressed-hand selection, and use the decoded `OPTION.CSF` held states with same-region press/release activation.
- [x] Activate the original `PRACTICE.PCX`/`PRACTICE.HAT` menu, preserve its executable-ordered War/Joust/Melee/Exit/Castle Skirmish labels, bind the directly decoded `JOUSPRAC.SMK`, and route War versus Melee/Castle Skirmish into isolated non-campaign tactical versus first-person practice sessions. Exact legacy combat presentation and parameters remain provisional.
- [x] Decode the first-person `Viewer`, `Scenario`, 128x128 column-major `Map`, fixed-size named `Blocks`, and paired `Backdrop`/`BackImage` records; use original scene geometry, viewer start/facing, wrapping panorama, interactive block roles, exit/gate boundaries, and defender/champion placements in melee and castle-skirmish sessions.
- [x] Classify solid-block cardinal surface slots separately from kind-4 billboard references and render the contacted wall or door face through the map-aware raycaster. Executable hit masks `0x100/0x200/0x400/0x800` confirm north/east/south/west at block offsets 44/48/52/56; byte-exact color-map regeneration confirms the `SKIRMISH.PAL` association.
- [x] Recover door behavior-bit and state-target semantics (RULE-ASSAULT-021).
- [x] Classify and activate original knight/champion/footman state templates (RULE-ASSAULT-029), import each state's `SFXDEFS` completion gate (RULE-ASSAULT-018), preserve wall-column occlusion, and remove dead actors from collision and combat before their death-state gate completes.
- [x] Decode the complete canonical `Pal0`-`Pal127` scene color-map set as four 32-step indexed-remap families. Decode Scenario count/shift/blend parameters and the per-block offset, regenerate the first family under `SKIRMISH.PAL` with the executable's blend/round/nearest-color rules, and apply its fixed-depth clamp through a bounded texture/map cache. All 90 owned active families match regenerated output byte-for-byte.
- [x] Remove the incorrect 15-castle mapping across tournament's `MELEE00`-`MELEE24` family. Executable tracing assigns the two-digit builder to tournament melee and confirms an inclusive 0-2 roll over unsuffixed `MELEE0`-`MELEE2` for practice. Initialization registers dispatcher `0x21EEC` in engine callback field `+0xB0`; its sole indirect invocation reaches slot 128 and literal `MELEE0.RES`. Practice selection and common campaign use are Confirmed.
- [x] Decode and palette-verify the 53-frame `SKIRMISH.CSF` sequence, classify its weapon, shield, and blood-effect runs, and bind the axe, crossbow, hammer, mace, sword, and dagger sequences to first-person combat (RULE-ASSAULT-026, RULE-ASSAULT-028, RULE-ASSAULT-031).
- [x] Decode the headerless 320x200 `SKIRMISH.PCX` indexed plane with its separate raw palette; use its exact 167x117 combat aperture, message/status panels, command labels, health strip, and radar well. Missing or unsupported official media fails startup; an assetless/generated presentation path is intentionally unsupported. Exact dynamic field contents and mouse-command semantics remain Provisional.
- [x] Preserve mask-83 exits as visible scene boundaries and render every non-actor kind-4 pickup, debris, and weapon-contact object as an occluded billboard (RULE-ASSAULT-005, RULE-ASSAULT-021).
- [ ] Trace the complete startup/menu state machine, input timing, cursor behavior, and region-action dispatch from `CONQUER.EXE`; keep semantics corroborated until each executable branch is confirmed.

The resource-decoding sprint is first because it unlocks exact dialogue, screen mappings, opponent identities, construction data, and balance tables needed by most later phases.

## Next implementation priorities

1. Continue first-person combat recovery: the remaining transition-table entries, the mode value the orders reset to, and the view row the actor rays test (RULE-ASSAULT-004, RULE-ASSAULT-008, RULE-ASSAULT-010). `parity/ASSAULT.md` lists the rest.
   Future-proof the recovered design with a monotonic simulation clock and per-record deadlines. Preserve original interval values, state order, strict threshold, and deadline-overrun behavior in compatibility tests, but keep rendering observational and use bounded update catch-up after stalls. Do not make effect or combat speed depend on processor throughput, render-call count, monitor refresh rate, or wall-clock adjustments; represent the original four-render-call blood lifetime as four explicit presentation/simulation steps once the original render cadence is established.
2. Continue tracing startup/menu input timing and HAT region-action dispatch from `CONQUER.EXE`; all ten Home labels, nine registered Home actions, War Planning controls, independent division movement, and the five-slot report-before-movement spy boundary are active. `JUMP!!` is now closed as an intentionally inactive label-only region. Spy cost, lifetime, slot scan, trigger ordering, all 176 person initialization rows, named property rows, linked lord inputs, initial household counts, strategic speed clock, route resources, terrain grid, and exact camera-ordered world-to-grid inversion are confirmed; schema-2 runtime migration and original input timing remain to be confirmed.
3. [Completed 2026-09-08; policy updated 2026-09-13] Bind the decoded five-definition age groups to campaign state using the executable-confirmed per-age selection policy and keep original prose local. Startup now requires those owner-imported resources rather than supporting built-in summary gameplay.
4. In progress: bind menu CSF sequences to verified palettes and roles. `OPTION.CSF` ON/OFF held/released and Resume frames use `OPTFIN.PCX`; `SWORDS.CSF` item art and `BUYSELL.CSF` control states use `SWDTEMP.PCX`; all six `FFMOUSE.CSF` frames are classified and contextual travel/talk/target/pressed-hand selection is active. Exact cursor timing, hourglass dispatch, and other menu sequences still require evidence.
5. [Completed 2026-09-10] Add legal-boundary automation that fails if imported media or generated analysis artifacts enter Git, then add Windows x64, Linux x64, macOS arm64, and macOS x64 CI restore/build/test/publish coverage plus native installer checks. `tools/Verify-Repository.ps1` interprets the data-only `repository-policy.json`; the local test launcher and every GitHub Actions package workflow enforce it before compiling.
6. In progress: `ICA`/`ICS`/`ICW` are decoded and active as seasonal 337-frame estate atlases using the `ICONTEMP.PCX` palette. Recover the executable's exact terrain/frame table plus roads, shield, cursor, and layout data; current semantic frame assignments remain provisional.
7. In progress: unsigned 8-bit mono PCM is confirmed; all imported samples are eagerly converted into a retained startup cache and the shared 2,159-byte interface sample is active. Trace and bind the remaining 101 sample event identifiers; `VSMITH.666` is audio, not the blacksmith dialogue database.

## Combat rendering notes

Runtime correction (2026-09-18): acquisition traverses the complete wrapped source map, not only the cropped visible layout. Scene activation now retains CPU-decoded sources for every structural face and all kind-4 heading sectors while allocating GPU textures only for the render set. This fixes melee practice failing when a valid off-layout ray candidate selected texture 11; focused closure coverage locks ordinary faces plus mirrored and unmirrored billboards.

Art-path correction (2026-09-18): dependency enumeration now begins with placed map blocks and follows only state-target edges eligible for ray traversal, so unused block templates no longer create false requirements for absent textures 3/4/6. The owned archive census also establishes `CONQUER/DEFEND2.RES` as the complete high-resolution combat actor atlas (texture span 64-138); active scene slots override that shared atlas, supplying complete normalized walk families 64/96/128 without losing scene-specific states. `SiegeTextureDependencies` and `LoadCombatTextureSources` preflight this layered closure. All procedural siege actor, object, wall, backdrop, and shell fallbacks are removed, leaving one supported original-resource rendering path.

Projection/blend correction (2026-09-18): indexed scene and CSF uploads now premultiply transparent pixels for MonoGame, removing the atlas-colored rectangles around actors and foregrounds. Runtime actor/object placement uses the executable's `center + lateral/forward` viewer basis instead of the former 60-degree approximation; actor width, lower/upper elevations, actor-minus-viewer heading sector, and keyboard strike target use the same fixed-depth billboard geometry as pointer acquisition. The 1088x200 `BackImage` is treated as four overlapping 320x200 views at stride 256, and the exact `(26,24,167,117)` combat aperture is selected rather than compressing a 640-pixel strip. Recovered movement timing remains unchanged and processor-independent: strict elapsed-time 200 ms descriptor ticks, a first transition after three ticks, and an 800 ms sustained straight-cell cadence.

Potential higher-powered-machine presentation work should be opt-in and observational: viewport supersampling and filtering can be explored after fidelity completion, while visibility, picking, occlusion, interaction, and AI retain the original 64-step ray. A longer ray is not planned because source lookup wraps in the 128-cell authored map and could reveal duplicated or unauthored scenery.

## Home checkpoint: 2026-09-14 inactive `JUMP!!` region

`FOPTS.HAT` defines ten enabled regions and the executable's adjacent label table includes `JUMP!!` at index 7. Registration routine `0x302A0`-`0x30492`, however, installs action, hover-state, and release callbacks only for indices 0-6 and 8-9. The omission is repeated across all three callback families while label renderer `0x30768`-`0x307D4` can still index the complete table. Therefore region 7 is deliberately visible label-only scenery, not an unrecovered command.

`SceneHotspot.Interactive` now expresses that distinction: Home retains all ten HAT bounds and hover labels, while activation ignores `JUMP!!`. A focused test pins region 7 as the only inactive Home hotspot. This callback membership and runtime mapping are **Confirmed**; the placeholder action and its claim that further executable confirmation was needed are **Disproved**.

## Session checkpoint: 2026-09-10

The current migration branch builds with zero warnings under the enforced 1,000-line source limit. All 329 xUnit cases and 146 executable specifications pass. The Inno Setup 7.1.0 package compiles, installs into an isolated directory, launches through its generated **ReConqueror A.D. 1086** Start-menu shortcut, and removes that shortcut during uninstall. Repository policy passes with the expanded proprietary-image boundary.

First-person combat now uses the required original `SKIRMISH.PCX` shell, exact viewport and panel geometry, scene-driven walls and actors, animated foreground weapons, and blood effects. The corrected DOS/16M mapping places the LE data-page base at raw `0x4C254` and `Pal%d` at object 2 offset `0x7D28`; all 90 owned active palette families reproduce byte-for-byte. Executable paths confirm north/east/south/west surfaces, the complete weapon-to-combat-row permutation, break rolls, dice, penetration, reach, actor templates, state-completion gates, pointer coordinates, and fixed-point foreground trajectory formulas. Record-offset `0x40` replaces contacted objects and doors immediately. Pickup dispatcher `0x51E60` reads the selected scene block's selector at `+0x48`, not the player record; cases 5, 7, 9, and 10 implement +25 wealth, capped 2d6 healing, Chain Hauberk bit 6, and +12 bolts. The original four render-counted blood steps have no stable hardware-independent duration, so the runtime preserves their order through a monotonic compatibility timer. `docs/handover.md` continues with defeated-enemy loot, retainers, siege consequences, and broader AI rules. Official imported assets are a verified launch requirement; no placeholder gameplay path is supported.

## Strategic reactive-finder checkpoint: 2026-09-17

Reactive finder `0x3BB4C` is implemented inside the scheduler rather than supplied by the application. The runtime preserves authored property/player-slot order, strict normal `30/250` and property-7 `40/200` per-axis gates, London's half-open rectangle, exact auxiliary-lord contact, property bytes `+0x0D/+0x0E`, empty-garrison behavior, and one-shot alert output. The full gate passes 362 xUnit cases and 146 specifications.

The six-record player update, command construction, bootstrap, join/leave exchange, and field-record lifecycle are recovered and implemented. `OriginalStrategicPlayerMovementSlot` persists exact route/target, cursor, state-8, grid, cooldown, terrain, position, and direction state for five army records plus avatar slot 5. `AdvancePlayerMovementPass` maps handler `0x11554`; `AdvancePlayerPass` maps `0x13168`; `OriginalStrategicPlayerCommands` maps route input, `0x1133C/0x113A4`, and constructor/removal `0x12CAC/0x12DD0`. `CreateForNewGame` maps `0x110E8`: the selected starting person's assignment clears, all records become complete, only avatar 5 activates at its exact route anchor, both selectors start at 5, and independent constructed-record count `AE5C` starts at zero. Construction preserves the exact six formation offsets and former-selection clearing; removal preserves first-active replacement and avatar restoration. The full gate passes 391 xUnit cases and 146 specifications. Next bind temporary force inputs, fixed updates and presentation, then activate schema 2 and remove the dated adapter.
