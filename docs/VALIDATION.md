# Validation

This page describes the checks a change has to pass before it is committed, locally and in CI.

## Local gate

`tools/Invoke-Validation.ps1` is the canonical local gate. `Run Tests.bat` discovers
the SDK and delegates to it. The gate takes an exclusive checkout-specific lock
and stops at the first failure:

1. `tools/Verify-Repository.ps1`, the repository policy described below.
2. Configuration, infrastructure, pinned-documentation and narrative-reference checks.
3. Synthetic Node and Python infrastructure tests and section-link checks.
4. A build of `Conqueror1086.slnx` into `artifacts/validation`, then the xUnit suite.
5. The executable specifications from that isolated build. These are the
   rebuild's own behaviour checks, written as plain assertions in `Program.cs`. They have nothing
   to do with the documentation standard's `spec/` directory.

The builds use two MSBuild workers by default, keep node reuse enabled, and disable
the shared compiler. The gate does not stop games or unrelated services. It needs no graphics device and no original
game files: the tests build their inputs from synthetic data, except the tests described under
[Tests against the original](#tests-against-the-original), which skip without them.

The gate needs the .NET 10 SDK, Node.js 22 or newer, Python 3.12 or newer and the
locked toolkit packages. Run `npm ci --ignore-scripts` and
`python -m pip install --require-hashes -r requirements-evidence.txt` before validation. CI runs
the same gate in `.github/workflows/ci.yml` on every push to `main` and every pull request.

The default fast gate excludes `Category=LongRunning`. Use `-TestFilter` to narrow
the suite or `-IncludeLongRunningTests` when that coverage is needed. Override
`-MinimumExpectedTests` for a deliberately narrowed run; the default discovery floor
is 98. The gate always runs the executable specification suite.

After a normal run has restored packages, use `-NoRestore` to reuse that restore
state. The build receives `--no-restore`; all checks and test selection still run.
Missing restore state fails through .NET diagnostics without a restore fallback.
Normal runs and CI retain automatic restore.

Configuration checks distinguish build identity from analysis readiness. Builds
can proceed while latest-patch provenance is unknown. Before executable analysis,
`tools/Verify-Configuration.ps1 -RequireAnalysisReady` must pass. See
[SOURCE-EDITIONS.md](SOURCE-EDITIONS.md).

## Tests against the original

A test listed in a parity row compares the rebuild with evidence from the original. Most such
tests replay a committed fixture and run in CI like any other test. A test whose evidence cannot
be committed, such as a format test that decodes every shipped file or a screen test compared with
a capture, reads the original's files from the directory named by the `GAME_DIR` environment
variable: one directory per build, named by its build ID (`GAME_DIR/BLD-GOG-EN/...`), and
`GAME_DIR/captures/` for captures and saves named by their hash. Such a test skips when the file is
absent, and its file carries the comment `// needs: GAME_DIR`. The documentation check fails a
listed test file that mentions `GAME_DIR` without the comment. No current test reads the original.

CI never has the original's files, so the marked tests skip there. They run on a maintainer's
machine with `GAME_DIR` set to the owned copy. After a run of `Run Tests.bat` in which every test in
every marked test file of a `validated` row passed and none was skipped, record the run and commit
the `VALIDATION.md` it writes at the repository root:

```powershell
$env:GAME_DIR = 'D:\conqueror-evidence'
& '.\Run Tests.bat'
./tools/Check-Documentation.ps1 -RecordValidation BLD-GOG-EN
git add VALIDATION.md
```

`VALIDATION.md` holds the commit, the date, the builds and the hash of each marked test file of a
`validated` row. The check, in CI as well, fails a `validated` row whose marked test file is missing
from the record or has changed since it was recorded, so a change to such a test needs a new local
run before it merges. A change to code a marked test exercises needs one too, which the check
cannot see.

## Repository policy

`tools/Verify-Repository.ps1` applies `tools/repository-policy.json` to tracked files. It rejects
tracked files under `UserContent/` and `analysis/original/`, original-media extensions outside the
approved clean-room and synthetic fixture roots, and tracked files larger than 1 MiB. No original
game bytes, extracted resources or generated analysis reports are committed; `analysis/` holds
local research material only.

## Spec checks

The `Documentation standard` job in `.github/workflows/ci.yml` runs the `check-documentation`
action from
[refurbished-dinosaurs-toolkit](https://github.com/kibertoad/refurbished-dinosaurs-toolkit),
pinned to a full commit SHA, on every push to `main` and every pull request. It checks `spec/`,
`parity/` and `deviations/` against the standard's list of
[checks](upstream/documentation-standard.md#checks) (lines 787-838), compiles each `.ksy` file with
the Kaitai Struct compiler, checks that every spec and deviation ID cited in `src/`, `tests/`, `tools/`
exists and is not superseded, fails when `spec/index/` or `PARITY.md` is stale, and
fails a `validated` row whose marked tests are not in `VALIDATION.md` as they are now. It
fetches the full history so it can fail a pull request that deletes a spec ID, area or deviation
that exists on `main`. The toolkit's
[setup guide](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/main/docs/documentation-standard-check.md)
lists its inputs.

`tools/Check-Documentation.ps1` runs the same checker locally from `vendor/`, after
`tools/upstream.mjs` verifies its digest and agreement with the CI action pin.
It needs no network download. `tools/Check-NarrativeReferences.mjs` separately checks
project narrative docs locally and in CI, excluding the immutable upstream text
and generic workflow examples, whose IDs are illustrative. The check writes `spec/index/`
and `PARITY.md`, and nobody edits them by hand. After changing the spec, `parity/` or
`deviations/`, regenerate them and commit what the check writes:

```powershell
./tools/Check-Documentation.ps1 -Write
git add spec/index PARITY.md
```

The Kaitai Struct compiler is found through the `KSC` environment variable or
`kaitai-struct-compiler` on `PATH`; without it the local run skips compiling the definitions and
prints a warning. The script does not check some items on the standard's list, such as the
fixture schema and the hashes of saves and recordings; its guide lists them, and reviewers check
those by hand. A save-patch write is given as a byte offset and value in the experiment's Setup
section.

## Migration verification

On 2026-09-30 the canonical fast gate passed, including the xUnit suite,
executable specifications, Node infrastructure tests and Python evidence tests.
The solution built with zero warnings and zero errors. No original files were read.
The local documentation check and Kaitai compilation passed using the toolkit's
checksum-pinned compiler. CI and release workflows passed actionlint, and the
Windows portable package and Inno Setup installer passed their local checks.
Linux/macOS installer execution and remote signing were not exercised locally.

## Documentation audit verification

The 2026-09-30 canonical fast gate passed after the documentation audit: source
readiness, pinned bytes, spec/index checks with Kaitai, narrative references,
area queues, committed LE inventory identity, synthetic reporter and capture
regressions, solution build, xUnit and executable specifications. See
[documentation-audit.md](documentation-audit.md) for the requirement-by-requirement
record and upstream fixes. The canonical tests used synthetic inputs and read no
original files. Separately, the source inspector and fingerprint-guarded LE mapping
read the owned executable for the coverage inventory (BLD-GOG-EN, FND-RES-009).
Those generated analysis outputs remain ignored; only compact inventory metadata
and independently authored findings are committed. The bounded runtime assessment
and its limits are recorded in [RUNTIME.md](RUNTIME.md).

`Check-Documentation.ps1` also runs `Check-ResearchTracking.mjs` and the
Conqueror-specific `Check-Coverage.mjs`. These enforce structural consistency;
they do not prove complete research or promote a spec/parity status. The changed
CI workflow passes actionlint. Packaging and platform installers were not
rerun for this research-only audit; their prior migration results remain above.
## Current capture contract

The selected client is rendered with PrintWindow full-content rendering; no
desktop fallback is permitted. Capture workers have a configurable deadline
and are stopped on timeout. Physical client pixels are measured in a temporary
per-monitor DPI context, restored afterwards. Schema-two metadata records
client size rather than desktop geometry. A failed frame rejects and removes
the entire checkpoint. Synthetic worker tests cover a stalled UI thread,
recovery, and right/bottom edge retention with a DPI-unaware caller. Higher-DPI
and mixed-monitor behavior remains unconfirmed locally.

## Upstream migration verification (2026-10-01)

The canonical fast gate passed after full template `7b3bbe46`, website
`82deb767`, checker `f5e62e08` and reporter `7da1b93` adoption. It compiled the
Kaitai definitions, checked queues, inventory metadata and exact upstream bytes,
ran the Node and Python suites (including worker timeout/DPI capture acceptance),
built the solution with zero warnings/errors, and passed xUnit and executable
specifications. The Kaitai archive matched the toolkit SHA-256 before use.
The gate used portable PowerShell 7 and synthetic data; no original game ran.

`node tools/upstream.mjs check-upstream` confirmed that all pinned snapshot
contents match current main, including byte-identical checker content on toolkit
main. Workflow actionlint and `git diff --check` passed. Project configuration,
installer GUID and signing wiring passed the canonical infrastructure check.
Packaging was not rerun because this adoption changes research tooling/guidance
and the documentation CI pin, with no packaging or gameplay changes.

## Full migration acceptance audit (2026-10-01)

[CI run 36799199015](https://github.com/kibertoad/reconqueror1086/actions/runs/36799199015)
completed successfully for migration commit `f2ae8779f269235f4ffc8f735e2c7215774195d5`.
Every job passed: documentation, Windows x64, Linux x64, macOS arm64/x64,
Windows installer, Linux installer and macOS arm64/x64 installers. Platform
jobs ran the canonical gate, assetless publish, media-boundary check and published
entry-point smoke check. Windows also built the portable package and installer,
installed it, checked shortcut/native-library layout, ran the native graphics
diagnostic with explicitly provisioned software OpenGL, and uninstalled it.
Linux checked Debian metadata, extracted layout and executable startup; macOS
built both packages and checked their expanded executable payloads.

The completeness audit compared every path in the template delta from e0325e0
to 7b3bbe46 and checked all shared Ghidra/evidence modules. Changed capabilities
are adopted; game-specific plan, validation, identities, LE adapter and populated
queues are retained deliberately. The only shared adoption-tool difference is
reporter file-list ordering; the same complete set is hash-verified. The section
link tool retains command-scoped Git trust for this checkout. Original spec,
parity, deviations and gameplay were unchanged by the migration.

Strict analysis readiness, pinned bytes/reporter set, documentation/Kaitai,
queue/coverage metadata, section links and workflow lint pass on the committed
migration. No migration acceptance item remains open. Long-running gameplay
research, high-DPI/mixed-monitor capture characterization and optional release
signing are outside this migration; no original-content import or signed release
was performed. Local packaging was not repeated because the committed migration
passed the platform-specific CI packaging acceptance above.

The check also fails when a code comment gives an address that no entry the
comment cites records, in its locations or text or in the evidence of an entry
it cites. A neutral name (`fn_â€¦`, `g_â€¦`) is always an address. A plain `0xâ€¦`
value is one only inside an image the job gives with the action's `images`
input, so colours, masks and offsets are left alone; the template cannot know
the original's image, so `ci.yml` only explains how to add it. Take the base
and size from the finding that records them. A range larger than `max-range`
(64 KiB by default), such as a whole section, records only its two ends, nothing
inside it. When a
comment fails, cite the finding that records the address, or write one.


The pre-commit hook runs `tools/Invoke-NodeChecks.mjs --no-ksy` on the staged tree.
Enable it with `git config core.hooksPath .githooks`. Local documentation checks
read the checker inputs from the CI step; command-line options override them.

## Latest upstream refresh verification (2026-10-01)

`tools/Invoke-Validation.ps1` passed after adopting template 8d0eef35,
rules ca39d075 and toolkit f8c51bfc. Build: no warnings or errors; .NET fast
tests: 565 passed; executable specifications: 146 passed; Node synthetic
tests: 67 passed; Python synthetic tests: 144 passed. Snapshot and reporter
digests, section links, documentation, queue tracking, LE inventory metadata,
repository policy and configuration checks passed. Kaitai compilation was
skipped because no local compiler was available; CI retains that compilation.
Long-running tests and installer packaging were not run for this tooling update.
The validation runner supplies the vendored Python import path while preserving
upstream test bytes and restores the previous environment afterward.

## Current-template NoRestore verification (2026-10-02)

The pre-migration canonical fast gate passed with its normal restoring build.
After adding the template main 79d18a20 NoRestore capability, the complete
`tools/Invoke-Validation.ps1 -NoRestore` gate passed with the existing restore
state. Command-double regressions passed for default build restore, explicit
`--no-restore` consumers, identical policy/tool checks and test filters, and a
failed build without a restore fallback. The .NET test run passed 565 tests;
the executable specifications passed 146 assertions. Existing Python and Node
infrastructure suites passed, together with the two new gate regressions.
Local Kaitai compilation remains skipped because its compiler is unavailable.
No original assets, gameplay changes or evidence-status promotions are involved.
This verifies the NoRestore batch, not the unfinished package migration.

## Toolkit package adoption verification (2026-10-02)

The complete canonical fast gate passed with NoRestore after migrating the
checker and reporters to reader 0.2.0, checker 0.1.0 and engine 0.4.0. Project
configuration preserves rule snapshot bytes and npm/Python locks. Regressions
exercise wrapper forwarding, synthetic return behavior, source hash rejection,
manifest/integrity/installed-version/action drift, CI checker argument forwarding
and the packaged Ghidra memory-block helper. The synthetic before/after report
retains every prior field and value; the new engine adds caller/site and
return-flow observations. No original source is used.

Rules freshness verified ca39d075 against current website main with unchanged
bytes. The Python requirements use release-distribution hashes for the engine
and Capstone, and the npm lock records package distribution integrity. Toolkit
copy-only tests are removed because the package publisher runs them before release;
project integration tests remain in the canonical gate. Local Kaitai compilation
is skipped because the compiler is unavailable. The shared .NET resource migration
and final template/package acceptance audit are still outstanding.

## Shared .NET resource migration verification (2026-10-02)

The canonical fast gate passed normally after restoring ScientificMethod.LegacyFormats
0.2.0 and its ScientificMethod.Core 0.2.0 dependency, then passed with NoRestore
after adopting shared optical APIs. The .NET suite passed 565 tests and the
executable specifications passed 146 assertions. Existing synthetic PCX, palette,
Smacker header/packet/audio/video/stream, cue sheet, raw ISO, CDDA and transactional
import checks now exercise the packaged readers. No proprietary fixtures were used.
Core has no new package dependency; gameplay and evidence statuses are unchanged.

Workflow lint passed with actionlint 1.7.12. The Windows portable publish passed
both assetless and native-platform smoke tests and contained both toolkit DLLs.
Inno Setup 7.1.0 compiled the Windows installer. The publish scripts additionally
carry the toolkit MIT notice at the path the package NOTICE names. Linux/macOS
execution and Kaitai compilation remain CI acceptance checks.

The final Windows package/installer rerun passed with the toolkit MIT notice
included; its bytes match docs/licenses/scientific-method-MIT.txt. The existing
local Kaitai Struct 0.11 compiler was then selected through KSC and the complete
documentation check passed with format compilation enabled. Strict analysis
configuration also passed. These close the local compiler and licensing checks;
cross-platform execution remains the acceptance evidence from CI on main.
