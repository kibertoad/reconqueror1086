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
kind 1 to mode 4 after acquisition. The complete mode-9/10 transition-column
map closes the remaining attribution: kinds 0-6 select `9/5`, `9/4`, `6/4`,
`9/10`, `6/4`, `9/10`, and `9/10` on acquisition success (kind 7 shares kind
2). Only mode-9 self-loops feed mode 9. Initializer `0x542F8` gives templates
0-9 current/requested/previous triples `4/4/4, 4/4/4, 4/4/4, 6/8/6,
4/10/6, 4/8/1, 1/4/2, 4/10/1, 6/8/4, 4/8/4`; templates 4 and 7 are absent
from every supported official placement. Handler `0x50020` is therefore
unreachable for the supported population and must not be activated
speculatively. Revisit it only if a newly supported, executable-corroborated
asset population supplies an entry route. Acquired melee Attack mode 8 and Follow
mode 16 share handler `0x4FE76`; the runtime routes both through the recovered
`0x112` sub-cell movement effect. Melee Retreat uses mode 5's `0x142` effect
and flag-`0x40` left-turn collision response. Kind-1 mode 4 reacquires through
`0x4F98D`; transition `0x4E6D3` then chooses ranged mode 11 or fallback mode 1.
Mode-11 handler `0x5010B` aims at the returned actor and requires ray distance
strictly below raw combat-row contact column 4 at object-2
`0xCE24 + 28 * row`; rows 23/24 double the attack effect interval. The runtime
activates this bowman Attack/Retreat route through a bounded line-of-sight
bridge. Do not apply mode-11 damage when the handler creates the effect:
scheduler `0x53365` calls `0x4F070` only when the live effect counter equals
its configured count, then `0x53D47` clears actor effect handle `+0x08` and
immediately recalls thinker `0x4F49C`. Preserve the strict doubled completion
gate, pending-target cancellation, and same-actor re-entry. Defend does not
seek. When mode-6 acquisition `0x4F98D` finds no
visible opponent, kind-0 transition `0x4E71F` and kind-1 transition `0x4E745`
both retain mode 6. Its handler `0x4FDCD` preserves heading and installs the
same `(flags & 0xA7) | 0x40` movement family as melee Retreat. The runtime
stores that family on the live movement effect so later order evaluation cannot
change its collision behavior mid-cycle. Exact fixed-point ray equivalence,
formation/path selection, and thinker cadence remain Provisional.

First-person pointer dispatch is also mapped. Handler `0x55524` consumes the
world hit returned by raycaster `0x470A8`. On primary flag 0, selected
friendlies first receive the hit contact coordinates as requested mode 12 and
consume the click; only otherwise does a friendly actor toggle selection, a
hostile become an explicit mode-8/player target, or an actionable object pass
the strict `< 0x280` distance test. Preserve this precedence and exact hit
identity. Runtime pointer candidates now use the original fixed horizontal ray,
wrapped 128-cell source lookup, neighbor probe order, pass-through stop bit,
state-target continuation, center/diagonal shapes, vertical bounds, and alpha.
Kind-4 pointer candidates use their live centered 8.8 position, negative-view
rotation, forward depth, ray-relative horizontal texture coordinate, heading
sector, behavior-bit-4 mirror, vertical bounds, and alpha in original insertion
order. Non-cardinal acquisition-ray integration remains Corroborated.

Requested mode 12 uses a projected surface contact, not empty ground.
Dispatcher `0x555AB`-`0x5562E` writes the raycaster's integer coordinates to
actor `+0x28/+0x2C` for selected actors and clears selection. Acquisition branch
`0x4FD7B` completes when both coordinates match; handler `0x4FF53` derives
heading from `target - current`, clears actor target `+0x24`, and schedules
movement. The destination may name a blocked contact cell; movement collision
decides whether it is enterable. Viewer setup at `0x5421F`-`0x542E4`
initializes elevation `0x80`, horizon `height / 2`, and ray width from the
viewport width. Raycaster `0x470A8` forms its horizontal basis as
`0x4000 + (((0x400000 / width) * (x - width / 2)) >> 8)` and traversal
`0x45158` normalizes the major axis to signed `0x100` for at most `0x40` map
steps. Switch table `0x34A0C` sends kind 0 to no candidate, kinds 1/4 to a
cell box, kinds 2/3 to center planes, and kinds 5/6 to opposing diagonals.
`0x44F7C` uses 32-slot arrays but stops at count `0x1F`, leaving 31 usable
contacts. `0x470A8` projects each block's
`+0x1C/+0x20` lower/upper elevations and samples its texture through `0x444E8`.
The former below-horizon floor-plane implementation is Disproved. GameFAQs FAQ
66730 is a trusted gameplay starting point but contains no internal projection
math and does not corroborate these formulas.

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
movement takes four ticks/800 ms per cell. The same timing and offset mapping is active for acquired
Attack mode 8, no-visible-target Attack mode 6, Follow mode 16, and melee
Retreat mode 5; it is not yet established for every actor mode.

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
when authored object states change. Broader modes, projected candidate
surfaces, source-map wrapping, and exact corner interaction remain provisional.

Every placed actor base and all 576 adjacent base/attack/hit/death state blocks
use behavior `0x87`, so actor bit `0x02` blocks the cell. At a cell crossing,
scheduler `0x53624`-`0x53694` restores the mover's saved underlying block to
its former cell, captures the destination block, and installs the actor block
there before the next live effect is processed. Multiple mode-12 actors sent
to one coordinate therefore never stack or fan out: one occupies the cell and
later arrivals hit the strict collision band, stop their live `0x112` effect,
and retry. Preserve single-cell occupancy and persistent retry. The original
does not define a processor-stable winner for an exact simultaneous tie:
input installs the orders before scheduler `0x530D0` and thinker `0x50524`,
the thinker starts at persistent cursor `D5C4`, and constructor `0x4C7F4`
takes the first free effect slot. Because `D5C4` advances once per unrestricted
main-loop pass, precedence depends on processor and input timing. Use authored
retainer-list order (official import order is x-major) as the explicit,
deterministic compatibility tie-breaker; never derive it from render cadence.

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
