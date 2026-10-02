# Template and toolkit migration

Status: implemented and locally validated after owner approval on 2026-09-30.
The owner also requested removal of
mandatory explicit implementation-plan approval gates from this repository and
the template. Plans remain required documentation; authorized work proceeds
without a second approval step.

## Sources and outcome

Target: `C:\sources\reconqueror` only.

- Template: `C:\sources\refurbished-dinosaurs-template`, main commit
  `3e8805ea474c60e7c3234213a108cb85a9e86265`.
- Toolkit: `C:\sources\refurbished-dinosaurs-toolkit`, main commit
  `c2b21ee62fc404391e8dcfafd7029185f81241a9`.
- Both revisions were checked against their configured GitHub remotes on
  2026-09-30. Both local checkouts have been updated to main.

The outcome is current reusable infrastructure adapted to this existing game,
with reproducible offline documentation checks and the existing gameplay intact.

## Migration scope

1. Add explicit project identity/configuration, preserving existing package IDs,
   application data directories, executable names, source environment variables,
   installer GUID and supported editions. Adapt configuration/bootstrap checks
   for an existing project rather than running a destructive template rename.
2. Adopt the template's pinned Standard v1, Methodology and work protocol,
   snapshot integrity checks, section-link checks and upstream licenses. Vendor
   the checker from the selected toolkit revision and match its CI action pin.
   Preserve `tools/Check-Documentation.ps1` as the compatible local entry point,
   with its existing write and local-validation-record options.
3. Adopt bounded evidence tooling, Ghidra helpers, the toolkit's pinned x86
   reporter and their synthetic tests and licenses. Generated reports remain
   local-only. This migration performs no original executable analysis.
4. Introduce `tools/Invoke-Validation.ps1` with checkout-specific locking, fast
   test filtering and minimum discovery counts. Keep `Run Tests.bat` as a wrapper
   and retain both the xUnit suite and executable specification suite. Preserve
   reusable MSBuild nodes and ownership-aware process cleanup/logging.
5. Adapt agent/workflow guidance and research tracking to the new protocol,
   preserving this repository's canonical push destination and orphan audit
   rules. Do not silently promote or demote existing evidence/parity statuses.
6. Adopt stronger repository checks, exact-byte attributes for pinned files,
   generated-file exclusions and NuGet advisory handling. Preserve the existing
   restricted resource extensions and ignore original content and analysis.
7. Adapt current installer validation and optional signing support to existing
   release/package identities. Preserve all four platform outputs and the
   ownership-aware installer. Signing stays optional; no credentials, signing,
   publication or release execution occurs in this migration.

## Intentionally retained

- `Conqueror.Core`, `Conqueror.Resources`, `Conqueror.Game`, the separately
  runnable `Conqueror.Import`, and `Conqueror.Inspect`: their purposes already
  match the template boundaries. Preserve these names and caller paths.
- Original source manifests, hashes, resource formats, bounded parsers,
  transactional importer and runtime pack validation. Generic sample manifests
  or extractors cannot replace game-specific behavior without equivalent tests.
- Game-specific packaging, installer source discovery and runtime paths.
- Existing spec entries, deviations, parity claims and gameplay rules.

## Evidence and open questions

The existing plan, architecture, validation guide, CI, importer and packaging
were inspected. The pre-migration baseline had no `tools/project-config.json`
and pinned the toolkit checker to `6e3cad31b6d61280a4649a873cf890377b402a75`.

At the initial migration baseline, BLD-GOG-EN established build identity but
latest-official-patch provenance was not yet established. That prerequisite
was subsequently closed by SRC-PATCH-CATALOG and SOURCE-EDITIONS; the current
`Verify-Configuration.ps1 -RequireAnalysisReady` passes. No migration blocker
remains from that historical prerequisite.

## Acceptance and synthetic tests

- Snapshot/reporter digests and CI pins agree; offline checks reject altered
  bytes, unsafe paths and inconsistent revisions.
- Relevant upstream, evidence and configuration tests pass over synthetic
  inputs. No test requires proprietary content in CI.
- Canonical fast validation builds the solution, runs xUnit and executable
  specifications, and preserves expected test discovery. Run relevant installer
  configuration checks; report platforms unavailable on this Windows machine.
- Run `tools/Check-Documentation.ps1 -Write` for any spec, parity or deviation
  changes, then check generated indexes. Existing new-checker failures are
  resolved with honest documentation or reported explicitly, never waived.
- Missing/foreign asset packs still fail clearly, package outputs remain
  assetless, and existing package/installer identities stay compatible.
- Review the final diff for template placeholders, proprietary/generated files,
  accidental rule changes and unrelated changes.

## Risks and delivery

The current toolkit may expose pre-existing documentation failures. The work
protocol may need research-queue and handover adaptation. Exact-byte snapshots
require attributes that survive Windows line-ending conversion. Existing tests
use xUnit's executable entry point, so the generic template runner must be
adapted rather than copied verbatim.

Prepare reviewable changes and record observed validation results. The sync
skill calls for reviewable local commits and post-commit process audits. Do not
push or open a PR without an explicit request. Preserve unrelated local files
and background processes.

## Result

The reusable infrastructure is migrated, with game-specific parsers, importer,
package identities and gameplay retained. The canonical gate, offline snapshot
and narrative checks, Kaitai compilation, workflow lint, synthetic GUI exit-code
regression and Windows package/installer checks pass. The platform diagnostic now
initializes graphics without original assets, and package checks wait for actual
GUI results. Software rendering is an explicit diagnostic mode with an external
driver; it cannot be enabled for gameplay or bundled into a package.

Project configuration preserves the existing solution/importer paths through
reconfiguration. The global sync skill and repository/template guidance no longer
require a second explicit plan approval. Both repositories now keep changing
inventory totals out of narrative prose.

At the initial migration checkpoint, latest official patch provenance was
unestablished and blocked original
executable analysis, not this independent infrastructure migration. Linux/macOS
installer execution and remote signing are left to their platform CI workflows.
No original executable analysis, release, push or proprietary-content import was
performed during migration.

## Latest inventory contract

Adopt template main `e0325e0b063735e94b7e3ac94b0b8b89d0a38a79`:
committed inventory verification now rejects identity, mapping, destination and
metadata mismatches, with explicit evidence required for a retained legacy path.
The shared inventory/report modules and synthetic evidence regressions are
adopted together. Reconqueror retains its LE mapping adapter because the generic
source loader supports MZ/FBOV, not its DOS/16M-bound LE image. Acceptance is the
shared evidence suite and the local committed-inventory check; no gameplay
behavior or evidence status changes through this tooling adoption.

## Documentation audit closure

SRC-PATCH-CATALOG now establishes public official-version provenance for the
owned build; SOURCE-EDITIONS and the strict analysis configuration gate agree.
The runtime capability assessment distinguishes verified startup/client capture
from unverified game input and gameplay-state control. All spec areas have stable
queues linked from existing Open questions. Coverage contains fingerprinted LE
function metadata, with loader provenance and limits, rather than a stub census.
Research tracking and committed inventory checks run in the canonical gate.
Current workflow skills are adopted and the plan records Survey's remaining
research requirements without claiming those questions are answered.

Shared defects are submitted upstream: Standard PR 28 and toolkit PR 20 for
executable file-data locations, template PR 35 for isolated client capture, and
template PR 36 for queue tracking. The reviewed Standard/checker branch
commits were initially pinned while those PRs awaited review; the current
adoption below uses their merged revisions. No evidence-status
promotion or gameplay change follows from this audit.

## Current upstream migration (2026-10-01)

Owner scope: adopt latest website, template and toolkit changes fully, then
push the verified result to canonical main. Target remains this repository only.

Reviewed template delta: `e0325e0` to `7b3bbe46b251b163ee02a6539ac0d81559dbe921`.
Website snapshots: `82deb767ab64ca9922bb6347d66d9856b7640e91`.
Checker/CI: `f5e62e083eda1aac202695682f8299dc681e4fd4`.
Reporter: `7da1b93cdd9ac0d59dbaf82b66b4db95d578ab9d`.

| Capability | Adoption and acceptance evidence |
|---|---|
| Executable file-data locations | Atomic snapshot refresh, checker/CI pins, agent guidance, research skill and spec-entry templates; verify digests, links and documentation gate. |
| Overlap proofs and contested reachability | Adopt reporter files, documentation and synthetic Python/Node regressions together through sync-x86; verify lock and canonical gate. |
| Research queues | Adopt committed template parser and regressions; retain alphanumeric area support and local coverage check. Gate checks every existing queue. |
| Selected-window capture | Adopt timeout-isolated capture workers, physical DPI-aware sizing, full-content rendering and atomic failed-checkpoint cleanup with both template acceptance tests. Retain project title and runtime limits. |
| Validation and project identity | Preserve Conqueror solution, importer, package identities, LE adapter and canonical gate; template queue/reporter/capture checks are wired into it. |
| Guidance and handover | Adopt current workflow semantics and entry templates, regenerate section links, replace pending-review provenance with merged revisions. |

The local template checkout has an unrelated unfinished merge. Adoption reads
its verified committed origin/main, leaving that checkout untouched. Generic
RNG/SAVE queue scaffolds do not replace this game's existing area queues.
No gameplay, evidence status or original-content change is authorized by this
migration. Acceptance is the complete canonical fast gate, relevant synthetic
configuration/capture tests, final delta review and a verified canonical push.
Packaging identities and installer logic are retained; no release is published.

### Verification outcome

The canonical fast gate and workflow lint passed on 2026-10-01; the dated
[validation record](VALIDATION.md#upstream-migration-verification-2026-10-01)
names the checks and platform limits. Snapshot freshness matches current upstream
main content. Capture, queue checker and their tests match committed template
main after the title substitution. The reporter files and tests match their
locked toolkit revision. All delta paths were reviewed: existing project
plan/handover and validation are adapted rather than replaced by generic scaffold
state; RNG/SAVE scaffolds are intentionally retained as this game's queues.
The reviewed migration is complete and ready for the explicitly authorized
canonical push. Git is the authoritative remote synchronization record.

### Full acceptance audit

The 2026-10-01 follow-up audit verified every changed template path and retained
shared module against committed upstream. Full cross-platform CI and all installer
jobs passed for f2ae877; [VALIDATION.md](VALIDATION.md#full-migration-acceptance-audit-2026-10-01)
records exact acceptance coverage and limits. No migration blocker or pending
acceptance remains. Historical source-provenance wording above is clarified, and
the main implementation plan records the current adoption.

## Latest upstream refresh (2026-10-01)

Scope: this repository only. Adopt template 8d0eef35, rules ca39d075 and
all checker/reporter files from toolkit f8c51bfc. The reviewed delta adds
call-target, boundary, ownership, carry arithmetic, pointer inventory and
dispatch reports, address-evidence checking, matching local/CI inputs and a
staged-tree pre-commit hook. Extend the reporter lock to include the latest
pointer and dispatch dependencies and synthetic tests. No upstream scratch
binaries or query artifacts are copied. Preserve Windows configuration tests,
the LE adapter and Conqueror validation and packaging identities.

Acceptance: canonical fast gate, exact snapshot/reporter hashes, regenerated
links, staged-tree hook, final diff review and canonical explicit-refspec push.
Risks: new reporter dependencies and stricter checks; exercise the full synthetic
suites. No gameplay changes, spec promotions or unresolved owner decisions.

Outcome: canonical fast validation passed; exact reporter bytes include the
latest pointer-inventory and dispatch dependencies. See the dated validation
record for results and the local Kaitai limit.

## Current main package migration (2026-10-02)

The completed scope and acceptance evidence are recorded below and in
[VALIDATION.md](VALIDATION.md). Template main
79d18a20 and toolkit main 0b4694df supersede the previous adoption target.
Rules main remains ca39d075. The latest toolkit publishes the checker, executable
reader, Python engine and reusable .NET readers as packages; its old vendored
checker path no longer exists. Migrate active callers, CI, dependency locks and
shared Ghidra script paths together; remove superseded copies and copy-only tests.
Preserve Conqueror's LE mapping, proprietary-source boundaries and game-specific
contracts. Compare .NET capabilities before replacing local implementations.

The template NoRestore option is adapted to the canonical gate's existing
automatically restoring build rather than adding a competing restore path.
Acceptance exercises default restore behavior, explicit no-restore consumers,
unchanged checks and test filters, and failed builds without restore fallback.
Package migration and the complete template delta audit are complete.

### Package tooling adoption

The tooling batch replaces vendored checker/reporters with reader 0.2.0, checker
0.1.0 and engine 0.4.0, locked with npm integrity and Python distribution hashes.
CI and release validation install the same dependencies. The staged-tree hook
uses its staged npm lock and the local cache. Rules snapshots remain unchanged
and their freshness command now covers only the four rules/license files.

Conqueror's existing wrapper, defensive MZ/inventory/review tests and LE adapter
remain. Shared Ghidra scripts come from the engine distribution; the local LE
mapper and game-specific edition/version-tracking exports remain. The synthetic
memory-map acceptance test now loads the packaged Java helper. Toolkit-only
tests and copied guide are removed; project wrapper and lock-drift regressions
replace copy-only verification. The retained template license remains immutable.

A synthetic return report retained every pre-migration field and value; the
new engine additionally reports caller/site and return-flow observations. The
canonical fast gate and package integration tests pass. This tooling batch does
not establish final migration acceptance: comparison found that the toolkit media
APIs originate in the local PCX/Smacker readers, so adopting the shared .NET
readers is the next infrastructure batch. The importer release identity, XXH3
manifest schema and transactional asset contract remain game-specific.

### Shared resource-library adoption

ScientificMethod.LegacyFormats 0.2.0 supplies the PCX, raw indexed image,
Smacker header/audio/video/stream and cue/bin/raw ISO/CDDA APIs. Shared aliases
keep consumer names consistent; duplicate implementations are removed. The
Core project has no new dependency. Existing synthetic parser, importer and
media tests exercise the shared readers; the existing PCX parity gap and all
row statuses remain unchanged. The shared cue helper additionally retains
pregap metadata, so a declared INDEX 00 bounds the data track before INDEX 01.
This affects host media traversal, not game rules.

Conqueror retains its exact owned-release fingerprint, XXH3 version-two manifest,
asset IDs, archive extraction, transactional installation, repair and uninstall
contracts. Toolkit asset manifests use a different source/installed-content
contract and are not a drop-in replacement for those game-specific APIs.
The new general OriginalContentSource abstraction is not added as a second
import pipeline; the importer uses the equivalent shared low-level optical APIs.
CSF, Dynamix resources and LE fixup/mapping remain game-specific. Publish scripts
include the toolkit MIT notice beside the retained template notice on all platforms.

### Current-template completion audit

Reviewed the full 8d0eef35-to-79d18a20 template delta and current toolkit package
migration guide at 0b4694df. The shared implementation is provided by published
packages rather than copying the template's older vendored reporter pin.

| Changed template paths | Current adoption and proof |
|---|---|
| tools/Invoke-Validation.ps1; tests/upstream/offline-validation.test.mjs | NoRestore behavior adapted to the canonical build; command-double regressions and full gates with and without restore passed. |
| tools/evidence/sync-x86.mjs; tools/evidence/x86-lock.json | Superseded by npm/Python release locks and tools/toolkit-packages.json; no competing vendored path remains. Package lock/action/installed-version drift controls passed. |
| tools/evidence/x86-reporter/legacy-image.mjs; pointer-inventory.mjs; report.mjs | Executable reader 0.2.0; existing defensive parser/inventory tests and wrapper synthetic before/after witness passed. |
| tools/evidence/x86-reporter/report.py; x86/dispatch.py; image.py; machine.py; reports.py; trace.py | Engine 0.4.0 includes the current bounded graph, overlap, pointer-provenance, dispatch, effect-order and return-flow capabilities. Prepared protocol and wrapper regression passed. |
| tests/evidence/bridge.test.mjs; test_dispatch.py; test_x86.py | Package-only tests belong to upstream releases; project wrapper, parser, inventory and lock tests remain locally. Superseded copies removed. |
| docs/BOUNDED-EVIDENCE-REPORTERS.md; docs/EVIDENCE-TOOLS.md | Project guide links to current upstream contracts and documents the retained LE limitation. |
| docs/IMPLEMENTATION-PLAN.md; docs/VALIDATION.md; docs/TEMPLATE-CHANGELOG.md | Project migration plan and dated validation records cover the adoption; generic template state/changelog does not replace game-specific planning. |

Template changes outside this delta were adopted in prior batches; project
identities, installer GUIDs, import paths, source fingerprints, gameplay and
evidence statuses remain preserved. The global MSBuild worker policy and
Conqueror's checkout-specific gate remain intentional adaptations. Shared Ghidra
scripts and .NET media/optical readers additionally follow the current toolkit
package guide. Configuration tests, strict analysis configuration, workflow lint,
Kaitai compilation and Windows publish/installer compilation passed.

Migration acceptance is complete. Main CI at 8ae493d passed all platform gates
and Windows, Linux and both macOS installer checks; the workflow security check
also passed. See [VALIDATION.md](VALIDATION.md) for the exact runs. No release or
original-game run was involved.
