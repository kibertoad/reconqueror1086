# latest-infrastructure

## Condition

Conqueror adopts current template main 79d18a20cb4d97c7153e74e5695cbb80e7ebf73e and toolkit main 0b4694df7edb621c19171dd79718458378c811a2 capabilities, with Standard/Methodology/Protocol ca39d0750e67c8c3900e8554e66a84083fe67452 verified unchanged. Published package dependencies replace superseded vendored toolkit copies and all active callers. The canonical fast gate, package/wrapper regressions, snapshot integrity, relevant configuration/installer checks and final semantic delta audit pass. The migration plan records intentional game-specific retentions and evidence for every acceptance criterion.

## Scope

This repository only. Infrastructure, tooling, dependency configuration, CI and migration documentation. No research or gameplay implementation. Preserve project identities, source contracts, LE adapter and evidence statuses. The owner authorized push to canonical main after completion; no release is authorized.

## Must not touch

Original content, gameplay rules, spec claims, queue items, unrelated sibling worktrees and remote URLs.

## Dead ends

- The old upstream freshness command returns HTTP 404 at current toolkit main because the checker moved into packages; update the command and lock schema, rather than accepting this as freshness evidence.
- Restricted shell access cannot fetch upstream and uses a different Git owner. Authorized escalated reads and validation succeed.

## Handover

- Stage: Survey; this goal handles infrastructure migration only.
- Last gate: 2026-10-02, full fast gates passed normally and with NoRestore after package/resource adoption. Kaitai compilation, strict configuration, workflow lint and Windows portable/installer build passed.
- Unfinished: none in the implementation. Final cross-platform CI verification follows the owner-authorized canonical main push.
- Blockers: none. Current upstream heads remain template 79d18a20, toolkit 0b4694df and rules ca39d075.
- Next: push through verified origin with HEAD:main, inspect the CI run for that commit, fix any failures and record acceptance before closing this goal. Shared package versions are reader 0.2.0, checker 0.1.0, engine 0.4.0 and .NET Core/LegacyFormats 0.2.0. Preserve reusable MSBuild workers; audits found no orphans.
