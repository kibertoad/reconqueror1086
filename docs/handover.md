# Reimplementation handover

## Baseline

The active implementation branch is `reimplementation-full-migration`. Before the current route/collision batch its verified remote tip was `28349b0` (`Restore actor sub-cell movement`); use Git and the full gate below as authority rather than assuming this note names the newest commit.

The authoritative gate is:

```powershell
& '.\Run Tests.bat'
```

The current hostile-AI implementation gate passes 271 xUnit cases and 146 executable specifications with zero build warnings. Repository policy passes; re-run the gate after checkout rather than inferring current health from this note alone.

## Latest completed combat recovery

The latest sequence of commits is:

- `6c776de` follows explicit scene state targets rather than adjacent records.
- `e9a95b2` limits state-target validation to reachable actionable objects.
- `273b214` restores the 25-row weapon dice/penetration calculation and ten combatant-template armor/health pairs.
- `fc69434` decodes each actor's scene-selected combat row and uses it for successful enemy damage.
- `1fef117` uses the same executable contact-distance column for player and enemy reach, including clear-lane ranged actors.

Owned-scene evidence covers 12,784 combat-scene blocks. The 144 placed actor definitions account for 1,002 placements; every actor template and combat row used by that population is bounded. Exact numeric reports and extracted artifacts live below ignored `analysis/original/` and must never be committed.

## Exact continuation point

The former `0x50020` unreachability conclusion is Disproved. It considered
initializer state but missed the dynamic hit-state route. Mode-14 handler
`0x503EC` compares stored-target health with newly reduced source health before
constructing the hit effect; mode 15 repeats it at `0x504F2`. Shared transition
`0x4E77E` selects mode 13 for a stronger target and cancels the prior live
effect. Predicate `0x4FD9A` then branches at source health
6 through transition `0x4E7A4`: mode 2 at or above 6, mode 10 below it. Handler
`0x50020` is therefore active and performs the exact non-cardinal away heading,
independent `1.5` x/y scaling, and `0x112` movement effect.

Shared mode-12 destination occupancy and deterministic compatibility ordering
are closed. All 144 placed actor bases and their 576 adjacent state
blocks use behavior `0x87`; scheduler `0x53624`-`0x53694` swaps the actor into
its new map cell before another effect runs. One actor can occupy a requested
coordinate, while later arrivals stop at blocker bit `0x02` and retry their
direct effect with retained offset. Runtime occupancy plus focused two-actor
tests preserve the rule. The original exact-tie winner is not processor-stable:
thinker `0x50524` begins at cursor `D5C4`, constructor `0x4C7F4` takes the first
free effect slot, and the cursor advances on each unrestricted main-loop pass.
Runtime authored order is the documented stable compatibility tie-breaker and
is regression-tested across update subdivisions.

Continue priority 1 in `implementation-plan.md` with broader actor AI after the now-closed hit/morale branch. The Retreat route includes mode-5/7 regroup, mode-1/3/4 support, renewed mode-8 pursuit, strict raw-row contact, and completion-gated mode-11 damage. When mode 14 records a hit, the actor compares its stored target with its reduced health; a stronger target selects mode 13, and a low-health actor enters mode 10 and moves away using the exact `1.5` non-cardinal formula. The former mode-11 attack-rejection attribution is Disproved. Boundary, cancellation, and integer-rotation regressions are active. The GameFAQs FAQ has no comparable internal formula. Movement retains strict 200 ms ticks, signed 8.8 offsets, collision families, and a processor-independent clock.

The adjacent supported-hostile slice is now implemented. Imported templates
3/5/8/9 retain their initial profiles and kinds 2/4/2/7. Kind-2/4 table
fixups share transitions `0x4E71F`, `0x4E745`, `0x4E77E`, and `0x4E851`
for modes 6/8/11/13, while kind 7 shares kind 2. `AdvanceHostileMovement`
runs visible player-side acquisition, exact direct pursuit, raw-row contact,
completion-gated strikes, hit-state prior-effect cancellation, health-6 morale
recovery, and low-health escape from a stronger stored target instead of randomized whole-cell
official enemy AI. Continue with hostile modes 1-3 and exact no-effect thinker
frequency; those remain Provisional.

The projected candidate helper, actor acquisition, and shared actor-movement paths are active. Switch table `0x34A0C`, the 31-usable `0x44F7C` contact arrays, block geometry fields `+0x10..+0x20`, traversal order, continuation, projection formulas, and primary dispatch precedence are closed. Acquisition `0x4F98D` adds authored-order scanning, heading `0x445C4`, centered ray `0x470A8`, exact identity through `0x4CEA0`, strict-nearest replacement from `0x7FFF`, and the `< 0x154` early exit. `OriginalSiegeProjection` and `OriginalSiegeActorAcquisition` implement these paths with heading, non-cardinal depth, alpha, identity, ordering, and tie tests. The movement slice at `0x5185F`-`0x51875` and `0x530D0`-`0x53D5F` includes direct boundary aiming, bit-2 collision, retained partial offset, early effect termination, effect-boundary arrival, and shared-destination occupancy. Earlier confirmed actor-versus-actor portions include:

- `0x4F10D`-`0x4F125`: attacker combat-row column 4 plus `0x40` is compared with fixed-point Manhattan separation.
- `0x4F2DC`-`0x4F30E`: the attacker's combat row supplies dice count, die sides, and penetration to the shared damage path.
- `0x4F2AC`-`0x4F2D6` confirms `random(200) < 100 + attacker skill - defender skill / 2`, plus 30 for a crossbow attacker within one cell of the player and 30 when attacker and defender face the same direction.
- Player combatant field `+0x34` is initialized at `0x58851`-`0x58887` as strength plus dexterity plus twice sword experience; field `+0x40` is strength plus stamina plus honor. The ten template attack-skill values are `50,70,60,50,50,50,50,50,70,85`.

The ignored `executable-data-xrefs.txt` was most recently regenerated for object-2 offset `0xD4D0`, the player combatant index. Promising references include `0x5191B`, `0x5192A`, `0x550D8`, and the `0x573xx`/`0x57Fxx` families. A follow-up inspector run targeting `0x51880,0x55080,0x572E0,0x57F40` was interrupted; treat its output as incomplete and rerun only the focused addresses needed.

Example owner-local command:

```powershell
$env:NUGET_PACKAGES='C:\Users\kiber\.nuget\packages'
dotnet run --project tools\Conqueror.Inspect --no-restore -- 'C:\GOG Games\Conqueror AD1086' 'analysis\original' --disassemble=0x51880,0x55080
```

## Remaining high-priority gaps

Kind-1 Retreat continuation is closed past the former mode-4 stopping point. The kind-1 table's mode-4 entry targets `0x4E6D3`, which selects ranged mode 11 on successful reacquisition or mode 1 on failure. Handler `0x5010B` aims at the returned actor, uses the `<= 0x154` close branch or raycaster `0x470A8`, and requires ray distance strictly below raw contact column `0xCE24 + 28 * combatRow`; rows 23/24 double the effect interval. Scheduler `0x53365` resolves damage through `0x4F070` at the final effect tick, and `0x53D47` immediately re-enters the actor thinker after cleanup. `ActorContactDistanceForCombatRow`, `PendingRangedTarget`, `RetreatOrderTarget`, and `RetainerRangedAttack` activate that Attack/Retreat path through the recovered imported-scene actor ray.

The older item 3 wording below is superseded for command dispatch, intent, visible actor/object clicks, selected-ground sub-cell presentation, shared-destination occupancy, no-target Attack, kind-1 Retreat, and kind-0 Retreat through mode-7 regroup. The shell band `x=4..209`, `y=175..187` and handlers `0x5700E/0x57168/0x572C5/0x5744B` confirm Attack 6, Defend 2, Follow 16 targeting player `D4D0`, and Retreat 10, with selected-or-all dispatch and selection clearing. Thinker/state paths `0x50524/0x4F49C`, archetype transition tables at object-1 offsets `0x3E43C/0x3E480`, and runtime dispatch tables `0x4F414/0x4F458` confirm Defend's 3x3 reaction, Attack's nearest-hostile search, acquired melee transition to mode 8, and Follow's approach. They also **Disprove** the former direct away-vector interpretation of public Retreat: kind-0 mode-10 transition `0x4E76B` chooses mode 5 after acquisition, kind-1 `0x4E805` chooses mode 4, and both choose mode 2 on failure. Mode 5 handler `0x4FDCD` preserves heading and installs flag `0x40`; scheduler `0x53C13`-`0x53C6C` clears the blocked axis and turns left. Same-side acquisition `0x4FC34` and transition `0x4E70C` then select mode 7; `0x4F8B0` and `0x4E732` enter mode 1 only below the strict `0x200` same-side ray threshold. Continue with mode 1 onward, exact thinker/effect tie ordering, and broader AI.

1. Finish exact foreground presentation. Pointer events load the cursor top-left and add `(9,9)` at `0x55DB8`-`0x55DE5`; the actor-contact path forwards that centered pair to foreground setup at `0x5590C`-`0x5591D`; runtime mouse/controller clicks inside the aperture now use those local coordinates, while keyboard actions retain the nearest projected target as an accessibility fallback. `0x54C07`-`0x54F12` supplies all row-family geometry, including the crossbow's one-frame-height rise from the viewport bottom; `0x54F98`-`0x5501E` supplies signed 8.8 motion; and `0x5529B`-`0x55448` proves rows 23-24 retain offset 2 and remain held instead of expiring. `SiegeForegroundTrajectory` covers every family, pointer scaling, frame subdivision, and persistent crossbow hold. The four-step blood effect remains the documented monotonic compatibility adaptation. Exact click-selected world-object targeting remains part of broader mouse-command recovery rather than foreground-coordinate provenance.
2. Placed pickup rewards are closed for the hashed release. Dispatcher `0x51E60` reads the selected block's low word at `+0x48`; the active player's 68-byte combatant record supplies only the recipient discriminator. Cases 5, 7, 9, and 10 consume 36 bags of coins for +25 wealth, 70 meals for capped 2d6 healing, 36 Chain Hauberks for equipment bit 6, and 36 piles of bolts for +12 ammunition. Scene arguments live at `+0x4A/+0x4C`; actor records reuse those words for template/row selection. The exact case bodies, global addresses, population census, runtime mapping, and tests are recorded in `original-findings.md` and `resource-formats.md`.
3. Continue exact actor movement and broader AI recovery from evidence. Selected mode-12 travel and supported hostile modes 4/6/8/10/11/13 use the imported per-tick 8.8 path and shared projection; their direct aim, strict bit-2 collision gate, live `0x112` stop/retry behavior, effect-boundary completion, contact, deferred damage, and morale escape are mapped. Hostile modes 1-3, later friendly formations, and exact no-effect thinker frequency remain open. Authored side selection is closed for the supported official scene population: loader `0x51560` uses behavior bit `0x80` plus selector 1, scans x-major, and promotes the first friendly to player `D4D0`; the complete 1,002-placement census partitions templates 0-2 as friendly and 3/5/8/9 as hostile. `ImportedSiegeLayouts` preserves those positions and `SiegeSession` caps them with the campaign formula. Campaign caller `0x39EAA`-`0x39F79` builds one contribution per strategic unit type as `min(floor(units / 3), 3)`, while `0x3A101`-`0x3A1CE` converts missing retainers into one-for-one strategic losses in Swordsmen, Halberdiers, Knights order. Raw surface/color normalization remains unmapped; defeated-enemy loot remains Disproved.
4. Continue exact route/`JUMP!!`/spy recovery, estate tile mapping, and remaining sound/event bindings as ordered in the migration plan.

## Safety and repository rules

- The original installation at `C:\GOG Games\Conqueror AD1086` is read-only reference material.
- The desktop game supports only a verified extraction from the supported official GOG release. `ImportedContentCatalog.LoadRequired` verifies the source hash, every manifest file, and all mapped runtime dependencies before the graphics loop; do not restore assetless or placeholder gameplay.
- Never commit `analysis/original/`, imported `UserContent`, decoded media, screenshots from proprietary assets, or extracted executables.
- Keep compiled C# files at or below 1,000 lines. `tools/Conqueror.Inspect/Program.cs` and `tests/Conqueror.Tests/ResourceAndDefinitionTests.cs` are already exactly at that limit; split new work instead of extending them.
- Use `apply_patch` for source edits, run `git diff --check`, then run the full gate before committing.
- Preserve unrelated worktree changes and use explicit paths when staging.
