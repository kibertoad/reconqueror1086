# Repository agent instructions

## Original technical-design documentation

The final reimplementation deliverable includes a documented technical map of
the original game, not only working replacement code. As implementation work
progresses, update the relevant files under `docs/` with:

- original executable/resource addresses, fields, table layouts, state
  transitions, and subsystem relationships discovered during analysis;
- recovered formulas and values, labeled Confirmed, Corroborated, Provisional,
  or Disproved;
- the source of each original-game claim, such as executable control flow,
  decoded resource data, the owned manual, or controlled observation;
- the corresponding reimplementation types, methods, definitions, and tests so
  readers can trace original behavior into the new architecture.

Keep proprietary bytes and generated analysis artifacts out of Git. Commit only
independently authored technical descriptions, compact facts, hashes, and legal
fixtures. A gameplay batch is not complete until its new technical mappings and
evidence have been documented alongside the implementation.

The current combat map establishes that loader `0x51560` recognizes actors by
behavior bit `0x80` plus selector 1, scans map cells x-major, and promotes the
first friendly to player `D4D0`. Across all 1,002 placements in the supported
official `MELEE*`/`DEFEND*` population, templates 0-2 are friendly and
3/5/8/9 hostile. Preserve this authored partition and cap retainers to the
available authored positions; do not infer sides from actor names.

First-person retainer commands are executable-mapped: Defend mode 2, Attack
mode 6, Retreat mode 10, and Follow mode 16 with player `D4D0` as target.
Commands affect selected living friendlies, or all living friendlies when none
is selected, then clear selection. Preserve these identities and distinguish
the retainer Retreat order from leaving the battle. Executable state dispatch
confirms the intent of each mode: Defend attacks only inside the surrounding
3x3 neighborhood, Attack seeks the nearest hostile, Follow moves toward the
player target, and Retreat moves away from its hostile target. The runtime may
use documented provisional grid stepping for these intents, but exact
fixed-point movement, collision/path selection, and thinker cadence must not be
presented as recovered until their executable paths are closed.

First-person pointer dispatch is also mapped. Handler `0x55524` consumes the
world hit returned by raycaster `0x470A8`: a friendly actor toggles selection;
a hostile actor becomes requested mode-8 target `+0x24` for selected
friendlies and consumes the click; with no selected friendly, the player acts
on that exact hostile; and an explicit-action object is accepted only while
ray distance is `< 0x280`. Preserve exact actor/object identity through input
dispatch. The current renderer-consistent billboard/depth/alpha picker is an
implementation mapping, not yet a claim that the original raycaster's full
fixed-point ground/cell traversal has been reproduced.

Empty-ground clicks use requested mode 12. Dispatcher `0x555BE`-`0x5562E`
writes the raycaster's integer coordinates to actor `+0x28/+0x2C` for selected
actors and clears selection. Acquisition branch `0x4FD7B` completes when both
coordinates match; handler `0x4FF53` derives heading from `target - current`,
clears actor target `+0x24`, and schedules movement. Preserve that destination
state and prior-command resumption. The current floor-plane unprojection and
cardinal stepping remain provisional until the full fixed-point map ray and
collision/path behavior are recovered.

Actor blocks have two distinct signed effect selectors: `+0x2E` selects the
visual state-completion effect, while `+0x46` selects movement. Mode handler
`0x4FFBC` reads the latter through the high word at `+0x44`. Across all 144
placed actor bases (1,002 placements), movement selectors 7/10 both resolve to
three ticks at 200 ms, flags `0x142`, local 8.8 delta `(64,0)`, and surface
stride 5. Loader `0x5185F`/`0x5186F` centers actor records at
`(cell << 8) + 0x80`; cloned kind-4 block offsets `+0x24/+0x28` begin at zero
across the complete official population and retain movement remainder. Preserve
the strict post-deadline 200 ms tick independently of processor speed and input
frequency. From zero, offsets advance `0,64,128,192`, then strict `> 0x80`
crossing changes the cell and wraps to `-64`: 600 ms is the first transition
and effect-cycle duration, not a universal cell cadence. Sustained straight
movement takes four ticks/800 ms per cell. Route choice, collision deflection,
and actor modes other than selected ground travel remain provisional.

## Git push destination

The authorized canonical repository is
`https://github.com/kibertoad/reconqueror1086.git`.
Before every push, inspect the repository's configured push destination with
`git remote get-url --push origin` (and `git remote -v` when additional context
is useful), and verify that it resolves to this canonical repository. Push
through the configured remote name and an explicit refspec, for example
`git push origin HEAD:main`.

Never rewrite, replace, or temporarily override a remote URL in order to push.
This prohibition includes `git remote set-url`, changing `remote.*.url` or
`remote.*.pushurl`, and command-scoped configuration such as
`git -c remote.origin.pushurl=...`. If the configured destination is missing or
does not match the repository the user authorized, stop and ask the user to
correct or approve the remote configuration instead of modifying it.

## Post-commit orphan-process audit

After every commit in this repository, inspect running processes for orphaned
work created by this repository's tasks. Check at least PowerShell
(`powershell` and `pwsh`), Ghidra/Java, .NET (`dotnet` and `testhost`), the
Conqueror game, and any other process families that the agent launched while
building, testing, validating, importing, or analyzing this repository.

Reusable MSBuild nodes (`dotnet` running `MSBuild.dll` with `/nodeReuse:true`)
are expected background workers, not orphans. Do not stop or log them merely
because their spawning build process exited, their start time matches a
repository validation, or they remain idle after a build. They are exempt from
cleanup unless there is separate evidence that the process is malfunctioning
and must be stopped to complete this repository's work. Do not disable MSBuild
node reuse in routine build or test commands; keeping these workers available
makes subsequent builds faster.

Multiple reimplementation agents normally work in parallel, but they may work
on other games. A process may be treated as belonging to this work when its
command line, parent, task/session, or source paths show that it targets this
repository or the read-only reference installation/assets at
`C:\GOG Games\Conqueror AD1086`. Do not terminate a process merely because its
executable name matches. Preserve processes for other games, unrelated
user/IDE/system processes, and validation that is intentionally still running.
If repository/source ownership is uncertain, leave the process running.

Stop confirmed orphaned repository processes. Whenever any process is stopped,
append an entry to `orphanCleanupLog.md` containing:

- the local timestamp including UTC offset;
- each stopped PID and process name;
- its start time or task/session association when known;
- why it was identified as orphaned;
- any related process deliberately left running and why.

If the audit finds nothing to stop, no log entry is required.
