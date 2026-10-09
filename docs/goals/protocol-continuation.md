# Protocol continuation

## Condition

Update to the latest published Standard, work protocol and shared libraries,
and fully convert existing data and evidence to their requirements. Completion
requires verified upstream provenance, consistent installed dependency locks,
conversion of every affected active caller and evidence entry, updated workflow
guidance, regenerated documentation, and passing applicable validation gates.
Preserve evidence uncertainty and immutable historical findings; conversion
does not authorize promoting claims. Commit completed batches and handovers on
this goal branch. Publishing remains unauthorized.

## Scope

Areas: all existing evidence and tracking, workflow guidance, upstream snapshots,
dependency locks, shared research tooling and runtime-library integrations.
Batches: tooling migration and evidence conversion, kept separate where required.
Only this goal runs locally while publication is not authorized.

## Must not touch

Gameplay rule changes. Proprietary content and generated analysis artifacts in
Git. Manual edits to immutable upstream snapshots. Remote configuration.

## Dead ends

None known.

## Handover

- Active: latest rules/libraries and full evidence conversion. Stage: Survey.
- Dependencies, native inventories/sidecars, boundary conversions and citation
  replacements are committed. FND-RES-062 and FND-RES-063 are the metadata
  findings. Claims retain their statuses; uncertain historical findings have
  replacements. No original game ran and no publication is authorized.
- Last gate: 2026-10-09, documentation check with index regeneration passed;
  canonical Invoke-Validation.ps1 passed with ResourceAndDefinitionTests filter
  and MinimumExpectedTests 1. Coverage conversion regressions and all native
  inventory/sidecar checks pass. Audit-RangeEnds.mjs reports no diagnostics.
  These checks do not close the goal's wider migration requirements.
- Unfinished working tree: none. .claude/settings.json remains unrelated and
  untracked. One-time migration scripts and proofs are retained only in ignored
  artifacts/coverage-migration; durable exporters and checks are committed.
- Raw exports and adoption contracts: artifacts/coverage-migration/{le,pe,ne,
  config,inst}.json. Re-adoption validates source identities, snapshot digest
  and completion marker. Raw output remains ignored and contains no Git assets.
- LE snapshot: analysis/documentation-audit/ghidra/LeInventory2.
  PE/NE: artifacts/launcher-inventory-{pe,ne}-20261005/Launcher{PE,NE}.
  Rebuilt MZ snapshot: artifacts/coverage-migration/mz-project/MzCoverage,
  programs CONFIG.EXE and INST.unpacked.exe. Repeated stable exports have the
  suffix -stable. Mapping qualifications are in coverage/README.md. Historical
  size-only bodies cannot prove a contiguous start-plus-size interval.
- Next: migrate active inventory/report callers and obsolete metadata paths;
  audit value/address ranges beyond the checker's function-boundary diagnostics;
  reconcile ISO listing/build path accounting; audit complete-reading/progress
  semantics and remaining workflow guidance; finish the explicit acceptance
  audit in template-migration-plan.md, dependency freshness and final gates.
- Blockers: none requiring an owner decision. Preserve MSBuild reusable workers.
