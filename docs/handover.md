# Reimplementation handover

## Current state

Stage: Survey. Template main 39d31fde is adopted, with the Standard at rules
c1758fd9 (rules numbered, still v1), checker 0.2.0 at toolkit a260e391, reader
1.0.0, engine 1.0.1 and the RefurbishedDinosaurs 6.2.0 runtime packages. Tooling
installs through pnpm. CI for this adoption has not run yet. The completed capability audit is in
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

The 2026-10-05 canonical fast gate passed after adopting shared runtime 6.2.0.
Shared APIs now own import/install helpers, hashing, portable paths, palette and
PCM conversion, settings recovery, content discovery, viewport scaling and text
wrapping. Existing game manifest and settings contracts remain in adapters.
The callback-based generated-file writer and game-specific verification remain
local. Acceptance details and limits are in [VALIDATION.md](VALIDATION.md).
Kaitai compilation was skipped because no compiler was available; owned-media
comparisons and LongRunning tests were not run. No original game ran.

The pinned Python engine is installed in ignored artifacts/validation-python;
set EVIDENCE_PYTHON to its Scripts/python.exe for this checkout's gate. The
machine's existing engine 8.1.0 is restored. Tooling dependencies are installed
from the existing locks. Remote CI has not been exercised for this update.

Post-commit audits found no confirmed task orphans. Preserve unrelated processes
and reusable MSBuild workers. Original source, captures and analysis artifacts
remain ignored locally. No original-game run or release publication occurred.
Push through the verified canonical remote as [AGENTS.md](../AGENTS.md) requires.
