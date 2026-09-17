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
- 196 xUnit test cases and 146 broader executable specifications passing without a graphics device through the isolated Windows test launcher.

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

Current progress: bounded `Viewer`, `Scenario`, `Map`, `Blocks`, `Backdrop`, `BackImage`, and `Pal0`–`Pal127` data from the owned scene archives drive first-person geometry, presentation, interaction, and combat. Executable tracing assigns `MELEE00`–`MELEE24` to tournament melee, a random unsuffixed `MELEE0`–`MELEE2` to practice, and common `MELEE0.RES` to campaign castle and London assaults. The original `SKIRMISH.PCX`, `SKIRMISH.PAL`, and 53-frame `SKIRMISH.CSF` supply the exact viewport shell, wall shading, actors, weapon foregrounds, and fatal/wounding effects. Recovered formulas cover actor skill and maximum health, the 200-value hit test, weapon dice and penetration, row-family trajectory envelopes, `(target - outer) << 9 / divisor` velocity, contact reversal, state-completion gates, angular billboard selection, immediate door state replacement, authored actor-side selection, actor heraldic normalization, campaign retainer consequences, and projected pointer contacts. The original blood effect is processor-dependent because it advances once per unrestricted render loop; `SiegeHitEffect` preserves its confirmed four frames with explicit monotonic 70 ms compatibility steps, documented as a reimplementation policy rather than an original formula. The desktop runtime requires a fully verified extraction from the supported official GOG release and fails before graphics initialization when any mapped dependency is missing, damaged, incomplete, or unsupported; placeholder and no-media gameplay are outside the supported design. Ordered pointer candidates, center/diagonal contacts, simultaneous-corner probes, wrapped lookup, pass-through/state continuation, the 31-entry sentinel, kind-4 live-center/depth/heading/mirror alpha selection, non-cardinal acquisition rays, and the supported hostile and friendly Defend formation loops are active. Broader friendly state integration remains open; the former empty-floor pointer return, same-call actor predicate chaining, and defeated-enemy loot are Disproved.

Combat recovery update (2026-09-13): placed behavior-135 actors now decode their block-offset-`0x4A` template and `0x4C` combat row. The runtime uses all ten executable-initialized attack-skill/armor/health triples, all 25 rows' dice and armor penetration, explicit object state targets, and symmetric row-specific contact distances for player and enemy attacks. Player attack skill, maximum health, and the shared hit threshold, including both positional bonuses, are executable-confirmed and active. This narrows the open combat work to cadence, grid/sub-cell reconciliation, movement AI, retainers, loot, and consequences rather than the already-restored health, hit, damage, and reach rules.

Combat recovery update (2026-09-13, enemy rewards): defeated-enemy loot is now closed as **Disproved** for the supported hashed release. The actor damage path `0x4EA40`-`0x4ECD8` copies death state `base + 3`; completion path `0x4F49C` removes it after the effect gate and calls hostile counter `0x4D8C0`. Complete executable reference inventories show no death-path access to wealth `0xD4A4`, ammunition `0xD4A8`, or equipment `0xD4C4`. Authored pickup dispatch remains the sole recovered combat reward mechanism, and the runtime's existing no-drop behavior is now guarded by a focused test. This supersedes the older “loot remains open” wording immediately above; later pointer updates below supersede this row's former click-selection gap. Retainers, siege consequences, and broader AI remain open.

Combat recovery correction (2026-09-13, campaign retainers and consequences): the earlier identification of `0x399F6`-`0x39A22` as the campaign army path was **Disproved** by the complete `0x5877C` caller inventory. That alternate path reads an actor-domain field through `0x2D500`; its `/50`, 2-10 clamp must not be attributed to strategic army size. The castle-invasion caller instead reads all three army unit pools through `0x29E58` at `0x39EAA`-`0x39EDB`, computes `contribution[type] = min(floor(units[type] / 3), 3)` at `0x39EE0`-`0x39F30`, sums the contributions, and supplies a one-retainer fallback at `0x39F40`-`0x39F79`. Setup `0x5877C` stores the resulting cap at `D49C`; loader pruning at `0x51B87` removes authored friendlies until `0x4D870`—which excludes player `D4D0`—is no greater than that cap. At combat completion `0x586A8` stores surviving retainers in `1F9E4`. The caller clamps that survivor count to the starting cap and computes losses at `0x3A101`-`0x3A123`, then subtracts one strategic soldier per dead retainer across the initially represented Swordsmen, Halberdiers, and Knights contributions at `0x3A129`-`0x3A17D`; setters `0x29E80` persist the pools and `0x29D24` removes an emptied army. These cap, fallback, and casualty formulas are **Confirmed** for the hashed release and are independently consistent with the GameFAQs observation that a one-soldier army supplies one companion whose death erases that army. `OriginalRetainerCombat.CampaignRetainerCapFor/ApplyCampaignLosses`, `SiegeSession`, `Campaign.FinishSiege`, and focused tests carry the mapping into the reimplementation. Formation, commands, and movement AI remain open.

Combat recovery update (2026-09-13, retainer command behavior; corrected below): actor thinker `0x50524` budgets `(actor count - 10) >> 4` records per main-loop pass and feeds state routine `0x4F49C`. The relocated acquisition and handler tables at runtime VAs `0x4F414` and `0x4F458`, together with transition helper `0x4E5F0`, confirm that Defend mode 2 scans only the surrounding 3x3 cells and attacks without seeking, Attack mode 6 chooses a strictly nearer hostile, and Follow mode 16 moves toward player target `D4D0`. The former inference that public Retreat directly uses handler `0x50020` to move away is **Disproved** by the complete transition route documented below. `SiegeSession.AdvanceRetainerOrders` drives authored friendly records explicitly and hostile actors select spatial opponents, eliminating the former aggregate interception probability. Its former whole-cell command movement is superseded by later scheduler work. `ResourceAndDefinitionTests.Retainers.cs` covers each command, Defend's neighborhood, and explicit friendly targeting.

Combat recovery update (2026-09-13, direct pointer targets): dispatcher `0x55524` consumes fixed-point raycaster `0x470A8` output and preserves the returned block/actor identity. Friendly actors toggle selection; a hostile actor becomes the exact mode-8 target for selected friendlies and consumes their click, or continues to the player's weapon path when none are selected; explicit-action objects require ray distance `< 0x280`; and the raycaster calls pixel-mask test `0x444E8`. These semantics are **Confirmed**. Later candidate and kind-4 updates below supersede this row's renderer-consistent floating-point picker and empty-ground remaining-work wording.

Combat recovery update (2026-09-13, empty-ground orders): pointer branch `0x555BE`-`0x5562E`, acquisition `0x4FD7B`, and handler `0x4FF53` confirm that requested mode 12 stores integer target coordinates in actor `+0x28/+0x2C`, affects selected actors only, clears selection and target actor `+0x24`, derives heading from `target - current`, and completes only when both current coordinates match. The runtime maps below-horizon centered pointer coordinates onto visible floor cells, rejects obstructed destinations, sends selected retainers to that exact cell, and resumes their previous command on completion. The mode semantics are **Confirmed**. Later fixed-point candidate updates supersede this row's below-horizon floor mapping and candidate/corner gap; multi-actor destination behavior is covered by the occupancy update below.

Combat recovery update (2026-09-13, actor movement cadence/sub-cells): mode-12 handler `0x4FFBC` obtains its movement descriptor from actor-block signed word `+0x46` (the high word at `+0x44`), not visual selector `+0x2E`. The complete official combat census finds 134 actor bases selecting descriptor 7 and ten selecting descriptor 10; all 144 bases and 1,002 placements start block offsets `+0x24/+0x28` at `(0,0)` and resolve to three ticks, 200 ms, flags `0x142`, local 8.8 delta `(64,0)`, and surface stride 5. Loader `0x5185F`-`0x51875` centers actor records at `(cell << 8) + 0x80`. Scheduler `0x530D0` rotates the delta through `0x44740`, advances only when elapsed is strictly greater than the interval, checks neighboring cells outside `-0x59..0x59`, and changes cells only beyond `+/-0x80`, wrapping the offset by `0x100`. Thus zero-offset ticks advance `64,128,192 -> adjacent cell/-64`: `3 * 200 = 600 ms` is the effect duration and first cell transition, but retained remainder makes sustained straight travel four ticks/800 ms per cell. The former universal 600 ms cell deadline is **Disproved**. `DynamixSceneBlock`, the expanded ignored census, `SiegeActorMovement`, imported spawns, `AdvanceRetainerMovement`, and `SiegeViewProjection` now preserve this **Confirmed** per-tick/sub-cell mapping for selected destination travel and use the same point for drawing and picking on a processor-independent clock. Exact map-ray coordinates, collision corner interactions, multi-actor destination behavior, and actor-wide cadence remain **Provisional**; direct route aiming and rejection are narrowed below.

Combat recovery correction (2026-09-13, mode-12 routing/collision flags): handler `0x4FF53` passes `target - current` through the piecewise integer heading helper `0x445C4`, then applies `(heading + 0x20) & 0xC0`; the four exact diagonal headings therefore choose the clockwise cardinal. This aim is recomputed when the live movement effect returns to actor logic. Scheduler `0x53425`-`0x535E5` rejects an axis after its proposed signed offset leaves `-0x59..0x59` when the neighboring map block has behavior bit `0x02`. Crucially, `0x4FFD1`-`0x4FFDA` rewrites descriptor flags `0x142` to live flags `(flags & 0xA7) | 0x10 = 0x112`; rejection therefore preserves the actor's partial offset and heading, zeroes the live effect's coordinate deltas and tick count at `0x53CC9`-`0x53CDD`, and returns immediately to actor thinking at `0x53D3F`-`0x53D5F`. A freshly aimed effect retries on the next scheduler update. The earlier claim that mode 12 reaches the flag-`0x40` left-turn branch is **Disproved**; `0x4FE27`-`0x4FE30` installs that flag for modes 5/6/17 instead. Open-path arrival is still observed only after normal effect completion, even when a cell crossing occurred earlier in the cycle. The complete official scene census finds 52,278 bit-2-blocked and 635,850 clear placements across all 688,128 cells, while all 1,002 actors restore clear underlying state-target blocks. `SiegeLayout` carries this authored blocker plane independently of semantic visuals, object transitions update it, and mode-12 movement implements direct aim, the strict collision band, retained-offset stop/retry, and completion boundary with focused tests. These details are **Confirmed** for the hashed release. Exact ground-ray coordinates, scheduler `0x40` corner-contact behavior, multiple actors sharing a destination, other actor modes, and broader AI remain **Provisional**. The GameFAQs guide does not specify these low-level routing rules and therefore neither corroborates nor conflicts with this correction.

Combat recovery update (2026-09-13, Attack/Follow movement cadence): the actor-kind dispatcher at object-1 offset `0x3E41C` points to per-archetype transition tables; template initializer `0x542F8` maps official friendly templates 0/1 to kind 0 and template 2 to kind 1. Kind-0 mode-6 transition entry `0x4E71F` keeps mode 6 on acquisition failure and requests mode 8 on success. Kind-1 entry `0x4E745` keeps mode 6 on failure and requests ranged attack mode 11 on success. The current-mode table at runtime VA `0x4F458` sends mode 8 and Follow mode 16 to the same handler `0x4FE76`, which calculates target-current heading through `0x445C4`, applies `(heading + 0x20) & 0xC0`, and installs the same live `(flags & 0xA7) | 0x10 = 0x112` movement effect. These state transitions, direct effect-boundary aiming, and movement construction are **Confirmed** for the hashed release. `SiegeSession.AdvanceRetainerMovement` now uses the imported 200 ms/8.8 scheduler for acquired-target Attack pursuit and Follow, preserving explicit pointer-selected target identity and removing their provisional whole-cell jumps. Focused tests assert the first three-tick cell crossing, retained `-64/+64` remainder, facing, and chosen-target direction. The runtime's nearest-target acquisition and one-cell Follow stop radius remain compatibility approximations; original visibility/contact thresholds, kind-1 ranged timing, Attack's no-target mode-6 wandering, Retreat mode 10, formations, and thinker call frequency remain **Provisional**. The GameFAQs guide offers no low-level transition or movement-cadence description, so it neither corroborates nor conflicts with this mapping.

Combat recovery correction (2026-09-13, public Retreat state route; reachability corrected 2026-09-14): command handler `0x5744B` writes requested mode 10, resets current mode, and calls `0x4E5F0`. On the next thinker pass, acquisition `0x4F98D` scans opposite-side actors through raycaster `0x470A8`, accepts the nearest visible identity, and stores its coordinate snapshot at actor `+0x28/+0x2C`. Friendly kind-0 transition `0x4E76B` chooses mode 5 on success or mode 2 on failure; kind-1 transition `0x4E805` chooses mode 4 or mode 2. This **Disproves** only the earlier attribution of `0x50020` as the public command's immediate action. Kind-0 mode-5 handler `0x4FDCD` preserves heading and flags `0x142`; scheduler collision clears the blocked coordinate and turns left with `(((heading + 0x20) & 0xC0) - 0x40) & 0xFF`. The later dynamic mode-14 hit -> mode-13 route reaches `0x50020` when the stored target is stronger and newly reduced source health is below 6, as mapped in the 2026-09-14 correction below. The former mode-11 attribution is Disproved. GameFAQs does not describe these internal states or collision rules and supplies no corroboration.

Combat recovery update (2026-09-13, authored actor sides): loader `0x51560` scans the map x-major at `0x517B7`, recognizes an actor from behavior bit `0x80` plus selector 1, takes template/row from block `+0x4A/+0x4C`, and records the first normalized side-zero actor as player `D4D0` at `0x51919`-`0x5192E`. A complete census of the supported 42 `MELEE*`/`DEFEND*` archive variants covers 144 actor-base blocks and all 1,002 placements: templates 0/1/2 are exclusively friendly, while 3/5/8/9 are exclusively hostile. That population mapping is **Confirmed** by executable control flow plus decoded official resources. `ImportedSiegeLayouts` now uses the exact actor predicate and template partition, omits the first x-major friendly as player, excludes actors from scene objects, and retains remaining friendly positions. `SiegeSession` applies `min(campaignCap, authoredRetainerCount)`, matching the loader's prune-only behavior. The 2026-09-14 checkpoint below closes raw surface/color normalization; formation, commands, and retainer movement are tracked in their later checkpoints.

Combat recovery update (2026-09-13, retainer commands): `0x55D20`-`0x55DAE` maps the shell band `x=4..209`, `y=175..187` to four 56-pixel dispatch columns. Attack, Defend, Follow, and Retreat handlers at `0x5700E`, `0x57168`, `0x572C5`, and `0x5744B` write requested actor modes 6, 2, 16, and 10; Follow writes player actor `D4D0` to target field `+0x24`. Every handler applies to selected living friendlies, falls back to all living friendlies when none is selected, and clears selection. These facts are **Confirmed** by executable control flow and the owned shell labels. The runtime now materializes capped authored retainers, displays them in the perspective scene and radar, exposes the four original command regions plus keyboard access, and no longer misinterprets `R` as whole-battle retreat. Exact selection raycasts, formations, mode transitions, and movement/pathfinding remain open.

Combat recovery update (2026-09-13, kind-1 Retreat continuation): the kind-1 transition table at object-1 `0x3E480` maps mode 4 to `0x4E6D3`; after mode-4 acquisition `0x4F98D`, that transition records previous/failure mode 1 and requested/success ranged mode 11. Handler `0x5010B` aims at target actor `+0x24`, uses the direct close branch when fixed Manhattan separation is `<= 0x154`, otherwise invokes raycaster `0x470A8`, and accepts the returned opposite-side actor only while ray distance is strictly less than object-2 combat table column 4 at `0xCE24 + 28 * row`. Unlike player contact processing, this comparison does not add `0x40`. For rows 23/24 the attack-effect constructor doubles descriptor field `+0x14`. These state identities, branch threshold, strict raw range, and ranged interval mutation are **Confirmed** for the hashed executable. `OriginalWeaponCombat.ActorContactDistanceForCombatRow`, `SiegeSession.RetreatOrderTarget`, and `RetainerRangedAttack` map the route into the runtime; focused tests cover mode-11 attack, obstruction fallback, raw values 7000/8192, and a long diagonal that is inside Euclidean fixed-point range but outside the former Manhattan approximation. The runtime's bounded cell line trace is **Corroborated**, not claimed identical to the original raycaster. The GameFAQs guide contains no state-machine or ray-distance detail and neither corroborates nor conflicts with this finding.

Combat recovery update (2026-09-13, mode-11 completion cadence): effect constructor `0x4C7F4` records the current millisecond clock at live-effect `+0x10` and the selected interval at `+0x14`. Scheduler `0x5310D` advances only when `now - deadline > interval` and preserves deadline overrun. At `0x53365`, damage routine `0x4F070` runs only when the effect counter equals its count, actor mode `+0x18` is 11, and target health `+0x40` is positive. Completion at `0x53D47` clears actor effect handle `+0x08` to `-1` and immediately invokes thinker `0x4F49C` for that same actor. This proves the shot applies damage at its final effect tick—not at handler entry—and may schedule its next effect immediately. `PendingRangedTarget`, `AdvanceEnemyAnimations`, and `AdvanceRetainerOrder` implement the gate for both kind-1 Attack and Retreat; a new command or hit cancels the pending shot. Focused tests cover strict equality, deferred damage, repeated doubled gates, immediate same-actor re-entry, both public command routes, and cancellation. The state, timing, and re-entry route are **Confirmed** for the hashed executable; the runtime uses an elapsed monotonic abstraction rather than claiming clock-source identity. GameFAQs has no comparable timing detail.

Combat recovery update (2026-09-14, supported hostile actor loop): imported hostile templates 3/5/8/9 now preserve initializer profiles `6/8/6`, `4/8/1`, `6/8/4`, and `4/8/4` alongside actor kinds 2/4/2/7. Hostile kind-2 and kind-4 fixup columns independently converge on transitions `0x4E71F` for mode 6, `0x4E745` for mode 8, `0x4E77E` for mode 11, and `0x4E851` for mode 13; kind 7 shares kind 2. This proves the supported defenders acquire friendlies, pursue them through direct `0x112` sub-cell effects, test strict raw-row contact, and defer damage until attack-effect completion. Mode-14 hit handler `0x503EC` and mode-15 handler `0x504C0` compare the actor's stored target with its newly reduced health; their shared `0x4E77E` transition selects mode 13 for a stronger target, after which the health-6 edge reacquires or enters exact `1.5`-scaled mode-10 escape. Transition `0x4E6C0` sends the completed escape to mode 4 on renewed acquisition or mode 3 otherwise; mode-4 success returns to 8, with kind-specific failure 2 (`0x4E8B2`) or 1 (`0x4E6F9`). `OriginalActorTemplate`, `OriginalModeProfile`, `ActorMode`, and `AdvanceHostileMovement` replace official defenders' former randomized whole-cell AI with the recovered elapsed-time path; focused tests cover all four profiles, pursuit deltas, strict deferred damage, effect repetition, hit-effect cancellation, and health 5/6. The former mode-11 attack-rejection attribution is **Disproved**. The following formation update closes modes 1-3 and the one-predicate execution boundary; only the original wall-clock thinker frequency remains intentionally adapted because it is processor-dependent. The [GameFAQs FAQ 66730](https://gamefaqs.gamespot.com/pc/574792-conqueror-1086-ad/faqs/66730) remains a trusted gameplay-oriented starting point, but supplies no internal state, timing, or formula evidence for this batch; these claims come from the hashed executable and decoded official scenes.

Combat recovery update (2026-09-14, hostile formation and thinker boundary): predicates `0x4F648` and `0x4F6F7` scan the live-centered surrounding 3x3 cells in relative y/x order for same- and opposite-side actors. Same-side ray acquisition `0x4FC34` drives modes 3/5, while `0x4F8B0` validates mode-7 contact below `0x200`. Kind-2/7 fixup sources `0x4E4C4/0x4E4C8/0x4E4CC/0x4E4D4/0x4E4DC` establish success/failure destinations `4/6`, `11/1`, `7/1`, `7/5`, `1/6`; kind-4 sources `0x4E53C/0x4E540/0x4E544/0x4E54C/0x4E554` establish `4/3`, `11/4`, `7/2`, `7/5`, `1/5`. State routine `0x4F49C` evaluates one current predicate, applies `0x4E5F0`, and dispatch invokes only the new mode handler. Modes 1-4 use no-effect `0x4FDAB`; direct modes 7/8 use `0x4FE76` and cardinalize with `(heading + 0x20) & 0xC0`. This **Disproves** the earlier runtime's immediate multi-predicate chain and non-cardinal pursuit. `AdvanceHostileMovement` now preserves the exact transition graph, spatial scans, target identities, hit interruption, and immediate post-effect thinker call. Since global thinker `0x50524` advances cursor `D5C4` per unrestricted main-loop pass, an original wall-clock interval is not processor-stable; the reimplementation runs one predicate per fixed 60 Hz simulation update, and player actions do not inject extra imported-hostile passes, as an explicit future-proof compatibility policy. Focused tests cover both hostile kind families, no-effect boundaries, 3x3 defend contact, same-side regroup, cardinal heading, and input-frequency independence. GameFAQs FAQ 66730 has no internal state-machine or scheduling detail and does not corroborate this low-level mapping.

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

- Implement lair discovery, direct arrival-to-encounter flow, rewards, visits, and associated marriage/rumor branches. Decoded dialogue attributes absent-dragon plunder only to Sir Frederick; do not invent a player plunder loop unless stronger executable or controlled-observation evidence appears.
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
| M5: Two endings | 8 | Both routes are playable as prototypes. The dragon moor stays hidden until original global variable 2 is raised by Anna Lisa's decoded secret-lair branch; clean-room mode mirrors that disclosure after her second successful courtship joust, and older saves already at the moor migrate safely. A prepared knight now proceeds from the owned `TRANDRAG.SMK` approach directly into the timed, moving-eye, one-thrust encounter; strength affects the hit window, retreat remains possible, and the owned `DRJSTWIN.SMK`, `DRJSTLSE.SMK`, and `DRJSTRUN.SMK` sequences play for its three outcomes. Dragon victory then continues into the executable-confirmed `CHAMPL30.SMK` King's Champion investiture, while taking London plays the confirmed `CROWNL30.SMK` throne ceremony. Reaching age 30 without either victory plays the confirmed `AVG_END.SMK` sequence, distinct from fatal dragon and Drogo outcomes. The imported `DRJSTWIN.PCX` and 25-frame `lance1.CSF` supply the dragon battle's original background, palette, and foreground lance. Decoded dialogue's absent-dragon plunder belongs to Sir Frederick's history rather than a supported player mechanic. Unpaid harvest debt now raises a persisted Drogo demand with exact-debt payment or a fatal first-person fight; killing him permanently disables the moneylender. | Recover Drogo's exact combat scene/stats, replace remaining shortcut courtship rewards with decoded dialogue state, confirm exact dragon target path/timing and lance frame placement from the executable, and recover crown politics plus any remaining fatal transitions. |
| M6: Original presentation | 9 | In progress: file-backed direct title/credits/item-movie playback, the confirmed campaign briefing, title fallback, options hub with original state widgets and persistent channel volumes/reduced motion, pause, a centered aspect-correct virtual canvas with windowed/borderless-fullscreen and integer-scaling modes, character screens, estate/map panels, all ten executable-ordered Home hotspots, Overview/War Planning backgrounds and controls, four descriptor-driven fief-management variants, the populated inn with ten named patrons and portraits, all six tournament lady selector conversations, original blacksmith node `3201`, ordinary parish selector `3100`, Cambridge armor-quest selector `3149`, CD audio, presentation descriptors, edge-triggered controller navigation, context-sensitive dialogue/map/shop/tournament/combat actions, and a right-stick virtual pointer for all original hotspots are active. | Exact army-path behavior, exact estate tiles, regional mapping for priest roots `5000`-`5500`, remaining conversation entry points and typed-state bridges, remaining CSF/SMK event bindings and seeking, fonts, cursors, subtitles, and input remapping. |
| M7: Release | 10 | In progress: launch/test/import utilities, five atomic save slots plus a separate transition-triggered autosave with explicit schema migration, previous-generation recovery, and user-visible generation-specific corruption diagnostics; supported-release detection; exact disk-space preflight; progress-reported atomic/resumable content writes; manifest-wide verify/repair and manifest-scoped uninstall commands; a data-driven legal-boundary check; four-platform CI; self-contained Windows/Linux/macOS packages; a manual four-installer release workflow; and an ownership-aware Windows installer with smart GOG discovery, shortcut validation, and automatic extraction are implemented. | Signed/notarized packages, clean-machine verification, third-party notices, and release checklist. |

## Schema-2 replacement-state checkpoint: 2026-09-17

`OriginalStrategicCampaignState` provides the non-aliased mutable replacement specified by `architecture.md`: all 14 properties, all 176 people, five exact enemy slots, six exact player records, route identities/cursors, integer and floating movement state, generator/reactive counters, bounded speed, month-derived profile, persisted camera, list heads, and sparse full-dword terrain mutations. `StrategicSchemaTwoMigration.Prepare` implements deterministic slot-ordered schema-1 settlement, returning complete columns only to valid still-hostile origins and journaling all return/dispersal outcomes before clearing the dated roster. Complete JSON round-trip and structural rejection tests pass. The decoded resource provider and both movement shells are now implemented; schema activation still waits for exact player command construction, application fixed-update/presentation wiring, and dated-adapter replacement.

## Strategic motion-kernel checkpoint: 2026-09-17

The decoded provider now drives a mutable physical-slot-order pass for all three original movement modes. `AdvanceMovementPass` reproduces direct mode's crossing test, exact object-2 `+0x4F97 = 0.9` terrain scale, post-step grid refresh, and modulo-byte origin return; routed mode consumes the real forward/reverse cursor with strict tolerance/crossing, integer normalization, prior-direction validation, prospective terrain lookup, kind-9 teardown, and strict object-2 `+0x7389 = 50.0` horizontal application gate; pursuit mode reads live target slot `+0x14`, signals target disappearance, recomputes normalized direction, and switches to direct origin return with one unscaled step on kind 9. Sparse saved terrain mutations override `icon.jp` during the same lookup. Six focused tests cover physical slot order, exact-boundary crossing, byte wrap, route tolerance/completion, corrupt-direction teardown, live pursuit, target disappearance, and fallback. Schema and dated campaign behavior remain unchanged; next integrate generator-first dispatch, completion contacts/encounters, and live player-army targets before activating fixed updates and schema 2.

## Strategic scheduler-shell checkpoint: 2026-09-17

`AdvanceSchedulerPass` now surrounds the motion kernel with the executable's generator-first, first-free-slot construction and per-slot completion interleaving. It implements both timed and reactive branches, ordinary and pursuit constructors, the corrected ordinary force expression `household + floor(floor(lord rating / 4) / 3)`, reactive detachment/support arithmetic, direct grid-person lookup, five-probe contact precedence, encounter handoff, byte-garrison reinforcement, teardown, and exact first-nearby-army/player/origin retargeting. Tests prove a newly generated record can move in the same pass and that an encounter stops later physical slots. The original's uninitialized finder-output read is documented and replaced with an explicit-current-detection safety policy. The next checkpoint supplies these inputs from live campaign state, preserves spy reporting before the pass, wires the scheduler to the fixed update and encounter presentation, activates schema 2, and removes the dated adapter.

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
- [x] Recover door behavior-bit and state-target semantics. The explicit-action bit is `0x10`; accepted actions immediately install record offset `0x40`, non-actionable doors remain closed, and wall/water targets preserve blocked passages. The former adjacent-record timer is Disproved.
- [x] Classify and activate original knight/champion/footman state templates. Executable renderer `0x46E3B`-`0x46F23` maps relative heading through the block's base surface and angular divisions and mirrors behavior-bit-2 actors; all 144 placed actor bases in 42 owned combat scenes use adjacent base/attack/hit/death divisions `8/4/4/2`. Import each state's selected `SFXDEFS` completion gate, preserve ranged attack doubling and the strict deadline boundary, preserve wall-column occlusion, and remove dead actors from collision/combat before their death-state gate completes. Sequential nine-frame attack and eight-frame collapse playback is Disproved.
- [x] Decode the complete canonical `Pal0`-`Pal127` scene color-map set as four 32-step indexed-remap families. Decode Scenario count/shift/blend parameters and the per-block offset, regenerate the first family under `SKIRMISH.PAL` with the executable's blend/round/nearest-color rules, and apply its fixed-depth clamp through a bounded texture/map cache. All 90 owned active families match regenerated output byte-for-byte.
- [x] Remove the incorrect 15-castle mapping across tournament's `MELEE00`-`MELEE24` family. Executable tracing assigns the two-digit builder to tournament melee and confirms an inclusive 0-2 roll over unsuffixed `MELEE0`-`MELEE2` for practice. Initialization registers dispatcher `0x21EEC` in engine callback field `+0xB0`; its sole indirect invocation reaches slot 128 and literal `MELEE0.RES`. Practice selection and common campaign use are Confirmed.
- [x] Decode and palette-verify the 53-frame `SKIRMISH.CSF` sequence, classify its weapon, shield, and blood-effect runs, and bind the axe, crossbow, hammer, mace, sword, and dagger sequences to first-person combat. Executable traces now confirm the 23 store-weapon item-to-row permutation, Light and Heavy Crossbow rows, exact foreground bases, melee approach/contact/return pose order, crossbow offset, per-row break-roll ranges, symmetric player/enemy contact distances, fatal/nonfatal hit-effect bases, and four-frame effect extent. Exact state-linked timing remains Provisional.
- [x] Decode the headerless 320x200 `SKIRMISH.PCX` indexed plane with its separate raw palette; use its exact 167x117 combat aperture, message/status panels, command labels, health strip, and radar well. Missing or unsupported official media fails startup; an assetless/generated presentation path is intentionally unsupported. Exact dynamic field contents and mouse-command semantics remain Provisional.
- [x] Preserve mask-83 exits as visible scene boundaries and render every non-actor kind-4 pickup, debris, and behavior-bit-`0x20` contact object as an occluded billboard. The executable confirms weapon contact at `0x55A02`, combat-row column-4 distance, and one-contact replacement through record offset `0x40`. Mask-19 pickups carry explicit-action bit `0x10` and use the strict `< 0x280` range at `0x556AF`. Dispatcher `0x51E60` reads the selected block's selector at `+0x48`: cases 5, 7, 9, and 10 implement +25 wealth, capped 2d6 healing, Chain Hauberk bit 6, and +12 bolts from arguments at `+0x4A/+0x4C`; the active combatant record supplies only the recipient discriminator.
- [ ] Trace the complete startup/menu state machine, input timing, cursor behavior, and region-action dispatch from `CONQUER.EXE`; keep semantics corroborated until each executable branch is confirmed.

The resource-decoding sprint is first because it unlocks exact dialogue, screen mappings, opponent identities, construction data, and balance tables needed by most later phases.

## Next implementation priorities

Combat recovery update (2026-09-13, shared mode-12 destinations): the complete
owned scene census finds behavior `0x87` on all 144 placed actor bases and all
576 adjacent base/attack/hit/death state blocks, so every live actor provides
blocker bit `0x02`. Scheduler `0x53624`-`0x53694` restores the mover's saved
underlying block to its former cell, captures the destination block into actor
record `+0x28`, mirrors it into live block state target `+0x40`, and installs
the actor block from `+0x24` into the destination before the next effect runs.
Multiple selected mode-12 actors targeting one coordinate therefore do not
overlap or fan out: one occupies the cell; later arrivals stop at the strict
collision band with retained offset and reconstruct their direct `0x112`
effect to retry. `OriginalCombatantTemplates.PlacedActorBehavior`, existing
runtime occupancy, and a focused two-retainer regression preserve this
**Confirmed** behavior. A processor-stable original simultaneous-tie winner is
**Disproved**. Input assigns orders before scheduler `0x530D0` and thinker
`0x50524`; the thinker begins at persistent cursor `D5C4`, constructor
`0x4C7F4` chooses the first free effect slot, and the cursor advances once per
unrestricted main-loop pass. Runtime authored order (x-major for official
imports) is therefore an explicit deterministic compatibility adaptation, with
an update-subdivision regression. GameFAQs has no equivalent internal account.
This update supersedes the earlier clauses that leave multi-actor destination
behavior or exact winner ordering Provisional.

Combat recovery correction (2026-09-14, reachable mode 10): every kind-table
mode-9/10 slot is decoded. Success destinations by kind 0-6 are `9/5`, `9/4`,
`6/4`, `9/10`, `6/4`, `9/10`, and `9/10`; kind 7 shares kind 2. Corresponding
failure/previous destinations are `6/2`, `6/2`, `1/3`, `1/5`, `1/3`, `1/2`,
and `1/1`. Thus every route to mode 9 is a mode-9 self-loop; no other
transition enters it. Initializer `0x542F8` writes current/requested/previous
triples for templates 0-9 as `4/4/4,4/4/4,4/4/4,6/8/6,4/10/6,4/8/1,1/4/2,
4/10/1,6/8/4,4/8/4`. The supported placement census uses only templates
`0,1,2,3,5,8,9`, none of which starts or requests modes 9/10. Templates 4 and
7 alone preload requested mode 10 and are unused in all 1,002 placements.
That census proves only that supported actors do not initialize in mode 9/10.
Its former claim that handler `0x50020` is unreachable is **Disproved** by the
dynamic hit-state route: mode-14 handler `0x503EC` compares stored-target
health with newly reduced source health, and mode 15 repeats it at `0x504F2`.
Shared transition `0x4E77E` selects mode 13 when target health exceeds source
health and helper `0x4E5F0` cancels the prior live effect. Predicate `0x4FD9A`
tests source health `>= 6`; mode-13 transition
`0x4E7A4` chooses mode 2 on success or mode 10 on failure. Handler
`0x50020` then computes heading from `current - stored target`, multiplies both
movement deltas by object-2 double `0x7CEA = 1.5`, converts each with x87
`FISTP`, and constructs a direct `0x112` effect using non-cardinal 1.15
rotation. `OriginalActorMotion`, `SiegeSession`, and focused hit-state,
health-boundary, cancellation, and integer-rotation tests implement this
Confirmed route. The former mode-11 attack-rejection attribution is Disproved.
GameFAQs has no internal actor-state account and supplies no low-level
corroboration.

Combat recovery update (2026-09-13, fixed-point empty-ground projection):
viewer initialization `0x5421F`-`0x542E4` fixes elevation at `0x80`, horizon at
integer `height / 2`, and both ray-width inputs to viewport width. `0x470A8`
forms forward basis `0x4000` and lateral basis
`((0x400000 / width) * (x - width / 2)) >> 8`; traversal `0x45158` normalizes
the major component to signed `0x100`, scales the minor component, wraps the
original map lookup with `& 0x7F`, and caps work at `0x40` steps. `GroundCell`
now uses that integer camera/vector contract and bounded obstruction traversal,
replacing the speculative floating 60-degree half-cell plane. Formula and cap
are Confirmed; center/edge/horizon/range/occlusion tests Corroborate the runtime
bridge. The candidate-helper update below supersedes this remaining-work note.
The trusted GameFAQs
FAQ 66730 contains no internal ray mathematics and is not corroboration here.

Combat recovery correction (2026-09-13, projected pointer contacts): switch
table `0x34A0C` sends kind 0 to no candidate, kinds 1/4 to a cell box, kinds
2/3 to center planes, and kinds 5/6 to opposing diagonals. `0x44F7C` owns 32
slots, but comparison `0x45061` stops at post-increment count `0x1F`, allowing
31 usable contacts. `0x45158` probes changed corners in `(newX, oldY)`,
`(oldX, newY)`, `(newX, newY)` order and single-axis entries before their two
perpendicular neighbors. Behavior bit 0 determines pass/stop, while eligible
kind-4/behavior-bit-3 blocks continue through state target `+0x40`. `0x470A8`
projects block `+0x1C/+0x20` lower/upper
elevations with `top = horizon - (upper - cameraElevation) * viewportWidth /
depth` and `bottom = horizon + (cameraElevation - lower) * viewportWidth /
depth`, then uses width/shift/height `+0x10/+0x14/+0x18` in alpha sampler
`0x444E8`. Primary flag-0 dispatch sends selected friendlies to that contact
before actor/object-specific actions, even when its cell is blocked. This
Disproves the preceding empty-floor bridge: kind 0 can never produce that hit.
`OriginalSiegeProjection` implements the wrapped 64-step walk, exact probe and
insertion order, pass/stop and state continuation, center/diagonal shapes,
31-entry cap, block bounds, and alpha-backed pointer selection, with focused
tests for each boundary. Kind-4 pointer candidates now use the executable's
live centered position, forward depth, ray-relative lateral coordinate,
heading-sector/mirror formula, and alpha while retaining actor/object identity.
Non-cardinal acquisition integration is active and executable-backed. The trusted GameFAQs FAQ 66730
contains no internal ray mathematics and is not corroboration here.

Combat recovery update (2026-09-13, actor acquisition): `0x4F98D` scans
68-byte actor records in authored order, skips inactive/self/same-side entries,
and subtracts live fixed positions at `+0x0C/+0x10`. Heading helper `0x445C4`
folds quadrants around `floor(0x20 * minor / major)`; local delta `(dx,dy)` is
passed in original coordinates as `(-dy,dx)`. Sine `0x446BC`, cosine `0x4472C`,
and rotation `0x447C4` use the signed 1.15 quarter-wave table at object-2
`+0xC904` and round each product with `(product + 0x3FFF) >> 15`. The centered
`0x470A8` ray must resolve through `0x4CEA0` to the exact candidate actor.
Nearest depth starts at `0x7FFF`, only a strictly smaller depth replaces the
selection, and an accepted depth below `0x154` terminates the scan. The runtime
wires this path to imported scenes and official textures; focused tests cover
cardinal/diagonal headings, a non-cardinal depth, transparent rejection,
identity, strict ties, and the close early exit. These low-level facts are
Confirmed by the supported hashed executable. GameFAQs FAQ 66730 remains a
trusted gameplay starting point, but supplies none of these formulas.

1. Continue first-person combat recovery: projected surface contacts and non-cardinal actor acquisition now share the recovered fixed-point projection, decoded elevations, texture alpha, live actor identity, and authored candidate order. Strict imported 200 ms movement ticks and retained 8.8 offsets, direct re-aim, authored blockers, collision families, Attack/Follow/Defend/Retreat routes, mode-11 completion cadence, single-cell occupancy, deterministic tie-breaking, and actor heraldic normalization are active. Kind-0 Retreat follows mode 5 through regroup mode 7, the mode-1/3/4 formation decision, renewed mode-8 pursuit, strict contact, and completion-gated mode-11 damage. Friendly Defend now follows both kind tables through the mode-2 neighborhood decision and subsequent formation, pursuit, scaled-escape, and contact states. Mode-14 hit handling performs the stored-target/source-health comparison before mode 13's health-six decision and reachable mode-10 non-cardinal escape. Supported hostile pursuit, attack, hit/morale, formation, defend, and regroup states are active with one predicate per fixed update. Continue with remaining friendly state integration and remaining first-person combat transitions.
   Retreat regroup update (2026-09-13): the kind-0 transition table at runtime `0x4E43C` sends mode 5 through `0x4E70C`, with failure/previous mode 5 and success/requested mode 7. Predicate `0x4FC34` scans active same-side non-self actors in authored order, derives live-center heading through `0x445C4`, casts `0x470A8`, requires exact identity through `0x4CEA0`, retains only a strictly nearer depth than `0x7FFF`, and stops early below `0x200`. Mode-7 handler `0x4FE76` installs direct `0x112` movement toward the stored actor. Its predicate `0x4F8B0` deliberately accepts any living same-side actor returned below `0x200`, not only the aimed actor; transition `0x4E732` then selects mode 1 or returns to mode 5. Official import now retains unified x-major actor order and the promoted player's actor metadata separately from the live session/Viewer position. The route through entry into mode 1 is Confirmed and tested; mode 1 onward remains Provisional. GameFAQs FAQ 66730 has no internal state, identity, or threshold evidence and neither corroborates nor conflicts with this mapping.
   Formation decision update (2026-09-13): mode-1 predicate `0x4F648` scans the surrounding map cells in relative row-major order (`y=-1..1`, `x=-1..1`), resolves actor blocks through `0x4CEA0`, skips self, and accepts the first same-side actor. Its center is the live fixed coordinate shifted by eight, mapped as `((cell << 8) + 0x80 + offset8) >> 8`. Kind-0 transition `0x4E6C0` chooses mode 4 when supported and mode 3 otherwise. Mode 3 reuses same-side ray scan `0x4FC34`; `0x4E6E6` selects direct regroup mode 7 or fallback 2. Mode 4 reuses opposite-side scan `0x4F98D`; `0x4E6F9` selects direct pursuit mode 8 or fallback 1. Fixups at runtime sources `0x4E43C`, `0x4E444`, and `0x4E448` independently establish those table entries. Runtime state decisions remain one per thinker-equivalent call, preserve sub-cell coordinates, and are covered for both isolated and adjacent-friendly paths. This route is Confirmed through construction of the first mode-8 effect; mode-8 contact/attack continuation remains open. GameFAQs has no comparable low-level detail.
   Mode-8/hit-state update (2026-09-13; corrected 2026-09-14): predicate `0x4F7A2` accepts an actual opposite-side ray hit only below raw contact column `0xCE24 + 28 * sourceRow`; transition `0x4E745` selects mode 11 or failure mode 6. Handler `0x5010B` repeats validation through the `<= 0x154` Manhattan shortcut or a fresh ray, and scheduler `0x53365` applies damage only at effect completion. The health comparison is not mode-11 attack rejection: mode-14 hit handler `0x503EC` compares stored-target health with newly reduced source health before constructing the hit effect, and mode 15 repeats it at `0x504F2`. Shared transition `0x4E77E` selects mode 13 for a stronger target and `0x4E5F0` cancels the prior live effect. Predicate `0x4FD9A` then selects recovery at health `>= 6`, otherwise reachable mode 10 and its 1.5-scaled non-cardinal escape. Runtime tests cover contact equality, delayed damage, hit cancellation, health 5/6, and exact integer movement. GameFAQs has no comparable internal detail.
   Mode-6 correction (2026-09-13): public Attack requests mode 6. Acquisition-table entry `0x4F98D` searches for the nearest visible opposite-side actor; kind-0 transition `0x4E71F` and kind-1 transition `0x4E745` retain mode 6 on failure, while success selects modes 8 and 11 respectively. Current-mode handler `0x4FDCD` preserves heading, clears target fields, selects movement descriptor `+0x46`, and rewrites its low flags as `(flags & 0xA7) | 0x40` (the official `0x142` remains `0x142`). The scheduler's flag-`0x40` collision branch clears the blocked local axis and turns left with `(((heading + 0x20) & 0xC0) - 0x40) & 0xFF` while the effect continues. `SiegeSession` uses the executable-backed actor ray and stores the chosen movement family on the live effect so an order cannot alter collision semantics mid-cycle. Focused tests cover preserved heading, movement cadence, collision continuation, ordered candidate identity, alpha rejection, and non-cardinal depth. The state route, flag formula, scheduler response, and imported-scene ray path are Confirmed from the hashed executable. The GameFAQs guide contains no internal state, ray, or movement-flag detail, so it neither corroborates nor conflicts with this recovery.
   The timing architecture, state binding, and runtime path are mapped and active. Scenario offset `0x1C` counts `SFXDEFS`; `0x4C7F4` copies them into live records and `0x530D0` advances independent deadlines. Actor state rendering, foreground trajectories, the 166 ms update cap, and centered pointer provenance retain their documented formulas. Fixed-point pointer and acquisition arithmetic, block elevation spans, alpha, the 64-step bound, multi-candidate continuation, diagonal/corner geometry, wrapped source lookup, and kind-4 billboard picking are active.
   Future-proof the recovered design with a monotonic simulation clock and per-record deadlines. Preserve original interval values, state order, strict threshold, and deadline-overrun behavior in compatibility tests, but keep rendering observational and use bounded update catch-up after stalls. Do not make effect or combat speed depend on processor throughput, render-call count, monitor refresh rate, or wall-clock adjustments; represent the original four-render-call blood lifetime as four explicit presentation/simulation steps once the original render cadence is established.
2. Continue tracing startup/menu input timing and HAT region-action dispatch from `CONQUER.EXE`; all ten Home labels, nine registered Home actions, War Planning controls, independent division movement, and the five-slot report-before-movement spy boundary are active. `JUMP!!` is now closed as an intentionally inactive label-only region. Spy cost, lifetime, slot scan, trigger ordering, all 176 person initialization rows, named property rows, linked lord inputs, initial household counts, strategic speed clock, route resources, terrain grid, and exact camera-ordered world-to-grid inversion are confirmed; schema-2 runtime migration and original input timing remain to be confirmed.
3. [Completed 2026-09-08; policy updated 2026-09-13] Bind the decoded five-definition age groups to campaign state using the executable-confirmed per-age selection policy and keep original prose local. Startup now requires those owner-imported resources rather than supporting built-in summary gameplay.
4. In progress: bind menu CSF sequences to verified palettes and roles. `OPTION.CSF` ON/OFF held/released and Resume frames use `OPTFIN.PCX`; `SWORDS.CSF` item art and `BUYSELL.CSF` control states use `SWDTEMP.PCX`; all six `FFMOUSE.CSF` frames are classified and contextual travel/talk/target/pressed-hand selection is active. Exact cursor timing, hourglass dispatch, and other menu sequences still require evidence.
5. [Completed 2026-09-10] Add legal-boundary automation that fails if imported media or generated analysis artifacts enter Git, then add Windows x64, Linux x64, macOS arm64, and macOS x64 CI restore/build/test/publish coverage plus native installer checks. `tools/Verify-Repository.ps1` interprets the data-only `repository-policy.json`; the local test launcher and every GitHub Actions package workflow enforce it before compiling.
6. In progress: `ICA`/`ICS`/`ICW` are decoded and active as seasonal 337-frame estate atlases using the `ICONTEMP.PCX` palette. Recover the executable's exact terrain/frame table plus roads, shield, cursor, and layout data; current semantic frame assignments remain provisional.
7. In progress: unsigned 8-bit mono PCM is confirmed; all imported samples are eagerly converted into a retained startup cache and the shared 2,159-byte interface sample is active. Trace and bind the remaining 101 sample event identifiers; `VSMITH.666` is audio, not the blacksmith dialogue database.

## Combat checkpoint: 2026-09-14 complete friendly Attack loop

Public handler `0x5700E` resets current mode, requests mode 6, and calls transition helper `0x4E5F0`; it never invokes a synchronous grid-range strike. On the next thinker pass acquisition `0x4F98D` retains mode 6 on failure, selects mode 8 for kind 0, or selects mode 11 for kind 1. Kind-0 predicate `0x4F7A2` aims at the stored target but retains the actual returned living opposite-side actor only when its ray depth is strictly below raw combat-row contact column `0xCE24 + 28 * row`; exact equality returns to mode 6 through `0x4E745`. Handler `0x5010B` constructs the attack effect, scheduler `0x53365` applies damage only at completion, and `0x53D47` clears the effect before same-actor thinker re-entry.

`AdvanceAttackOrder`, `ModeEightContactTarget`, `BeginFriendlyMode`, and `PendingRangedTarget` preserve this complete public and pointer-selected route. Regressions cover pursuit before damage, strict below/equal contact boundaries, intervening-opponent identity, and completion-gated damage. The state loop is **Confirmed** and the former synchronous kind-0 melee shortcut is **Disproved**. The following checkpoint closes exact Follow modes 16/17; raw actor color normalization and remaining first-person combat transitions follow.

## Combat checkpoint: 2026-09-14 complete friendly Follow loop

Friendly kind-0 table sources `0x4E478/0x4E47C` and kind-1 sources `0x4E4BC/0x4E4C0` resolve to identical mode-16/17 transitions `0x4E7CA/0x4E7DD`. Mode 16 evaluates same-side predicate `0x4F8B0`, then transitions to mode 17 for both outcomes. Mode-17 handler `0x4FDCD` clears target fields, preserves heading, and installs the `0x40`/`0x142` movement family. Predicate `0x4FB39` casts toward player global `D4D0`, requires the returned actor to be that exact identity at depth `< 0x7FFF`, and records its current integer coordinates. Failure retains 17; success returns to mode 16, whose handler `0x4FE76` cardinalizes and installs direct `0x112` movement.

`AdvanceFollowOrder`, `FollowModeSixteenHasContact`, `FollowPlayerIsVisible`, and `BeginFriendlyMode` now implement this **Confirmed** loop for both friendly kinds. The former one-cell stop approximation is **Disproved**. Tests cover the unconditional first mode-17 effect, preserved heading, incorrect returned identity, strict equality rejection, and accepted below-boundary return to direct mode 16. The following checkpoint closes raw actor color normalization; remaining state transitions follow.

## Combat checkpoint: 2026-09-14 original actor color normalization

Loader `0x51A8B`-`0x51B74` maps player red, green, and blue to palette-family / walk-texture tuples `(0,64)`, `(32,128)`, and `(64,96)`. Every friendly receives the player tuple. Hostiles preserve authored values unless their palette-family base conflicts with the player; red and green conflicts become blue, while a blue conflict becomes green. State copier `0x4E39C` changes the attack/hit/death texture base but retains normalized block `+0x08`, and actor rendering `0x46E3B`-`0x47057` adds the block distance-map result within that 32-entry family.

`SiegeActorColorMapping` and `SceneEnemyTexture` implement this **Confirmed** path, including unknown-color fallback to executable default red. Focused tests cover all player tuples, both conflict alternatives, non-conflicting hostile preservation, all 32 distance variants, and invalid family bounds. The complete owned census covers 144 actor bases and 1,002 placements with authored selector bases 0/32/64/96; no decoded bytes or generated report is committed. Continue with remaining friendly states and first-person combat transitions.

## Combat checkpoint: 2026-09-14 kind-1 Retreat and hit recovery ordering

Kind-1 table source `0x4E4A4` maps mode 10 to transition `0x4E805`, selecting mode 4 after visible-opponent acquisition or mode 2 on failure. Mode-4 source `0x4E48C` maps to `0x4E6D3`, selecting ranged mode 11 or fallback mode 1. State routine `0x4F49C` permits only one predicate before invoking the new handler, so public Retreat requires distinct `10→4` and `4→11` thinker passes; the former runtime constructed its shot one pass too early. Mode-13 source `0x4E4B0` maps to `0x4E851`, which records failure mode 10 and success mode 6 around the shared health-`>=6` predicate. Mode-5/6 sources `0x4E490/0x4E494` map to `0x4E70C/0x4E745`, retaining their wandering states or selecting regroup mode 7 / ranged mode 11.

`AdvanceRetreatOrder` now routes public and hit-triggered states through `AdvanceDefendOrder` one decision at a time. Regressions cover explicit mode-4 separation, intervening-target mode-11 resolution, strict doubled completion gates, cancellation, the mode-13 health 5/6 boundary, 1.5-scaled escape, and ordinary mode-6 movement. This state slice is **Confirmed**, and the same-pass public Retreat shot is **Disproved**. Continue with remaining first-person transitions and broader menu/estate/audio work.

## Combat checkpoint: 2026-09-14 exact pointer action depth

World dispatcher `0x55524` receives an 8.8 depth from fixed-point raycaster `0x470A8`. Explicit-action comparison `0x556AF` accepts only depth `< 0x280`; the later player weapon path compares the same returned depth strictly with the selected combat row's raw contact column plus its `0x40` player allowance. The runtime formerly preserved hit identity but discarded this depth at the session boundary, substituting Euclidean actor/object cell-center distance.

`SiegePointerHit.Distance8` now flows into depth-aware `SiegeSession.Attack`, `Shoot`, and `Interact` overloads. Pointer actions use the returned depth without a second grid occlusion or center-distance calculation; keyboard actions preserve their existing forward-grid compatibility path. Tests cover `weapon contact - 1` acceptance and equality rejection, plus explicit-action acceptance at `0x27F` and rejection at `0x280`. These depth and strict-boundary semantics are **Confirmed**; the cell-center substitution is **Disproved**.

Potential higher-powered-machine presentation work should be opt-in and observational: viewport supersampling and filtering can be explored after fidelity completion, while visibility, picking, occlusion, interaction, and AI retain the original 64-step ray. A longer ray is not planned because source lookup wraps in the 128-cell authored map and could reveal duplicated or unauthored scenery.

## Combat checkpoint: 2026-09-14 unified friendly actor state

The executable has one current-mode field at actor `+0x1C`; `0x4F49C` reads it, evaluates one predicate, stores the selected transition, and invokes only the new handler. The runtime nevertheless retained a parallel private `RetreatMode` for kind 0, duplicating the mapped mode 1-8/10/11/13 graph and leaving public `ActorMode` at 10 while later states executed.

`AdvanceRetreatOrder` now sends both friendly kinds through the same `AdvanceDefendOrder` table and `BeginFriendlyMode` handlers. Hit transition writes only `ActorMode`, and effect-boundary re-entry observes that same field. The 165-line duplicate movement switch and shadow state are removed. Regressions now expose kind-0 mode `10→5→7→1→3→7` and `10→5→7→1→4→8` sequences as well as the previously covered kind-1 and hit routes. This is an architecture correction to the **Confirmed** actor `+0x1C` mapping; no original transition or timing rule changes.

## Home checkpoint: 2026-09-14 inactive `JUMP!!` region

`FOPTS.HAT` defines ten enabled regions and the executable's adjacent label table includes `JUMP!!` at index 7. Registration routine `0x302A0`-`0x30492`, however, installs action, hover-state, and release callbacks only for indices 0-6 and 8-9. The omission is repeated across all three callback families while label renderer `0x30768`-`0x307D4` can still index the complete table. Therefore region 7 is deliberately visible label-only scenery, not an unrecovered command.

`SceneHotspot.Interactive` now expresses that distinction: Home retains all ten HAT bounds and hover labels, while activation ignores `JUMP!!`. A focused test pins region 7 as the only inactive Home hotspot. This callback membership and runtime mapping are **Confirmed**; the placeholder action and its claim that further executable confirmation was needed are **Disproved**.

## Session checkpoint: 2026-09-10

The current migration branch builds with zero warnings under the enforced 1,000-line source limit. All 329 xUnit cases and 146 executable specifications pass. The Inno Setup 7.1.0 package compiles, installs into an isolated directory, launches through its generated **ReConqueror A.D. 1086** Start-menu shortcut, and removes that shortcut during uninstall. Repository policy passes with the expanded proprietary-image boundary.

First-person combat now uses the required original `SKIRMISH.PCX` shell, exact viewport and panel geometry, scene-driven walls and actors, animated foreground weapons, and blood effects. The corrected DOS/16M mapping places the LE data-page base at raw `0x4C254` and `Pal%d` at object 2 offset `0x7D28`; all 90 owned active palette families reproduce byte-for-byte. Executable paths confirm north/east/south/west surfaces, the complete weapon-to-combat-row permutation, break rolls, dice, penetration, reach, actor templates, state-completion gates, pointer coordinates, and fixed-point foreground trajectory formulas. Record-offset `0x40` replaces contacted objects and doors immediately. Pickup dispatcher `0x51E60` reads the selected scene block's selector at `+0x48`, not the player record; cases 5, 7, 9, and 10 implement +25 wealth, capped 2d6 healing, Chain Hauberk bit 6, and +12 bolts. The original four render-counted blood steps have no stable hardware-independent duration, so the runtime preserves their order through a monotonic compatibility timer. `docs/handover.md` continues with defeated-enemy loot, retainers, siege consequences, and broader AI rules. Official imported assets are a verified launch requirement; no placeholder gameplay path is supported.

## Strategic reactive-finder checkpoint: 2026-09-17

Reactive finder `0x3BB4C` is implemented inside the scheduler rather than supplied by the application. The runtime preserves authored property/player-slot order, strict normal `30/250` and property-7 `40/200` per-axis gates, London's half-open rectangle, exact auxiliary-lord contact, property bytes `+0x0D/+0x0E`, empty-garrison behavior, and one-shot alert output. The full gate passes 362 xUnit cases and 146 specifications.

The six-record player update and command construction are now recovered and implemented. `OriginalStrategicPlayerMovementSlot` persists exact route/target, cursor, grid, cooldown, terrain, position, and direction state for five army records plus avatar slot 5. `AdvancePlayerMovementPass` maps handler `0x11554`; `AdvancePlayerPass` maps `0x13168`; `OriginalStrategicPlayerCommands` maps `0x12044`-`0x12A48`, including `AE58` route state, player/enemy/three-division precedence, `0x1000`/`0x10000` tags, first-point priming, undo, and the 20-point input cap. The selected-record global is `AE64`; `AE6C` is the distinct distinguished army used by encounter/reactive/trigger gates, correcting the earlier label. The full gate passes 377 xUnit cases and 146 specifications. Next bind exact division/avatar inputs, fixed updates and presentation, then activate schema 2 and remove the dated adapter.
