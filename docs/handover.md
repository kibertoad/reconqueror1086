# Reimplementation handover

## Baseline

The active implementation branch is `reimplementation-full-migration`. Before the current hostile-formation batch its verified remote tip was `6d1130a` (`Restore hostile actor pursuit and morale`); use Git and the full gate below as authority rather than assuming this note names the newest commit.

The authoritative gate is:

```powershell
& '.\Run Tests.bat'
```

The current strategic-movement mapping gate passes 329 xUnit cases and 146 executable specifications with zero build warnings. Repository policy passes; re-run the gate after checkout rather than inferring current health from this note alone.

## Latest completed combat recovery

Strategic player/enemy contact now reaches the recovered resolver instead of
leaving the map with a placeholder handoff notice. `ConquerorGame.StrategicEncounter`
holds the captured six-counter contact, blocks subsequent player/hostile
scheduler work, uses the exact five `0x256FC` menu rectangles, and renders the
owned `BATTLE.PCX`/`MEN8.CSF` source assets through the existing exact session
draw helpers. Its automatic exit and interactive terminal path settle only
through the corresponding `Campaign` resolver methods; the generic
`FieldBattleSession` is not involved. The tactical event producer, user-facing
meanings of the four menu codes, caller-owned score modifier, and presentation
scaling are still Provisional. The older `FieldBattle` screen is a separate
provisional combat mode: it now requires and renders the same owned
`BATTLE.PCX`/`MEN8.CSF` assets rather than falling back to generic art, while
its grid formation mechanics and command HUD remain host-owned replacement
policy and a priority fidelity gap.

The latest sequence of commits is:

- `6c776de` follows explicit scene state targets rather than adjacent records.
- `e9a95b2` limits state-target validation to reachable actionable objects.
- `273b214` restores the 25-row weapon dice/penetration calculation and ten combatant-template armor/health pairs.
- `fc69434` decodes each actor's scene-selected combat row and uses it for successful enemy damage.
- `1fef117` uses the same executable contact-distance column for player and enemy reach, including clear-lane ranged actors.

## Exact continuation point

Continue priority 1 in `implementation-plan.md` with broader actor AI. The first-person combat mapping is in `spec/` (area ASSAULT, with the raycaster in VIEW), and `parity/ASSAULT.md` lists what the rebuild still lacks. The largest open items are the transition-table entries RULE-ASSAULT-008 lists as not read, the mode value the orders reset to (RULE-ASSAULT-004), and the view row the actor rays test (RULE-ASSAULT-010).

The ignored `executable-data-xrefs.txt` was most recently regenerated for object-2 offset `0xD4D0` (`player_index`). Promising references include `0x5191B`, `0x5192A`, `0x550D8`, and the `0x573xx`/`0x57Fxx` families. A follow-up inspector run targeting `0x51880,0x55080,0x572E0,0x57F40` was interrupted; treat its output as incomplete and rerun only the focused addresses needed.

Example owner-local command:

```powershell
$env:NUGET_PACKAGES='C:\Users\kiber\.nuget\packages'
dotnet run --project tools\Conqueror.Inspect --no-restore -- 'C:\GOG Games\Conqueror AD1086' 'analysis\original' --disassemble=0x51880,0x55080
```

## Remaining high-priority gaps

1. Click-selected world-object targeting remains part of broader mouse-command recovery (RULE-ASSAULT-005). Keyboard actions keep the nearest projected target as an accessibility fallback.
2. Continue actor movement and broader AI recovery from evidence, starting with the transition entries that RULE-ASSAULT-008 lists as not read and the later friendly formation outcomes.
3. Continue exact route recovery and bind spies to autonomous enemy-army movement, then continue estate tile mapping and remaining sound/event bindings as ordered in the migration plan. Spy cost/lifetime and `JUMP!!` are closed by the executable evidence described below.

## Combat rendering notes

The actor ray can traverse 64 wrapped source-map cells beyond the cropped runtime layout, and pass-through/state-target candidates may therefore use textures that are not currently rendered. The runtime now retains decoded CPU texture sources for every structural face and every possible kind-4 heading sector in the imported scene, while creating GPU textures only for visible layout/actor/object requirements. This closes the subsequent melee-training failure on texture 11 without inflating the render texture set. `DynamixSceneBlock.RaycastTextureReferences` and its structural, unmirrored, and mirrored-sector regression preserve that acquisition-source closure.

The follow-up texture-3 failure exposed two separate assumptions. Acquisition dependencies must originate in placed map blocks and only follow ray-eligible state targets; unused definitions can legally name textures absent from the active archive. Separately, actor color normalization selects walk families 64/96/128 that sparse scene archives do not each repeat. The owned release's `CONQUER/DEFEND2.RES` supplies the complete high-resolution 64-138 combat actor atlas, with active scene entries layered over it. `SiegeTextureDependencies` and `LoadCombatTextureSources` implement and test both rules. Imported siege drawing no longer has rectangle, flat-color wall, synthetic backdrop, or substitute-shell fallbacks; missing original art is a startup error on the single supported path.

The first atlas-enabled run exposed renderer-only white borders, placement, pose, and backdrop errors. Scene/CSF transparent pixels now upload as premultiplied zero rather than palette RGB with alpha zero. `ProjectActors`/`ProjectObjects` use the recovered viewport-width lateral basis, imported actor layout uses one projected 8.8 cell plus decoded lower/upper elevations, and render sectors use actor heading minus viewer heading. This replaces the former 60-degree/aspect-ratio/bearing approximations and also gives keyboard attacks the projected actor center. A follow-up complete blitter trace **Disproves** the provisional four-view `BackImage` crop: `0x43B8C` uses `heading * 3`, panorama wrap, and stored-horizon alignment, yielding cardinal x `0/192/384/576` and only source rows 141-199 in the upper 59 rows of the 167x117 aperture. `BackdropSlice` now implements that path. The 200 ms movement descriptor ticks were not changed: they already use elapsed time, strict deadlines, retained offsets, and an 800 ms sustained straight-cell cadence rather than CPU throughput.

An optional enhanced renderer may eventually supersample the viewport or improve presentation filtering, but it should retain the original 64-step compatibility ray for visibility, picking, occlusion, and AI. Extending the ray is not presently recommended: the source lookup wraps within the authored 128-cell map and could expose duplicated or unauthored space. This is a post-fidelity option, not a change to original-game claims.

## Home `JUMP!!` checkpoint: 2026-09-14

Home callback setup `0x302A0`-`0x30492` registers each of its three callback families for `FOPTS.HAT` regions 0-6 and 8-9, omitting region 7 every time. The complete label renderer still indexes `JUMP!!` at region 7, and the HAT keeps that region enabled. `SceneHotspot.Interactive` therefore leaves its bounds and hover label intact but suppresses activation; the former placeholder notice is removed. The other nine Home mappings remain active and are now Confirmed by their relocated handler registrations.

War Planning action `0x36720` requires temporary wealth `1A090 >= 0x50`, subtracts 80 through `0x37340`, and increments the pending assignment count at `1A0EC`. Commit `0x36AFC` tries each pending assignment through `0x38C68`; the first sets the single live-spy flag `AE68`, while another live assignment is rejected. Strategic update `0x3C290` calls report routine `0x38C84` immediately before movement update `0x3C088`. The report scans five `0x118`-byte records at object-2 `+0x1AB48` in slot order, retains the live spy while none is active, and consumes it when the first active record is considered. `StrategicEnemyMovement` and `StrategicSpyReport` persist this five-slot, report-before-advance path; monthly settlement no longer manufactures unknown-garrison intelligence. Follow-up tracing maps the record, property/person tables, named rows, strict generation gate, ordinary force formula, mode-2 waypoint state, and completion/retarget behavior. Route builder `0x4A070` selects `rt_<low>_<high>.rat` and reverses its pairs for high-to-low travel; only self-routes and Cambridge/Dunster are absent. Each selected file is an exact little-endian count followed by signed 32-bit x/y pairs. `StrategicRouteDecoder` bounds and decodes that layout, `OriginalStrategicMovement.PropertyRouteResources` exposes the 90 canonical resource identities, and supported startup requires them all. Route flag 0 is now closed: new-game helper `0x43670` chooses one of seven starting-home persons, stores selector `C9D0`, and `0x4A070` loads the corresponding unreversed `sc_0.rat`-`sc_6.rat`; `StartingRoutes` exposes the exact person/name/group/assignment/coordinate/resource bindings and startup requires all seven. The surrounding generator branch policy is mapped: absent/present property-list branches pass route flags 0/1 respectively, mode 2 falls back to mode 1, active records receive authored-slot-order pursuit attempts, and the reactive path has exact property-7, existing-pursuer, player-slot, counter, and 0/1-of-6 gates. The constructor uses its first free slot. Calendar helper `0x3866C`, setter `0x3E894`, and tables `B720/B750/B760` identify profiles 0-3 as Summer/Autumn/Winter/Spring, with Winter alone selecting reduced terrain speeds. Routed cadence maps tile IDs through the exact 331-byte lookup at `+0xAF78`, profiles through `+0xB610/+0xB704`, exact 30-float tables at `+0xB614/+0xB68C`, kind-9 deactivation, and a strict horizontal `< 50.0` application gate. The shared multiplier `+0xAEEC` is initialized to 1, bounded to 1-15, and consumed once per eligible strategic loop pass; elapsed milliseconds are Disproved. GOB resource 292 `icon.jp` is the exact 80-by-80-projection, 200-by-400 dword terrain grid serialized column-major; `temp.jap` is its mutable scratch copy. Cell helper `0x63E20`, viewport adapter `0x629B0`, and picker `0x6E5A0` now close the inverse mapping: route anchors are `(80*(row+1),20*(column+1))`, rendered cell centers alternate by column parity, and inclusive diamond pixels use wrapped current-camera draw order. All 8,304 points in the complete 102-resource owned `rt_*/sc_*` population project successfully; 210 lie on an inclusive edge, and 34 occurrences at six distinct coordinates change cell between opposite camera extremes. `StrategicWorldGridDecoder`, `StrategicWorldProjection`, the catalog integration, derived census, required-resource gate, and focused tests preserve these decisions. Generic destination reinforcement is Disproved. `architecture.md` defines the required non-bijective schema-1-to-2 settlement and exact persisted state. The runtime's incompatible indices and dated model remain Provisional; next implement schema 2, persist camera state, and make decoded points drive movement before removing the adapter. Continue estate tiles and event audio afterward.

The schema-2 replacement shape and settlement boundary are implemented but deliberately dormant. `OriginalStrategicCampaignState` deep-copies all 14 properties and 176 people, owns five indexed enemy slots plus six player movement records, and persists both motion families with generator/reactive counters, speed, profile, camera, list heads, and unique full-dword terrain mutations. `StrategicSchemaTwoMigration.Prepare` settles dated columns in slot order, returning complete forces to valid still-hostile schema-1 origins or journaling dispersal, then clears the incompatible roster. Schema 1 cannot supply a starting-home selection, so migrated state uses explicit sentinel `-1`; its missing camera uses deterministic `(0,0)`. Focused tests cover copied definitions, non-aliasing, complete JSON round-trip, settlement order/outcome, validation failures, and the fact that the schema version remains 1.

The route/grid provider gate is now closed. Core's `IOriginalStrategicResources` exposes immutable signed route points and projected full-dword cells; Game's `ImportedOriginalStrategicResources` eagerly decodes and caches forward/reverse forms of all 97 executable-selected paths plus `icon.jp`, failing startup on malformed input. `ConquerorGame` binds it to the initial campaign, both new-character routes, and loaded saves. Prepared replacement state is validated at binding, including exact waypoint-count agreement with the decoded route. Do not activate migration yet. Next drive the five slots from this provider on the fixed update, replace `AdvanceDays`' dated calls, and advance the schema only after load/save/runtime tests pass.

The three-mode motion kernel is now active behind that still-dormant replacement boundary. Dispatcher `0x3C088` calls generation before scanning slots 0-4; `AdvanceMovementPass` implements the scan but deliberately leaves generation and completion handling to the next shell. Direct handler `0x38D78` uses truncation helper `0x63EC0`, exact double `+0x4F97 = 0.9`, terrain/profile/speed scaling, refreshed projected cells, and modulo-byte origin return on kind 9. Routed handler `0x4A300` advances at most one waypoint with strict `< 6` tolerance or direction crossing, normalizes with integer length, samples from the prior scaled direction, and applies both components only below exact horizontal limit `+0x7389 = 50.0`. Pursuit handler `0x3908C` proves `+0x14` is a player movement-slot target, not a location; it aims from the prior prospective position, retains normalized direction, and changes mode 3 to direct mode 1 with one unscaled originward step on kind 9. Saved terrain mutations override the immutable cell dword. Next implement the generator-first scheduler shell, exact constructors and completion/contact outcomes, supply five live player-army targets, then wire one pass to the fixed game update and activate schema 2. Do not push this continuation batch to `main`.

The generator-first scheduler shell is now implemented and supersedes that continuation point. `AdvanceSchedulerPass` constructs into the first free slot, immediately permits a new slot to move, and interleaves completion/contact handling before the next physical slot. It includes both force initializers, direct grid auxiliary-person lookup, encounter stop, matched-origin byte reinforcement, teardown, and exact retargeting. Corrected disassembly **Disproves** the earlier ordinary force expression: each type is `household + floor(floor(lord rating / 4) / 3)`, with the original `0/0/1` zero minimum. The executable also has an uninitialized finder-output read in the timed branch; the clean runtime requires a real detection from the current pass. The next boundary is live application integration: source the five player targets/global inputs/reactive detection, preserve report-before-update, drive the fixed update, hand encounters to presentation, activate schema 2, and retire the dated adapter. Do not push this continuation batch to `main`.

The reactive-finder input is now closed and supersedes that continuation point. `0x3BB4C` scans properties 0-13 then active player armies 0-4; it uses strict per-axis normal `30/250` and property-7 `40/200` near/approach limits, London's half-open rectangle `x=[700,12640), y=[600,4560)`, or exact current-cell auxiliary identity matching the property's lord. Near contact sets property byte `+0x0D`; approach sets one-shot `+0x0E` and emits `OriginalStrategicPropertyAlert`. Empty garrisons stay marked but do not construct. `AdvanceSchedulerPass` now derives this result from live target records, with focused ordering, threshold, London, lord-cell, and empty-garrison tests.

Do not wire named campaign locations as live player records. The six records are persisted and executable: `0x11554` drives tagged-target or 21-pair inline-route movement, and outer `0x13168` moves each record before strict `<30` enemy contact, selection, avatar cooldown alerts, and the distinguished record's five-cell trigger. Input `0x122AC` prefers player then enemy then temporary division hits, tags targets with `0x1000`/`0x10000`, and accepts 20 authored points despite 21-pair physical storage. Bootstrap `0x110E8` clears the selected starting person's assignment, marks all six complete, activates only avatar 5 at its exact route anchor, writes `AE64/AE6C=5`, and leaves independent count `AE5C=0`. Join/leave `0x1133C/0x113A4` is active through campaign membership. Constructor/removal `0x12CAC/0x12DD0` now preserves the six-success boundary, exact home formation offsets, deliberately asymmetric stale-command clearing, first-active replacement, and distinguished-record avatar restoration. Next bind temporary-force inputs, fixed updates, and presentation before schema 2 activation. Do not push later continuation work to `main` without a new explicit request.

## Safety and repository rules

- The original installation at `C:\GOG Games\Conqueror AD1086` is read-only reference material.
- The desktop game supports only a verified extraction from the supported official GOG release. `ImportedContentCatalog.LoadRequired` verifies the source hash, every manifest file, and all mapped runtime dependencies before the graphics loop; do not restore assetless or placeholder gameplay.
- Never commit `analysis/original/`, imported `UserContent`, decoded media, screenshots from proprietary assets, or extracted executables.
- Keep compiled C# files at or below 1,000 lines. `tools/Conqueror.Inspect/Program.cs` and `tests/Conqueror.Tests/ResourceAndDefinitionTests.cs` are already exactly at that limit; split new work instead of extending them.
- Use `apply_patch` for source edits, run `git diff --check`, then run the full gate before committing.
- Preserve unrelated worktree changes and use explicit paths when staging.
