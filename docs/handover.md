# Reimplementation handover

## Current state

Stage: Survey. The infrastructure migration goal is complete. Template main
79d18a20 and toolkit main 0b4694df are adopted; Standard/Methodology/Protocol
ca39d075 remain verified unchanged. Published toolkit packages replace the
superseded vendored copies. The completed capability audit is in
[template-migration-plan.md](template-migration-plan.md).
The earlier documentation audit is in [documentation-audit.md](documentation-audit.md).
Game identities, owned-source contracts, LE adapters, gameplay and evidence
statuses are preserved. The staged-tree pre-commit hook is enabled.

## Next work

Survey remains open for full installation/media, data-family and manual-screen
reconciliation. Follow [implementation-plan.md](implementation-plan.md) and the
area queues. Source provenance is established in SRC-PATCH-CATALOG; strict
analysis readiness passes. [RUNTIME.md](RUNTIME.md) records runtime capabilities.

The next implementation slice is strategic schema-two runtime integration.
Keep it dormant until its remaining inputs, events, presentation and save/load
integration are complete; follow the parity rows.

## Verification and local state

The 2026-10-02 canonical fast gates passed normally and with NoRestore.
Kaitai compilation, strict configuration, workflow lint and local Windows
packaging passed. Canonical main CI passed all platform and installer jobs;
the workflow security audit also passed. Exact acceptance runs and limits are
in [VALIDATION.md](VALIDATION.md). Shared runtime migration is ready for review: RefurbishedDinosaurs.LegacyFormats, Media.Smacker
and Media.Playback 1.0.0 replace the old runtime pin and movie clock. The 2026-10-03 canonical
fast gate passed in this Linux workspace with the pinned evidence engine; Kaitai compilation
was unavailable. Owned-media comparisons were not run. See VALIDATION.md.

Post-commit audits found no confirmed task orphans. Preserve unrelated processes
and reusable MSBuild workers. Original source, captures and analysis artifacts
remain ignored locally. No original-game run or release publication occurred.
Push through the verified canonical remote as [AGENTS.md](../AGENTS.md) requires.
