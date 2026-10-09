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
- Dependency adoption and inventory-export/range-audit tooling are committed.
  No publication is authorized. No evidence status changed; no original game ran.
- Checks: six conversion/sidecar regressions pass. ExportCoverageSnapshot.java
  compiled in installed Ghidra, and repeated exports of all five sources are
  identical. Verified stable project digests and source SHA-256/XXH3 identities.
  All five converted inventories pass the shared canonical reader and the new
  Check-Coverage.mjs sidecar/body-intersection validation.
- Documentation gate on the current unfinished conversion FAILS on legacy range
  ends. Audit-RangeEnds.mjs reports all candidates without the diagnostic cap;
  its ignored report is the review set. Remaining candidates need intended-scope
  review, the entry's own last-item/size evidence, or whole-entry supersession.
  Grouped function extents are review hints only. Do not increment every end.
- Uncommitted migration work: five canonical start/size/ranges inventories, ten
  provenance/regions sidecars, the replacement tools/Check-Coverage.mjs,
  the new extent finding and the notation conversions citing it. Conversion helpers and
  ExportBoundaryMetadata.java are uncommitted, as are coverage/tool guidance
  and regenerated spec indexes. The draft finding approaches its line limit;
  put additional extents in a separately allocated finding.
  Keep these together with their evidence corrections for final validation.
  .claude/settings.json was already untracked and remains unrelated.
- Raw exports and adoption contracts: artifacts/coverage-migration/{le,pe,ne,
  config,inst}.json. Re-adoption validates source identities, snapshot digest
  and completion marker. Raw output remains ignored and contains no Git assets.
- LE snapshot: analysis/documentation-audit/ghidra/LeInventory2.
  PE/NE: artifacts/launcher-inventory-{pe,ne}-20261005/Launcher{PE,NE}.
  Rebuilt MZ snapshot: artifacts/coverage-migration/mz-project/MzCoverage,
  programs CONFIG.EXE and INST.unpacked.exe. Repeated stable exports have the
  suffix -stable; source/mapping qualifications and boundary changes still need
  documentation in coverage/README.md. Historical size-only bodies cannot prove
  that their start-plus-size interval was contiguous.
- Origin refs were fetched on 2026-10-09. The new RES extent finding is a draft;
  its next sequential ID was checked unused but has not been allocated.
  Recheck IDs before taking additional findings. LE project digest remained
  unchanged after the metadata-only boundary probe. All six conversion tests
  and all five native inventory/sidecar checks pass on the unfinished tree.
- Next: record verified native function extents and review/correct legacy ends;
  reconcile ambiguous candidates without changing evidence claims silently;
  update coverage/caller documentation and remove obsolete metadata paths;
  audit all value/address ranges and ISO listing/accounting; finish the full
  requirement-by-requirement acceptance audit in template-migration-plan.md.
- Remaining scope includes existing report configurations/active callers,
  complete-reading claims, progress semantics and all workflow guidance. A green
  sidecar check alone does not prove the goal complete.
- Blockers: none requiring an owner decision. Preserve MSBuild reusable workers.
