# Protocol continuation

## Condition

Continue the owner's ongoing restoration work according to the local Protocol
and document results using the local Standard. Record suggestions and concerns
for template, process, Protocol, Standard and shared-tooling authors in `gaps.md`.
Commit completed batches and handovers; do not push to main. The owner supplied
no finite completion condition or turn limit, so this goal remains ongoing;
completion of one batch does not complete the objective.

## Scope

Areas: project planning, Survey reconciliation, BLD-GOG-EN build inventory, RES inventory findings, formats and queue planning, and shared research tooling.
Batches: research and its tooling only. Select concrete Survey research areas
and record their claims here before changing their entries or queue items.
Only this goal runs locally while publication is not authorized.

## Must not touch

Gameplay implementation under `src/`. Proprietary content and generated analysis
artifacts in Git. Immutable rules snapshots. Remote configuration.

## Dead ends

None known.

## Handover

- Stage: Survey. Research remains at FND-RES-012, FMT-RES-005 and FMT-RES-116.
  Q-RES-007 is closed; Q-RES-120 through Q-RES-123 remain Static.
- Completed tooling: 5b18ad6 updates toolkit adoption and CI action to 2390d04,
  engine to 0.6.0 and adds pypcode 4.0.0. Other toolkit packages were latest.
  GAP-005 records the replaced duplicated installed-version assertion.
- Last checks: 2026-10-02 canonical fast gate passed with Kaitai 0.11, along with
  hash-checked downloads and actionlint. See docs/VALIDATION.md. No parity
  validation is claimed and no original program ran for this tooling session.
- Unfinished: none from the dependency update. No push was performed. Process
  audits found no confirmed repository orphans; reusable MSBuild workers remain.
- Blockers: none known.
- Next: resume Q-RES-009 / FMT-RES-007 and manual-screen reconciliation in a
  research session. Rules snapshots remain unchanged unless the owner requests
  their refresh. Gameplay implementation remains outside this goal's scope.
