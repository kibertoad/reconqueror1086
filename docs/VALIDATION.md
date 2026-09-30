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
pinned Capstone dependency on `PATH`. Install the Python dependency with
`python -m pip install -r tools/evidence/x86-reporter/requirements.txt`. CI runs
the same gate in `.github/workflows/ci.yml` on every push to `main` and every pull request.

The default fast gate excludes `Category=LongRunning`. Use `-TestFilter` to narrow
the suite or `-IncludeLongRunningTests` when that coverage is needed. Override
`-MinimumExpectedTests` for a deliberately narrowed run; the default discovery floor
is 98. The gate always runs the executable specification suite.

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
[checks](upstream/documentation-standard.md#checks) (lines 782-833), compiles each `.ksy` file with
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