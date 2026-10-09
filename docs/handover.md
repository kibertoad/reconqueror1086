# Reimplementation handover

## Current state

Stage: Survey. The rules/library and existing-evidence conversion goal is met.
Current upstream provenance and exact dependency versions are authoritative in
tools/upstream-lock.json, tools/toolkit-packages.json, Directory.Build.props,
pnpm-lock.yaml and requirements-evidence.txt. The acceptance audit is in
[template-migration-plan.md](template-migration-plan.md).

Existing native inventories, provenance/region sidecars, active report callers,
shared unpacking, reviewed location notation and workflow guidance are current.
Evidence and parity statuses are preserved. Use the generated status indexes,
PARITY.md and tools/Report-Coverage.mjs for progress and its explicit limits.
Coverage does not establish a complete reading or completed restoration.

## Next work

- Continue Survey through Q-RES-017 / FMT-RES-015 and the area queues named by
  [implementation-plan.md](implementation-plan.md).
- Establish the unavailable code baselines and driver-payload scope recorded
  in coverage/baseline.json; follow [coverage/README.md](../coverage/README.md).
- Audit complete readings only from entries' complete_reading findings; leave
  their function-level coverage unavailable until that audit exists.
- The next implementation slice is strategic schema-two runtime integration,
  with the rows and activation conditions in implementation-plan.md. Use an
  implementation session working from the spec alone.

## Verification and local state

The 2026-10-09 default canonical Invoke-Validation.ps1 gate passed documentation,
Kaitai, migrated evidence regressions, solution build, non-long-running xUnit
selection and executable specifications. Frozen dependency installation,
installed-version verification, strict source readiness, native sidecars and
boundary audits passed. Long-running tests and remote cross-platform CI were
not run. [VALIDATION.md](VALIDATION.md) records the checks and limitations.

Unfinished: none. .claude/settings.json remains unrelated and untracked.
No original game ran and no push or publication was performed. Publishing
still requires the owner's explicit request and the canonical remote check.

Local frozen adoption contracts, migration scripts and proof reports remain
ignored under artifacts/coverage-migration; its {le,pe,ne,config,inst}.json
contracts retain the source/snapshot paths and identities for re-adoption.
Original source, captures, decoded resources and analysis databases remain local.
Post-commit audits found no confirmed repository orphans. Preserve unrelated
processes and reusable MSBuild workers. RUNTIME.md records run capabilities
and the original-game lock; emulated calls need no run lock.
