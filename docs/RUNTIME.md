# Runtime access

The owner granted standing authorization on 2026-10-09 for programmatic DOSBox-X
installation/builds, debugger verification and isolated CONQUER.EXE mapping/RNG
probes. Agents need no renewed owner permission for these actions; see the
standing authorization in [AGENTS.md](../AGENTS.md). Capability verification,
supported-state writes, exclusive run locking and owned-process cleanup remain
required. Authorization alone does not establish any capability in the table.

## BLD-GOG-EN

The owned edition runs in GOG's bundled DOSBox 0.74, with its documented S3 SVGA
and memory configuration (BLD-GOG-EN). Official-version provenance and the owned
source fingerprint are verified in [SOURCE-EDITIONS.md](SOURCE-EDITIONS.md).

The 2026-09-30 capability assessment used a separate writable DOSBox C: drive
under ignored analysis/documentation-audit/runtime/, containing copies of the
installed data and configuration. The original disc was mounted read-only as
D:. The owner installation was not the writable drive. A bounded launch reached
DOSBox's Program: CONQUER state; direct-window capture recorded that launcher's
client pixels. This proves launch and capture transport, not a repeatable
prescribed gameplay state or a gameplay observation.

Here person means the documented interactive workflow requires a person; none
means no verified adapter in this repository/environment provides the capability.
It describes the current environment, not every possible instrumentation method.

| Capability | Who | Evidence or attempt | What would change it |
|---|---|---|---|
| Start and reach a prescribed gameplay state | person | Owned DOSBox launch works; no state-readable unattended probe exists | Implement and verify a build-specific state probe |
| Send game input: keyboard | person | SRC-MANUAL describes keyboard input; Windows accepted a posted Escape message, but game consumption was not verified | Verify keyboard input through an observable game-state transition |
| Send game input: mouse | person | SRC-MANUAL describes mouse input; no mouse message has been tried on its own, so this answer is carried over from the earlier single input answer and names no attempt | Try a mouse message and verify it through an observable game-state transition |
| Read guest memory and control breakpoints | agent | DOSBox-X 2026.10.01 native TCP debugger: synthetic memory roundtrip and breakpoint hit verified; original relocated DOS entry and protected-mode guest memory read verified on 2026-10-09 | Map live LE game addresses before using spec fields or game-function breakpoints |
| Map live guest memory to spec fields | agent for loaded LE objects; RNG state checked natively | FND-RNG-004 and tools/live_mapping.py: per-run full code/relocation/descriptor checks plus native RNG controls | Verify heap-object provenance and supported field identities before a gameplay-state probe |
| Load a patched save | none | No original-save patch/load workflow is verified; FMT-SAVE-001 through FMT-SAVE-005 retain gaps | Complete the needed structure and verify a reversible patch/load experiment |
| Capture client frames | agent | Direct PrintWindow capture verified against an offscreen synthetic renderer and owned DOSBox client; blank/unsupported results fail | Recheck each different renderer and interpret each frame before using it as evidence |
| Capture sound | person | Bundled DOSBox documentation specifies Ctrl-F6 WAV capture; no unattended audio adapter is verified | Verify a repeatable audio recording case |
| Replay a recording with game draws | none for full-game rebuild replay | Bounded original journals and RNG-formula replay are verified; actual rebuild replay is not | Complete caller ownership and verify draw-by-draw replay against the rebuild |
| Call a function in an emulator harness | agent | Unicorn 2.1.4; fingerprinted BLD-GOG-EN LE loader and RULE-RNG-001 function smoke comparisons verified on 2026-10-09 | Add explicit service models only for functions that need them; unsupported accesses stop |

The bundled DOSBOX/Documentation/dosbox_README.txt also specifies Ctrl-F5 PNG
capture and Ctrl-Alt-F5 AVI capture. Those recordings do not supply game RNG draws
or prove deterministic original/rebuild replay.

## Capture and run discipline

The old desktop-copy helper could sample an occluding application. Its invalid
assessment capture was discarded. [Capture-OriginalWindow.ps1](../tools/Capture-OriginalWindow.ps1)
now captures only the selected window's client through PrintWindow, with no
desktop fallback. A uniform frame is rejected because renderer support and state
cannot be established from it. Unsupported renderers need a verified emulator
capture path or a person; do not substitute desktop sampling. Its
`checkpoint.json` (schema version 3) names each frame by the `xxh3` the spec cites
it by, computed after the burst by `tools/evidence/xxh3.mjs`, so the script checks
for Node.js and the installed reader package before it captures anything.

Original runs take an exclusive machine lock at
C:\ProgramData\refurbished-dinosaurs\run.lock on Windows or
/var/tmp/refurbished-dinosaurs/run.lock elsewhere, overridden by
REFURBISHED_DINOSAURS_RUN_LOCK. The assessment took this lock and released it
when its owned DOSBox process ended. [orphanCleanupLog.md](../orphanCleanupLog.md)
records forced cleanup. Captures, configurations and probe logs remain local.

There is no unattended gameplay-state probe or full-game RNG recorder yet.
Debugger transport and the observed RNG mapping are verified (FND-RNG-004), and
a per-run validator is available for the observed unpaged, flat-selector layout.
Rule-tagged draw capture is verified only for the bounded startup case of
EXP-RNG-001. Full-game capture and rebuild replay remain unverified.
Research must not assume arbitrary state fields or full recording capabilities.
An emulated call starts no game process and requires no run lock.

## DOSBox-X debugger installation and probe

The official portable Windows x64 release 2026.10.01 is installed locally at
`artifacts/runtime-tools/dosbox-x-2026.10.01/portable/bin/x64/Release/dosbox-x.exe`.
The archive SHA-256 is
`3c50915e63aefe35777d65ebb4391a06b4a2c2342e4c75cbee6bbda4c6916e33`.
It leaves the owned bundled DOSBox installation intact. The upstream debugger
[control protocol](https://github.com/joncampbell123/dosbox-x/blob/dosbox-x-v2026.10.01/README.debugger)
connects to a loopback listener; no MCP server is required by this Python adapter.

Run `python tools/Probe-DosBoxDebugger.py --emulator <local-exe> --output
artifacts/runtime-tools/probe-synthetic` for the synthetic test. It creates a
synthetic DOS program, takes the exclusive machine run lock, starts its own
hidden emulator, checks memory and breakpoint effects, then stops that process
and releases only its own lock. A genuine hidden native console is required;
`CREATE_NO_WINDOW`, `-noconsole` and redirected console handles failed debugger
entry. Use the non-SDL2 Release executable verified here. Numeric debugger
arguments and an explicit dump filename passed the memory comparison.

With `--original`, set GAME_DIR for that command to the owned installation
containing game.ins. The disc is read-only; C: uses the existing isolated drive
under analysis/documentation-audit/runtime. The probe verifies the original's
relocated DOS entry and a protected-mode memory read, not a gameplay state.
Its current source-identity check also requires the local extracted executable
under analysis/original/disc-root. Dumps and reports stay under ignored artifacts.

## Structured debugger build and live mapping diagnostics

The portable release does not include the structured Agent API. A local source
build uses official DOSBox-X revision
`b6abbd5980a885f5f310a4088c59a8688d1b116c` (tag `dosbox-x-v2026.10.01`),
configuration `Agent Debug SDL2`, x64. The upstream Python client from that same
checkout supplies JSON-RPC transport and models. The hash-pinned
`dinorefurb-dosbox-session` package owns checkout verification, the run lock,
private C: preparation, endpoint, readiness, operation observation and cleanup.
`tools/shared_dosbox_runtime.py` adapts its API to the game recorder; mapping
and supported input contracts remain local. Its named pipe is unique to the probe.

Clone the official repository into the ignored directory
`artifacts/runtime-tools/dosbox-x-agent-source`, selecting that tag and verifying
`git rev-parse HEAD` against the revision above. The verified local build command
uses the installed Visual Studio 2019 Build Tools and Windows SDK:

```powershell
& 'C:/Program Files (x86)/Microsoft Visual Studio/2019/BuildTools/MSBuild/Current/Bin/MSBuild.exe' `
  artifacts/runtime-tools/dosbox-x-agent-source/vs/dosbox-x.sln `
  '/p:Configuration=Agent Debug SDL2' /p:Platform=x64 `
  /p:PlatformToolset=v142 /p:WindowsTargetPlatformVersion=10.0.19041.0 /m:2 /v:minimal
```

The executable is under `bin/x64/Agent Debug SDL2/`.
For an experimental alternate build, replace the configuration with
`Agent Debug No Heavy SDL2` and select it using `--debugger-build no-heavy`.
This configuration has compiled successfully and passed a native startup probe
through the initial screen-loader entry (see docs/VALIDATION.md). It lacks CPU
tracing and memory-change breakpoints. The probe rejects
CPU tracing with this selection and records the selected executable hash in
its local debugger-build.json. The default remains the verified heavy build.
New structured sessions also save capabilities.json and session-identity.json
locally, identifying the actual feature set, source revision, owned process and
session. Use those records when diagnosing an owned run; feature availability
still requires verification against the requested operation.

Synthetic verification observed a stopped startup, a native breakpoint operation, register effects and
a memory write/read with the expected-hash precondition. The process needs the
same hidden native console as the portable debugger. Before target launch, the
wrapper observes a guest-written readiness marker after C: and D: setup; RPC
availability alone does not prove AUTOEXEC has finished. An observation timeout
preserves the pending operation, and cleanup failure retains the run lock while
the owned process is still live.

Install `requirements-evidence.txt` with `--require-hashes` in a private Python
environment. The default development interpreter may contain editable toolkit
packages and is unsuitable for release verification. Run
`artifacts/validation-python/Scripts/python.exe -B tools/test_shared_dosbox_runtime.py`
for synthetic adapter checks. Guarded input uses the released session's
`FieldContract` and `write` API; a failed write prevents further continuation.
The caller permits only supported event payloads and queue count, then checks
the complete queue relationship before continuing.

New probes default to normal core and fixed 10000 cycles. `--cpu-profile gog`
selects GOG's `auto 30%` cycle setting while retaining normal core for debugger
stepping. `--cpu-core auto` is experimental: a native comparison failed a
recording guard. Faster traversal has not yet been verified. Record the chosen
profile from probe-configuration.json rather than inferring it from defaults.
For a bounded diagnostic, use a fresh output directory and a command-scoped
GAME_DIR pointing to the owned installation:

```powershell
$env:GAME_DIR = 'C:/GOG Games/Conqueror AD1086'
python -B tools/Probe-LiveMapping.py --output artifacts/runtime-tools/live-map-new `
  --samples 8 --observation-ms 1000 --rng-break --cycles 1000
```

The probe copies the fingerprinted executable from the local extracted store,
GOB and INI into a private drive, disables movie/credits in that copy, and mounts
the owned disc read-only. It records descriptors, local memory diagnostics and
bounded startup traces. `--rng-break` installs candidate RNG/fatal/exit entry
breakpoints after a unique identity control matches. This diagnostic neither
records all draws nor proves a prescribed gameplay state. Compare its complete
local snapshot with `python -B tools/Verify-LiveSnapshot.py <physical-memory.bin>`;
residual writes require independent audit before the candidate map is trusted.
With `--observations <mapping-observations.json>`, the snapshot verifier requires
the per-run validator to pass: a unique identity control, unanimous object
relocation bases, exact code comparison apart from the observed selector word,
unpaged protected mode, one readable code descriptor, a flat writable data
descriptor and an independent RNG pointer. The live probe repeats these checks
at each instrumented entry before reading its stack or state. An unexpected code
change, ambiguous control, descriptor change or unsupported layout stops it.
`python -B tools/test_live_mapping.py` checks synthetic acceptance and rejection.
Use `--trace-loader` for an additional bounded local instruction trace at the
survey limit. The Debug configuration progresses timed startup slowly at high
cycle budgets; the 1,000-cycle diagnostic reached the seed entry described in
FND-RNG-003. This is a breakpoint transport check, not evidence of all draws.
The original content and traces remain local and must never be committed.

### Bounded startup RNG recording

EXP-RNG-001 records two fresh launches with different seeds. The probe stops
after RULE-PERSON-002's startup draws, before character selection. It verifies
each native raw result, bounded result and state transition against RULE-RNG-001;
it rejects every caller outside this prescribed case before executing its draw.
This does not verify full-game completeness or replay against the rebuild.

```powershell
$env:GAME_DIR = 'C:/GOG Games/Conqueror AD1086'
python -B tools/Probe-LiveMapping.py --output artifacts/runtime-tools/startup-new `
  --samples 8 --observation-ms 1000 --rng-break --cycles 1000 `
  --seed-check --record-startup-shifts
python -B tools/test_rng_recording.py
```

Use a fresh output directory for each launch. The address-free local journal
separates events from local register/memory diagnostics. Export a passed case
with tools/Export-RngFixture.py; this whitelist exporter emits only named rules,
bounds, results and RNG states into the experiment fixture. Keep all guest
snapshots and trace diagnostics local.

### Native entry/return recording transport

Use the same fresh-drive command with `--record-native --record-draw-limit 30`
in place of `--seed-check --record-startup-shifts`. Native entry/return
breakpoints observe the raw and bounded results without single-stepping through
interrupts. Each stop rechecks protected mode, descriptors and the relevant
original function code against the validated initial snapshot. A pending
continuation is observed again after each observation expiry until it stops;
there is no second-expiry cutoff. Every observation verifies the owned emulator
is still alive. Transport failures propagate separately from observation expiry;
the continuation is never restarted automatically.

Once candidate entry breakpoints are installed, native recording also observes
that same continuation until it stops, rather than spending the remaining
mapping-survey samples on diagnostic pauses. If the survey never reaches the
native recording entry, the command fails instead of reporting success without
a journal. Bounded mapping-only surveys still take explicit diagnostic pauses;
their startup wait catches only observation expiry, not transport timeouts.

The current accepted policies cover initial seeding, character shifts and the
home selector's directly read argument 7 (FND-STRATEGY-043). The complete
selection rule remains disputed; capture preserves its native argument without
assuming the disputed table interpretation. The
prompt reseed/remainder policy is statically grounded in FND-TALK-003 but has
not yet been verified in a live dialogue. Unaccepted callers, unexpected state
writes and reentrant RNG invocations stop capture before further work and keep
local memory/register diagnostics. The native journal marks full-game and
accepted-caller completeness false. Increasing the draw limit does not bypass
those guards. tools/rng_journal.py separately verifies reseeds, ordering and
raw/bounded state transitions; run tools/test_rng_journal.py for synthetic checks.
This remains intermediate transport verification, without actual rebuild replay.

Completed seeds and bounded results are appended through session 0.3.0's
`log_event` and synced before continuing the guest. Its `session/events.jsonl`
header records our schemas, versioned outcome contract and imported module hashes.
The terminal validator delegates envelope checks to `read_event_log`, then compares
its events with our journal and replays their numeric semantics. Historical
unwrapped `native-rng-events.jsonl` captures retain their original contract.
Existing logs are never reused. An interrupted prefix does not establish recording
completeness or authorize resuming a guest. Mapping-only probes leave an incomplete
event log with their module identity; they do not claim a recorder outcome.

For a startup state diagnostic, add `--stop-at-screen` and increase
`--record-draw-limit` above the startup prefix. The optional breakpoints stop
before either screen-loading procedure runs (FND-UI-001), recording its requested
screen, draw and mode arguments. The report remains incomplete for full-game
coverage. This entry stop does not establish loaded screen state or readiness
for input. `python -B tools/test_native_rng_recorder.py` checks the boundary and
rejects an unrecorded RNG-state change or unregistered screen number.

`--stop-after-screen` instead follows the initial loader to its return
(FND-UI-017). It checks the returned record, screen-object identifier
(FND-UI-002), history head and recorded RNG state, rejecting recursive loading
or an invalid pointer. A quiet native startup-click run verified this return
and its screen identity/history guards (see VALIDATION.md). It does not
establish physical input delivery or full-game completeness.

Add `--continue-after-screen` to retain that initial loaded-screen guard and
then continue RNG recording. After a valid return, the recorder writes local
`screen-load-checkpoint.json`, removes the one-time screen, preparation and
extraction breakpoints, and resumes with its seed/draw policies still active.
Unknown callers still stop before their state write. The final journal stays
incomplete unless the bounded draw limit is reached; that limit also makes no
full-game completeness claim. This continuation has synthetic validation;
Traversal beyond game options still requires original verification.

With continuation, `--title-click` queues one supported-state primary click
at (10, 10) only after the verified initial screen identifier is zero.
SCR-UI-001 supports that full-screen title region. The same queue timing,
hash and readback guards apply, and RNG state must remain unchanged.
The controller records this input locally before resuming. This option has
synthetic checks. A deliberately paused native continuation snapshot showed
game-options identity/history after that input (VALIDATION.md). A subsequent
native checkpoint run verified both initial and game-options return boundaries.

`--screen-checkpoints` requires `--continue-after-screen`. It retains separate
entry/return guards for initial loading and replacement (FND-UI-017/020), checks
the corresponding saved frame, current object identifier, history head and
recorded RNG state, and appends local `screen-load-checkpoints.json` observations.
Nested loading is rejected. The title input is queued only once, after the
initial verified return; subsequent screen observations do not repeat it.
Add `--stop-after-screen-id 1` to end on the verified game-options return.
This target stop remains incomplete for full-game coverage. These guards have
synthetic validation and native initial/title-to-options return verification.
The guarded New Game transition to character options also passed a native check;
later transitions still require their own original checks.

`--new-game-click` requires screen checkpoints. After the first verified
game-options return it queues a primary click at (100, 350), inside region 3
of SCR-UI-002, once per run. FND-UI-012 supports that region's new-game route.
The same empty-queue, supported-field, timing, expected-hash, readback and
unchanged-RNG guards apply. The recorder does not bypass unsuitable timing
or seed/draw ownership checks. With `--stop-after-screen-id 2`, the next probe
can stop on verified character-options state or reject an unowned intervening
draw. This input stage has synthetic checks and a passing native target-return
check recorded in VALIDATION.md. It does not establish later gameplay coverage.

`--generation-click` requires `--new-game-click`. After verified character
options it queues one primary click at (200, 250), inside region 0 of
SCR-UI-003, to open youth generation. It uses the same queue and RNG guards.
With screen target 3, a probe verifies the generation return while recording
its initial dilemma draw under RULE-PERSON-003. Synthetic input checks passed;
native generation traversal passed the target-return check in VALIDATION.md.
The first answer and Continue have native checks below; Reroll remains unverified.

`--youth-answer` requires generation input and continuation beyond screen 3.
It queues the first answer at (100, 350), inside SCR-UI-004's first-answer
rectangle. FND-PERSON-005 and FND-UI-023 support its callback entry and return. The recorder
checks the callback's matching stack frame, unchanged code, complete journal
and numeric RNG replay before ending with `youth-answer-return-reached`.
It never treats that boundary as full-game completion or queues Continue yet.
Synthetic sequence checks and the native first-answer return check passed;
the dated result is in VALIDATION.md. This does not verify later answers.

`--youth-continue` requires `--youth-answer`. After the matching answer return
it adds a temporary breakpoint at the pointer classifier entry of
FND-BATTLE-023. It observes the same screen record and youth history, and waits
for an empty queue and an unsigned release gap sufficient for a short click.
It writes no timer or release state. Once ready, it queues (500, 150) inside
the Continue rectangle of SCR-UI-004/FND-UI-021 and removes that temporary
breakpoint. FND-UI-022 supports its entry and two restored-stack returns.
The matching return and journal replay end this diagnostic with
`youth-continue-return-reached`. A quiet native run passed this boundary and
durable-event/numeric-replay verification (VALIDATION.md); it verifies one
Continue traversal, not the complete youth sequence.

`--youth-cycles N` prescribes answer/Continue pairs. Current fresh-game traversal
uses `--youth-age-checkpoints --youth-cycles 5`: RULE-PERSON-004 and
RULE-PERSON-007 support the observed initial AGE 13 and terminal AGE 18.
The recorder checks the initial youth-screen AGE, each answer's entry and
return, and each Continue's entry and return. An answer must add exactly one
year and Continue must preserve it. A changed or missing boundary fails.
After each nonterminal Continue return, the next answer waits at the supported
pointer-readiness boundary. The fifth Continue must return after verified
screen 6 replacement; a multi-cycle diagnostic ends as
youth-sequence-return-reached. Full-game completion remains false.

`--dubbing-entry-input` supplies the guarded entry waits with animations disabled.
`--dubbing-click` requires the prescribed terminal cycle count and no earlier
screen target. After the terminal Continue return it waits at the supported
pointer-classifier boundary for an empty queue and suitable observed timing,
then checks record, object and history before queuing the full-screen click.
FND-UI-024 locates its callback and restored-stack return. A matching callback
return after verified village loading and numeric RNG replay ends as
dubbing-return-reached. Unknown RNG callers remain rejected.

AGE-checked captures use conquer-native-rng-journal-v4 and outcome contract 2.
The shared terminal outcome binds the ordered AGE observations by SHA-256;
the verifier checks the observations as well as log/journal agreement and
numeric replay. Older v1/v2/v3 captures retain their six-cycle endpoint meaning;
they are not reinterpreted as v4. Omitting AGE checkpoints selects the historical
diagnostic contract, not the current prescribed fresh-game traversal.
Five-cycle transport has synthetic validation; a fresh complete native
five-cycle and dubbing-to-village control is still required.

The native inclusive-draw policies also accept the three fixed-bound dilemma
selection calls of FND-PERSON-013. Initial selection and reroll carry
RULE-PERSON-003, and continued selection carries RULE-PERSON-004. Each requires
the exact return site and argument 4 and verifies both raw state/result and
the inclusive reduction. Other callers or changed arguments remain rejected.
Synthetic policy checks passed; native draws at these calls are recorded in
EXP-UI-001 and EXP-PERSON-002. Full-game coverage remains separate.

### Operating and interpreting a probe

Both DOSBox probe commands mute host audio by default with `MIXER MASTER 0:0`
and `[midi] mididevice=none`. Emulated sound devices remain available, so
muting does not require changing the original's sound settings or driver paths.
Use `--sound-investigation` only when the probe specifically investigates sound;
its generated configuration then permits host audio. This is the owner's
standing preference, recorded in AGENTS.md.

`Probe-LiveMapping.py --animations-off` explicitly sets the supported
`ANIMATIONS` switch to `OFF` in the private INI (FMT-CONFIG-001). Use it for
a controlled run that does not investigate animated transitions. The default
preserves the installed animation setting. The local `probe-configuration.json`
records the selected animation and audio options; it is not RNG trace evidence.

With `--stop-after-screen`, add `--startup-checkpoints` to observe the
preparation entry, animation test and shared epilogue of FND-UI-018, plus
the intervening service return boundaries of FND-UI-025. Each return checks
its own stack displacement before deferred argument cleanup.
Each checkpoint verifies the original code, descriptors, stack position and
unchanged recorded RNG state, records the guest's animation flag in local
`startup-checkpoints.json`, then continues the same run. These checkpoints
write no guest state and do not count as loaded-screen or full-game evidence.
The same option observes archive-to-loose-file extraction entry and return
from FND-SAVE-003, checking the saved frame and recorded RNG state. It records
an ordinal and returned length locally, without reading or publishing the
extracted content. An outstanding extraction is marked explicitly in an
incomplete journal. This progress observation is separate from RNG events.

The native recorder also accepts the exact hit-check scaled draw of
RULE-ASSAULT-023 (FND-ASSAULT-031) and the ten-busy-voice remainder draw of
RULE-SOUND-002 (FND-SOUND-008). It checks the scaled helper's caller and bound,
or the voice divisor, before allowing the draw, then verifies the raw state
and the appropriate reduced-result register. Other scaled callers remain
rejected. Synthetic verification of these policies does not establish that
the corresponding original gameplay paths have been exercised.

`--startup-click` requires `--startup-checkpoints --stop-after-screen`.
At the exact preparation wait of FND-UI-019, the recorder verifies its caller,
arguments, stack, original code and recorded RNG state. It then queues a
supported-state primary press/release at (0, 0), using the observed pointer
clock and the timing checks of RULE-BATTLE-012. Only the documented event
fields and count change; both unknown record tails remain untouched. Writes
use expected hashes, publish the count last and require matching readback.
The one-shot wait breakpoint is removed after that input. The local
`supported-input.json` records the prescribed click without original addresses.
This is debugger-controlled queue input, not verification of a physical mouse
or Windows input delivery. A native run verified the queued click, preparation
checkpoints and guarded screen return without an unrecorded RNG-state change.
Repeat the controlled startup probe with a fresh output directory:

```powershell
$env:GAME_DIR = 'C:/GOG Games/Conqueror AD1086'
python -u -B tools/Probe-LiveMapping.py --output artifacts/runtime-tools/startup-click-fresh --samples 8 --observation-ms 1000 --rng-break --cycles 1000 --record-native --record-draw-limit 1000 --stop-after-screen --debugger-build no-heavy --animations-off --startup-checkpoints --startup-click
```

`--dubbing-entry-input` requires native recording, animations disabled and
the prescribed youth traversal. At FND-UI-026's two entry input waits it
verifies the active screen request, saved stack depth, original code, data
selectors and recorded RNG state. It observes pointer readiness and queues
one supported short click for each wait. Accepted input must return one and
drain the queue before the next phase. The final entry return restores its
entry stack. Schema v3 retains pending entry state and explicit completion;
the screen-return guard refuses a return that skipped these observations.
This control is separate from the later `--dubbing-click` callback input.
Its synthetic checks pass; native traversal remains unfinished.

`--youth-age-checkpoints` adds read-only observations at the youth screen
return and each prescribed answer/Continue entry and return. It follows
FND-PERSON-001 and FND-PERSON-003's row-zero AGE field with bounded pointers
and table counts. The local report records the boundary, completed cycle
count, AGE and RNG state. It does not alter AGE, timers or RNG and does not
assume a cycle count from the observation. Use it to investigate a traversal
that reaches its endpoint earlier than the prescribed count.

The probe imports its recorder and input helper before starting the guest and
passes the imported module names to the session, which hashes them into its event
log header before starting the emulator. Historical runs use `probe-source.json`. Later source
edits do not change those loaded modules. A traceback may display lines from an
edited file, so use the run's hashes and its recorded state when diagnosing it.

Do not replace this with `nosound=true` in the structured build: native and
synthetic readiness attempts with that setting failed before the guest marker.
The portable debugger's synthetic check passed with it, so that result alone
did not establish compatibility with the structured build.

Before executable analysis, run `tools/Verify-Configuration.ps1
-RequireAnalysisReady`. Use a fresh local-only output directory, a
command-scoped GAME_DIR and the machine lock acquired by the wrapper. Keep
the original disc read-only and change only the private drive. Check the
selected build's executable hash and actual capabilities; a configuration
header alone does not prove a runtime feature is enabled.

Keep the process/session identity and the pending operation together. Read
session-identity.json rather than guessing a session identifier. A second
diagnostic client must supply unique RPC request IDs on every call: the pinned
Python client restarts its default counter for each instance, and reused IDs
can conflict with requests from the recorder. An explicit UUID-based namespace
avoids that collision. Poll the
same owning command and debugger operation while it is live. An operation
identifier or CPU-time measurement does not reveal how many draws completed;
read the completed-event log or final journal for that. Observation expiry
alone is no reason to restart, add another continuation or release the lock.

A manual diagnostic pause is an interruption, not an expected recorder
boundary. The recorder rejects such a stop and attempts local register and
memory diagnostics; do not present its surviving prefix as a finished run.
If a deliberate pause is needed, verify the owned session and process first.
After terminal completion, check both the journal's status/completeness fields
and cleanup: the owned process must have stopped before its lock is released.
Never use that check to stop another task's emulator or remove its lock.

For the current AGE-checked traversal, use a fresh output directory and the
private hash-locked Python environment. Keep host sound disabled:

```powershell
$env:GAME_DIR = 'C:/GOG Games/Conqueror AD1086'
& artifacts/validation-python-latest/Scripts/python.exe -B tools/Probe-LiveMapping.py `
  --output artifacts/runtime-tools/dubbing-fresh --samples 8 --observation-ms 1000 `
  --rng-break --cycles 10000 --debugger-build no-heavy --record-native `
  --record-draw-limit 1000 --animations-off --startup-checkpoints --startup-click `
  --stop-after-screen --continue-after-screen --title-click --screen-checkpoints `
  --new-game-click --generation-click --youth-answer --youth-continue `
  --youth-cycles 5 --youth-age-checkpoints --dubbing-entry-input --dubbing-click
```

For a current native recorder's terminal diagnostic, verify durable-log agreement,
event order and numeric RNG replay with an explicit expected boundary:

```powershell
python -B tools/verify_native_recording.py artifacts/runtime-tools/continue-fresh --expect-status youth-continue-return-reached
python -B tools/verify_native_recording.py artifacts/runtime-tools/five-cycles-fresh --expect-status youth-sequence-return-reached --expect-youth-cycles 5
python -B tools/verify_native_recording.py artifacts/runtime-tools/dubbing-fresh --expect-status dubbing-return-reached --expect-youth-cycles 5
python -B tools/test_verify_native_recording.py
```

The validator rejects a different or incomplete boundary, recorded failures,
pending or missing operation flags, changed event contents/order and an incorrect
final RNG state. Older journals missing flags added after their run are rejected;
do not amend a capture to satisfy the check. File reads are bounded. This check
does not establish process cleanup, original trace completeness, input reachability
or replay against the rebuild; check those separately. Diagnostic journals must
keep full-game and accepted-caller completeness false.

Use `--expect-youth-cycles N` to additionally require the exact completed-cycle
count and endpoint screen identity/history head. For one through five cycles the
endpoint remains youth; six requires dubbing. A cycle-count expectation of one
requires the Continue-return status; larger counts require the sequence-return
status. This checks recorded boundary data, not the screen's pixels or all youth
outcomes. Without that option, the command checks terminal status and event
consistency only and makes no cycle-count or endpoint claim.

Tool edits do not update Python modules already loaded by a running probe.
Finish observing that run under its original code, and test the new version
in a fresh launch. Keep the loaded-screen return check distinct from the
screen-entry check, and both distinct from verified input readiness. The RNG
formula replay is a transport consistency check; actual rebuild replay and
complete caller ownership remain separate obligations.

After full verification, propose reusable lifecycle, locking, operation-wait and
debugger helpers upstream, together with a reproducible guide. Keep game-specific
LE identity, field mapping and rule ownership here; check existing upstream
issues and helpers before proposing extraction.

## Unicorn function harness

Install the hashed dependency with `python -m pip install --require-hashes -r
tools/emu/requirements.txt` after the main evidence dependencies. The harness
uses Unicorn 2.1.4 and the existing pinned Capstone decoder. It supports only
the fingerprinted BLD-GOG-EN bound LE image, its verified object/page layout and
internal 32-bit offset relocation forms. Unsupported formats, page flags,
relocations, interrupts and hardware/service instructions stop with errors.
There are no import, interrupt, port, timer or video models. Loaded state is
fresh for every call, code is protected, and an instruction budget bounds it.

Run `python tools/emu/test_harness.py` without original files. To smoke-check
RULE-RNG-001's functions, set GAME_DIR for that command to the local original
store containing disc-root/CONQUER.EXE and run `python tools/emu/verify_rng.py
--output artifacts/runtime-tools/unicorn-rng`. Missing GAME_DIR skips. Named
parameters/results are separate from local instruction traces. These smoke
comparisons do not establish branch coverage, complete callers/inputs, or raise
any spec or parity status. A research batch must still record its experiment
and evidence before those results can validate a parity row.
