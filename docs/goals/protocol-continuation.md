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
- Adopted dependency locks and rules snapshots are in their authoritative lock
  files. The dependency-adoption batch is committed; full acceptance is pending.
- Last checks: 2026-10-09 canonical gate passed with
  FullyQualifiedName~ResourceAndDefinitionTests, minimum discovery 1; startup
  failure reporter regression passed separately. Kaitai compiled. Upstream
  freshness matched rules/checker main. No original program ran.
- Unfinished: inventory body ranges, provenance and region sidecars; complete
  half-open-range and build/listing accounting audits; conversion guidance and
  final acceptance. docs/template-migration-plan.md lists the requirements.
- Local state: .claude/settings.json was already untracked; preserve it.
- Existing LE project: analysis/documentation-audit/ghidra/LeInventory2.
  PE/NE projects: artifacts/launcher-inventory-pe-20261005/LauncherPE and
  artifacts/launcher-inventory-ne-20261005/LauncherNE. MZ snapshot folder
  analysis/original/mz-inventory/proj is empty; recover or rebuild it with
  verified source/mapping rather than inventing provenance.
- Shared ExportFunctionInventory.java still exports only starts and sizes;
  the new protocol needs a project exporter/adapter with actual body ranges
  and the denominator partition. Existing Check-Coverage.mjs does not prove it.
- No rule, format, screen or bug is established; complete-reading claims must
  not be inferred from evidence citations. No legacy root validation record
  or original-reading parity test requires migration.
- Next: implement and validate inventory export/sidecar tooling; re-export all
  five inventories; audit range ends and primary ISO path/accounting; update
  remaining guidance; perform the full requirement-by-requirement audit.
- Blockers: none requiring an owner decision. No push is authorized.
