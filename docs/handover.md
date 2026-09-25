# Reimplementation handover

## Baseline

The active implementation branch is `reimplementation-full-migration`. Before the current hostile-formation batch its verified remote tip was `6d1130a` (`Restore hostile actor pursuit and morale`); use Git and the full gate below as authority rather than assuming this note names the newest commit.

The authoritative gate is:

```powershell
& '.\Run Tests.bat'
```

## Latest completed combat recovery

Strategic player/enemy contact reaches the field battle of the spec area BATTLE: `ConquerorGame.StrategicEncounter` holds the captured contact, blocks later scheduler work, and settles only through the `Campaign` resolver methods. `parity/BATTLE.md` lists what differs from the original. The older `FieldBattle` screen is a separate provisional combat mode whose grid formation mechanics and command HUD are host-owned replacement policy and a priority fidelity gap.

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

The schema-2 replacement shape and settlement boundary are implemented but deliberately dormant. `OriginalStrategicCampaignState` deep-copies all 14 properties and 176 people, owns five indexed enemy slots plus six player movement records, and persists both motion families with generator/reactive counters, speed, profile, camera, list heads, and unique full-dword terrain mutations. `StrategicSchemaTwoMigration.Prepare` settles dated columns in slot order, returning complete forces to valid still-hostile schema-1 origins or journaling dispersal, then clears the incompatible roster. Schema 1 cannot supply a starting-home selection, so migrated state uses explicit sentinel `-1`; its missing camera uses deterministic `(0,0)`. Focused tests cover copied definitions, non-aliasing, complete JSON round-trip, settlement order/outcome, validation failures, and the fact that the schema version remains 1.

The route/grid provider gate is now closed. Core's `IOriginalStrategicResources` exposes immutable signed route points and projected full-dword cells; Game's `ImportedOriginalStrategicResources` eagerly decodes and caches forward/reverse forms of all 97 executable-selected paths plus `icon.jp`, failing startup on malformed input. `ConquerorGame` binds it to the initial campaign, both new-character routes, and loaded saves. Prepared replacement state is validated at binding, including exact waypoint-count agreement with the decoded route. Do not activate migration yet. Next drive the five slots from this provider on the fixed update, replace `AdvanceDays`' dated calls, and advance the schema only after load/save/runtime tests pass.

Do not wire named campaign locations as live player records; the six records of `player_forces` (RULE-STRATEGY-010) are persisted and executable. Next bind the yearly brigand order and the orders from the king (RULE-STRATEGY-016), fixed updates, and presentation before schema 2 activation. Do not push later continuation work to `main` without a new explicit request.

## Safety and repository rules

- The original installation at `C:\GOG Games\Conqueror AD1086` is read-only reference material.
- The desktop game supports only a verified extraction from the supported official GOG release. `ImportedContentCatalog.LoadRequired` verifies the source hash, every manifest file, and all mapped runtime dependencies before the graphics loop; do not restore assetless or placeholder gameplay.
- Never commit `analysis/original/`, imported `UserContent`, decoded media, screenshots from proprietary assets, or extracted executables.
- Keep compiled C# files at or below 1,000 lines. `tools/Conqueror.Inspect/Program.cs` and `tests/Conqueror.Tests/ResourceAndDefinitionTests.cs` are already exactly at that limit; split new work instead of extending them.
- Use `apply_patch` for source edits, run `git diff --check`, then run the full gate before committing.
- Preserve unrelated worktree changes and use explicit paths when staging.
