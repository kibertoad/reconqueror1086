# Reimplementation handover

## Baseline

The handover baseline is `main` after the documentation checkpoint, with the preceding implementation tip at `1fef117` (`Use original enemy contact ranges`). At that implementation tip, local `HEAD` and `origin/main` were identical and the working tree was clean.

The authoritative gate is:

```powershell
& '.\Run Tests.bat'
```

The last completed implementation gate passed 140 xUnit cases and 146 executable specifications with zero build warnings. Repository policy also passed. Re-run the gate after checkout; do not infer current health from this note alone.

## Latest completed combat recovery

The latest sequence of commits is:

- `6c776de` follows explicit scene state targets rather than adjacent records.
- `e9a95b2` limits state-target validation to reachable actionable objects.
- `273b214` restores the 25-row weapon dice/penetration calculation and ten combatant-template armor/health pairs.
- `fc69434` decodes each actor's scene-selected combat row and uses it for successful enemy damage.
- `1fef117` uses the same executable contact-distance column for player and enemy reach, including clear-lane ranged actors.

Owned-scene evidence covers 12,784 combat-scene blocks. The 144 placed actor definitions account for 1,002 placements; every actor template and combat row used by that population is bounded. Exact numeric reports and extracted artifacts live below ignored `analysis/original/` and must never be committed.

## Exact continuation point

Continue priority 1 in `implementation-plan.md`: recover pickup rewards before broadening combat behavior. Campaign scene dispatch, door state replacement, pointer-to-foreground coordinates, melee and crossbow foreground trajectories, and processor-independent four-step blood presentation are now closed at the currently available fidelity.

The actor-versus-actor routine beginning at executable VA `0x4F070` is the active trace. Confirmed portions are:

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

1. Finish exact foreground presentation. Pointer events load the cursor top-left and add `(9,9)` at `0x55DB8`-`0x55DE5`; the actor-contact path forwards that centered pair to foreground setup at `0x5590C`-`0x5591D`; runtime mouse/controller clicks inside the aperture now use those local coordinates, while keyboard actions retain the nearest projected target as an accessibility fallback. `0x54C07`-`0x54F12` supplies all row-family geometry, including the crossbow's one-frame-height rise from the viewport bottom; `0x54F98`-`0x5501E` supplies signed 8.8 motion; and `0x5529B`-`0x55448` proves rows 23-24 retain offset 2 and remain held instead of expiring. `SiegeForegroundTrajectory` covers every family, pointer scaling, frame subdivision, and persistent crossbow hold. The four-step blood effect remains the documented monotonic compatibility adaptation. Exact click-selected world-object targeting remains part of broader mouse-command recovery rather than foreground-coordinate provenance.
2. Recover pickup reward semantics independently of the feedback callback. The field called `Behavior` is a bitmask: mask 19 pickups carry explicit-action bit `0x10`, but callback dispatcher `0x51E60` receives its selector from the active player's 68-byte combatant record, not the selected scene block. Callback target 19's sound ID `0x3FFEA` therefore does not identify a pickup reward. Explicit-action range and immediate offset-`0x40` replacement remain Confirmed and active. Door cells now use this same exact mechanism: 1,086 owned placements with bit `0x10` install their explicit floor/ground/wall/water target immediately, while 632 placements without the bit remain closed. The former timed adjacent-door model is removed.
3. Replace provisional retainer, loot, siege consequence, and broader AI rules from evidence.
4. Continue exact route/`JUMP!!`/spy recovery, estate tile mapping, and remaining sound/event bindings as ordered in the migration plan.

## Safety and repository rules

- The original installation at `C:\GOG Games\Conqueror AD1086` is read-only reference material.
- Never commit `analysis/original/`, imported `UserContent`, decoded media, screenshots from proprietary assets, or extracted executables.
- Keep compiled C# files at or below 1,000 lines. `tools/Conqueror.Inspect/Program.cs` and `tests/Conqueror.Tests/ResourceAndDefinitionTests.cs` are already exactly at that limit; split new work instead of extending them.
- Use `apply_patch` for source edits, run `git diff --check`, then run the full gate before committing.
- Preserve unrelated worktree changes and use explicit paths when staging.
