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
confirms the intent of Defend, Attack, and Follow: Defend attacks only inside
the surrounding 3x3 neighborhood, Attack seeks the nearest hostile, and Follow
moves toward the player target. The earlier claim that the public Retreat
command directly invokes away-vector handler `0x50020` is Disproved. Requested
mode 10 instead transitions friendly kind 0 to preserved-heading mode 5 and
kind 1 to mode 4 after acquisition. Acquired melee Attack mode 8 and Follow
mode 16 share handler `0x4FE76`; the runtime routes both through the recovered
`0x112` sub-cell movement effect. Melee Retreat uses mode 5's `0x142` effect
and flag-`0x40` left-turn collision response. Kind-1 mode 4 reacquires through
`0x4F98D`; transition `0x4E6D3` then chooses ranged mode 11 or fallback mode 1.
Mode-11 handler `0x5010B` aims at the returned actor and requires ray distance
strictly below raw combat-row contact column 4 at object-2
`0xCE24 + 28 * row`; rows 23/24 double the attack effect interval. The runtime
activates this bowman Retreat attack route through a bounded line-of-sight
bridge. Defend does not seek. Attack's no-target wandering, exact fixed-point
ray equivalence, formation/path selection, and thinker cadence remain
Provisional.

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
state and prior-command resumption. Cardinal heading and sub-cell stepping are
recovered below; the current floor-plane unprojection, actor convergence, and
unmapped collision/path behavior remain Provisional.

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
movement takes four ticks/800 ms per cell. Multi-actor destination behavior
remains Provisional. The same timing and offset mapping is active for acquired
Attack mode 8, Follow mode 16, and melee Retreat mode 5; it is not yet
established for every actor mode.

Mode-12 route and collision handling is narrower than that general caveat.
Heading helper `0x445C4` followed by `(heading + 0x20) & 0xC0` aims directly
at the destination at each three-tick effect boundary; exact diagonal ties
choose the clockwise cardinal. Scheduler `0x53425`-`0x535E5` tests the
neighboring map block's behavior bit `0x02` only beyond the strict
`-0x59..0x59` band. The descriptor stores flags `0x142`, but mode 12 rewrites
them to `(flags & 0xA7) | 0x10 = 0x112` at `0x4FFD1`-`0x4FFDA`.
On rejection, `0x53CC9`-`0x53D5F` therefore preserves the actor's offset and
heading, terminates that live effect, and returns immediately to actor thinking;
the next scheduler update creates a fresh directly aimed effect and retries.
The `0x40` left-turn branch belongs to the unmodified `0x142` effect family,
not mode 12. Open-path completion is evaluated only after its three-tick effect
returns to the actor thinker, even if the actor entered the target cell
mid-cycle. Carry
the original bit-2 blocker separately from visual tile labels and update it
when authored object states change. Broader modes, corner interaction,
multi-actor destination behavior, and exact ground-ray coordinates remain
provisional.

Public Retreat command handler `0x5744B` resets current mode, requests mode 10,
and calls transition helper `0x4E5F0`. On the following acquisition pass,
friendly kind-0 transition `0x4E76B` chooses mode 5 when a visible opponent was
found and mode 2 otherwise; friendly kind-1 transition `0x4E805` chooses mode 4
or mode 2. Kind-0 mode-5 handler `0x4FDCD` preserves actor heading and rewrites
the movement flags to `(flags & 0xA7) | 0x40`, leaving descriptor `0x142`
unchanged. When collision reaches `0x53C13`-`0x53C6C`, it clears the colliding
sub-cell axis and turns to
`(((heading + 0x20) & 0xC0) - 0x40) & 0xFF`, the cardinal direction to the
left; unlike flag `0x10`, the effect continues. Preserve this state route and
do not identify handler `0x50020` as the command's direct action. Kind-1 mode 4
repeats acquisition and reaches ranged mode 11 through `0x4E6D3`; `0x5010B`
uses a strict raw contact-distance gate and doubled ranged effect interval.
Exact fixed-point ray-visible acquisition and later mode-5 formation
transitions remain Provisional.

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
