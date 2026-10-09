# Conqueror: A.D. 1086 implementation plan

## Objective

Complete a clean-room MonoGame reimplementation of *Conqueror: A.D. 1086* that preserves the original campaign features and balance while requiring a verified local extraction from the supported official GOG release. The project never distributes original assets: a legal owner supplies them through the local resource importer, and startup fails clearly when they are missing, incomplete, damaged, or unsupported.

## Current protocol stage

Stage: Survey. Intake identifies the owned edition and current patch provenance
(BLD-GOG-EN, SRC-PATCH-CATALOG); Runtime access is assessed in [RUNTIME.md](RUNTIME.md).
Area queues and the inventory for the executable already studied are present.
The complete installation/media path accounting is recorded in FND-RES-010 and
BLD-GOG-EN. Every screen SRC-MANUAL describes has a screen entry, SCR-UI-021 to
SCR-UI-036 as `sourced` listings. Survey remains open until every manifest
data family is reconciled. The disc's `DEMOS/`, `INN/` and `VESA/` trees and
its readme, version and placeholder files are under the build's Other files
(FND-RES-057, FND-RES-058). Conservatively retained auxiliary
media need runtime-use review and format entries until evidence supports
exclusion.
Later research includes Q-RES-017 / FMT-RES-015; Q-RES-164 through
Q-RES-167 retain icon-consumer questions; Q-RES-162, Q-RES-163,
Q-RES-206 and Q-RES-207 retain text-dictionary questions; Q-RES-157 retains the
archive-copy consumer question; Q-RES-141 through
Q-RES-150 and Q-RES-192 retain configuration-consumer questions; Q-RES-199, Q-RES-202 and
Q-RES-203 retain install-script questions; Q-RES-135 through
Q-RES-137 retain bitmap-consumer questions; Q-RES-132 through
Q-RES-134 retain batch-consumer questions; Q-RES-124 through
Q-RES-131 retain driver-consumer and payload questions; Q-RES-120 through Q-RES-123
retain disc-consumer and audio questions; Q-RES-118 and Q-RES-119 track the cue consumer questions. Every kind of entry in C1086.GOB and the scene files has a format entry
(FMT-RES-121 to FMT-RES-127 cover the last ones), except `ffonta2.fnt`, which
nothing reads (FND-RES-068); Q-RES-208 to Q-RES-214 retain their questions.
Existing implementation is tracked by parity rows; it does not prove Survey
complete or promote evidence. Missing code baselines remain explicit in the
coverage report and coverage/baseline.json.

The next implementation slice is strategic schema-two runtime integration.
This migration changes guidance and evidence notation, not gameplay or claim statuses.

## Delivery principles

The current template/toolkit infrastructure migration is specified in
[template-migration-plan.md](template-migration-plan.md). The owner authorizes its scope; plans do not add another approval gate.
The rules snapshot is recorded in `tools/upstream-lock.json`. Package versions and their toolkit revision
are recorded in `tools/toolkit-packages.json`, with dependency integrity in
`pnpm-lock.yaml` and `requirements-evidence.txt`. Plans document authorized work and do not
require a separate explicit approval before implementation or tooling proceeds.

- Keep gameplay rules in typed definitions and generic interpreters rather than screen-specific conditionals.
- Keep simulation code independent of MonoGame so it remains deterministic and testable without a graphics device.
- Record every claim about the original in `spec/` as the [documentation standard](upstream/documentation-standard.md) describes, and cite spec IDs from code, tests and these docs.
- Define every inspected container, compression stream and decoded resource family as a `spec/formats/` entry, and describe the rebuild's decoders in `docs/resource-formats.md`.
- Give every spec entry the status its evidence supports, and list what is still uncertain in its Open questions.
- Do not raise a status without executable or resource evidence, controlled observation, or an agreeing source. Mark code that guesses with `PLACEHOLDER: <ID>` and list it in `PARITY.md`.
- Store imported resources only under Git-ignored `UserContent` and validate every manifest path and source hash.
- End every phase with a playable build, clean save migration, and passing automated tests.
- Keep every compiled C# source file at or below the 1,000-line ceiling enforced by `Directory.Build.targets`; split responsibilities before they exceed the bound.

## Evidence sources

- The owned installation, decoded resources, executable control flow, and repeatable controlled observations are authoritative for this exact release.
- The [GameFAQs Conqueror 1086 A.D. FAQ by mikel123456](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730) (SRC-GAMEFAQS-66730) is a useful secondary source for game mechanics, screen flow, strategies, and general information. A rule that rests on it alone stays below `supported`.
- Record where the FAQ agrees or disagrees with the executable in the What the sources say section of each rule and in the Known errors of SRC-GAMEFAQS-66730, and keep disagreements as open questions rather than silently choosing either behaviour.

## Progress and evidence boundaries

Use PARITY.md and the generated status indexes for current totals. Run
`node tools/Report-Coverage.mjs` for inventory/citation progress and explicit
missing baselines; coverage/README.md defines its limits. Coverage is not a
share of behavior understood, and complete-reading coverage is unavailable
without a checked function-level baseline.

Every implementation slice works from the spec alone. It targets named parity
rows at `implemented` unless docs/RUNTIME.md and recorded replay evidence make
`validated` reachable. File-backed format rows may use owned-file comparisons;
rules, screens and memory formats need recorded-run or emulated-call fixtures.
A complete static reading can establish an entry but does not validate its
replacement. Add the headless fixture runner before slices that replay those
fixtures. Remaining owner-run observations are risks, never waiting gates.

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

- Document original byte layouts, widths, length semantics, compression framing and validation rules in `spec/formats/`, with findings as evidence. `docs/resource-formats.md` explains the rebuild and cites spec IDs.
- Record evidence populations and the exact hashed release in the findings that require them; keep changing validation totals in generated reports.
- Give every field and codec entry a status in `spec/formats/` and keep uncertain fields in its Open questions.
- Link original claims to spec evidence; keep inspector reports local and link implementation tests through spec IDs and parity rows.
- Update the specification whenever support is added for a new archive, image, palette, dialogue, audio, animation, save, or metadata structure.
- Preserve unsuccessful hypotheses when they prevent future contributors from repeating the same false identification.

Acceptance criteria:

- A contributor can implement an independent parser from the documentation without consulting proprietary files.
- Every implemented field has a documented bounds rule and the Standard status its evidence supports.
- Generated reports and original bytes remain ignored; only compact facts, layouts, hashes, and independently authored fixtures are tracked.
- Format documentation and decoder changes are reviewed and committed together.

### 1.2 Dialogue extraction

Deliverables:

- Decode text encoding, speaker identifiers, conversation nodes, choices, conditions, and resource references.
- Generate a local, versioned dialogue manifest keyed by stable original identifiers.
- Add a runtime dialogue repository backed by the required local official-asset extraction.
- Record factual findings and the Standard evidence status without committing complete copyrighted dialogue.

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
- Record native code/data locations, competing readings, release hashes and the Standard evidence status in spec findings and entries.

### 2.2 Controlled behavioral observation

Deliverables:

- Take an original run only after its own static attempt, or to confirm a static reading. Use the capabilities and lock in RUNTIME.md; request a live session where a person must drive it.
- Fix starting state and vary one input; record the seed and every rule-tagged random draw where the protocol requires recorded runs. Reach state through supported fields and wait on readable state, never a fixed delay.
- Build small analysis commands for distributions and candidate-formula comparison.
- Keep original saves/captures local and commit only derived facts and test fixtures that contain no proprietary content.

### 2.3 Definition promotion

Keep research and implementation in separate batches. For each recovered table:

1. Record the table and its evidence in a research batch, then hand off the spec.
2. Change the relevant typed definition.
3. Add an executable specification through the real interpreter.
4. Update the parity row in `parity/`.
5. Remove the `PLACEHOLDER` comment for it.

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
- Implement target movement, lance oscillation, the live item/lance-experience score, strict per-axis success, damage/failure, and victory. RULE-JOUST-003 has no separate strength or full-equipment gate.
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

## Slice completion tracking

The phase acceptance criteria below define the work, while parity rows record
its implementation status. Research blockers live in the area queues; a slice
names their IDs and either closes them or explicitly accepts their gaps.
Completed work and dated validation belong in Git and docs/VALIDATION.md.

## Host policies

These are the rebuild's own choices where no original behaviour is mapped yet. `PARITY.md` lists how far each area follows the original.

- The strategic encounter screen and the older `FieldBattleSession` adapter need `BATTLE.PCX` and `MEN8.CSF`, the estate map needs the seasonal `ICA`/`ICS`/`ICW` atlas, and the dragon encounter needs `DRJSTWIN.PCX` and `LANCE1.CSF`. Each fails explicitly when its asset is missing and never builds substitute art.
- `FieldBattleSession` is the older replacement overhead battle. Its grid placement, formation mechanics, mouse buttons (`FieldBattlePointerControls`) and ground destinations are host policy. Its 450 ms tick consumes every elapsed interval and keeps the remainder, and a destination moves the formation one cell per tick around occupied cells.
- The tournament screen's lance meter and keyboard labels are host policy; the meter advances 120 source units per second of elapsed time.
- Village Tournament availability uses the rebuild's location and month test (`IsTournamentHere`) instead of the original's gate (FND-UI-005).
- Generic UI text uses `PixelFont` on the current layouts; `CONFONT.CSF` is drawn where a screen's original typesetting is mapped (RULE-MEDIA-002).
- Without imported dialogue state the prototype courtship ladder of `Balance.Courtships` runs; with it, the ladies' scripts own the rewards (FND-TALK-011). The dragon moor stays hidden until conversation variable 2 is set, and without dialogue data Anna Lisa's second courtship win sets it.
- The map panel's travel labels and commands are host policy.

## Optional post-fidelity rendering enhancements

These are future opt-in presentation features, not compatibility requirements and not substitutes for completing the original rendering path. The default compatibility profile must retain the recovered 64-cell ray traversal and its gameplay consequences. In every enhanced profile, AI acquisition, combat range, pointer picking, interaction, and gameplay occlusion must continue to use the original gameplay ray and thresholds.

- Render the original virtual canvas at a higher internal resolution, then offer integer or carefully selected filtered scaling.
- Offer a modestly increased *visual-only* draw distance, capped before the 128x128 wrapping source maps reveal conspicuous repeated geometry or unintended views around the world.
- Add optional atmospheric distance fade or fog to integrate distant geometry and conceal map repetition without changing collision or visibility decisions.
- Permit reduced-detail distant scenery where it improves legibility, while actors and actionable objects remain visible, targetable, and selectable only when accepted by the original gameplay path.

The owned art contains no established distant LOD asset tiers, so any reduced-detail treatment must be derived presentation work and clearly separated from the preservation profile. Evaluate these options only after the faithful renderer is complete, using visual regression captures to ensure the original profile remains unchanged.

## Research priorities

Current focus (owner decision, 2026-10-09): analyse CONQUER.EXE and the
resources it consumes. Select queue items within that scope; auxiliary-runtime
questions are Later work and do not block current mapping.

Follow the area queues in the protocol's order, keeping static attempts before
emulated calls and original runs. The remaining startup/input work belongs to
the UI and MEDIA queues and their parity rows. Survey still needs the data-family
reconciliation and missing-code baselines described above and in coverage/README.md.
Local source locations and analyzer setup belong in RUNTIME.md and ghidra.md.

## Later: auxiliary-runtime analysis

Owner decision, 2026-10-10: the restoration starts from the preinstalled game.
SETUP.EXE, _SETUP.EXE, INST.EXE and other setup/install executables are excluded
from further analysis and recreation, superseding their earlier Later priority.
Their existing evidence, inventories and media accounting remain historical
records. Installer-only queue questions and coverage gaps are not completion
requirements. Installed configuration and resources read by CONQUER.EXE remain
in scope. The Later policy below applies to the other auxiliary runtimes.

Owner decision, 2026-10-09: defer analysis of every runtime other than
CONQUER.EXE as non-essential to current mapping. This includes AUTOPLAY.EXE,
CONFIG.EXE, CONCFG.EXE, BOOTDISK.EXE,
external wrappers and separate driver/library payload code. Resource data and
calls within CONQUER.EXE remain current; tracing into another runtime is Later.

Preserve existing findings, inventories, manifest entries and queue questions.
Queue evidence sections remain unchanged. Auxiliary consumer reconciliation
and missing inventories do not block current CONQUER.EXE work; full Survey
remains open. Current executable coverage uses CONQUER.EXE alone, while
full-build reports retain deferred files and unavailable baselines. Resume
Later work when the owner changes focus or CONQUER.EXE mapping is complete.

## Next implementation slice

Strategic schema-two runtime integration follows architecture.md and
RULE-STRATEGY-006, RULE-STRATEGY-007, RULE-STRATEGY-010, RULE-STRATEGY-011,
RULE-STRATEGY-012 and RULE-STRATEGY-013. Its target rows are in parity/STRATEGY.md
and stop at `implemented` until recorded replay evidence is available.

Exit: construct exact player commands and temporary-force inputs from live
serializable state; wire fixed updates, encounter handoff and presentation;
activate schema two only when bootstrap, migration and save/load are complete;
then remove the dated adapter. Preserve the spy-report ordering and unresolved
evidence gaps in the named entries. Require deterministic headless cases for
the whole branch tables and migration outcomes before activation.

Later implementation uses parity/ASSAULT.md for remaining combat transitions,
parity/UI.md and parity/MEDIA.md for cursor/menu timing and event bindings,
parity/ESTATE.md for atlas and terrain assignments, and RULE-SOUND-002's row
for sample callers. Missing spec behavior is a `Spec gap:` note, not a guess.

## Infrastructure requirements

The current rules snapshot/checker are pinned by tools/upstream-lock.json;
toolkit versions and revision are in tools/toolkit-packages.json. Exact package
locks and requirements-evidence.txt define installed dependency integrity.
Follow template-migration-plan.md for the current migration acceptance audit.
Use the canonical validation gate and keep owned-source evidence local.
Publication requires the owner's explicit request.
