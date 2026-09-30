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

The owned build is documented in BLD-GOG-EN. That establishes build identity,
but the reviewed material does not conclusively establish the latest official
patch gate required by the template. Do not invent that provenance or set
`patchStatusEstablished` to true without evidence. Investigate documentary
provenance separately if needed; executable analysis stays blocked until the
gate is established. Report any configuration check blocked by this fact.

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

Latest official patch provenance remains unestablished and blocks original
executable analysis, not this independent infrastructure migration. Linux/macOS
installer execution and remote signing are left to their platform CI workflows.
No original executable analysis, release, push or proprietary-content import was
performed during migration.
