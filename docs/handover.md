# Reimplementation handover

## Current state

The restoration template/toolkit migration is implemented and locally validated.
Its scope, pinned revisions, adaptations and retained game-specific behavior are
in [template-migration-plan.md](template-migration-plan.md). The existing gameplay
implementation and package identities are retained. Template prerequisite audits
are not evidence that gameplay fidelity is complete; consult [PARITY.md](../PARITY.md)
and the individual parity rows.

## Validation

Run the canonical fast gate with `./tools/Invoke-Validation.ps1` or `Run Tests.bat`.
See [VALIDATION.md](VALIDATION.md) for dependencies, filters, evidence-dependent
tests and observed results. On 2026-09-30 the gate, offline documentation checks,
Kaitai compilation, workflow lint and Windows packaging/installer compilation
passed. Windows native diagnostics were checked with bounded waits and actual
GUI exit codes. Linux/macOS installer execution and remote signing were not
exercised locally. No release or push was performed.

## Prerequisite for original executable analysis

[SOURCE-EDITIONS.md](SOURCE-EDITIONS.md) records the known owned build and missing
latest-official-patch provenance. Do not mark `patchStatusEstablished` true
without evidence. `./tools/Verify-Configuration.ps1 -RequireAnalysisReady` must
pass before original executable analysis; it currently rejects this unresolved
prerequisite. Independent builds and synthetic validation remain available.

## Gameplay continuation

Resume [implementation-plan.md](implementation-plan.md) after satisfying the
source prerequisite for any analysis it needs. Existing priorities include the
unread actor transition entries in RULE-ASSAULT-008, the order-reset mode in
RULE-ASSAULT-004, and actor-ray view selection in RULE-ASSAULT-010. Consult
[ASSAULT parity](../parity/ASSAULT.md) for gaps and [BATTLE parity](../parity/BATTLE.md)
for the provisional field-battle behavior.

Strategic schema-two preparation remains dormant; do not activate it before the
remaining motion, event, presentation and save/load integration is complete.
Follow the implementation plan and existing rule/parity entries for the next
batch. Put original-game observations in `spec/`, and deliberate departures in
`deviations/`, alongside the implementation and parity changes.

## Local task state

Migration validation has finished. Post-commit audits found no confirmed orphaned
repository processes to stop. Generated analysis and validation artifacts remain
ignored local working material. Preserve them and unrelated processes; reusable
MSBuild workers are expected. Pushes require an explicit owner request and the
canonical-remote check in [AGENTS.md](../AGENTS.md).
