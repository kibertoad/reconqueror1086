# Reimplementation handover

## Current state

Stage: Survey. Template main 25c5808b is adopted. The Standard and protocol are
at rules efa138ba (still v1), with checker 2.2.0 at toolkit 92a55920, reader
2.3.0, engine 12.0.0 and the RefurbishedDinosaurs 10.0.0 runtime packages.
Tooling installs through pnpm. The adoption record is in
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

The pinned Python engine 12.0.0 is installed in ignored
artifacts/validation-python; set EVIDENCE_PYTHON to its Scripts/python.exe for
this checkout's gate. The machine's global Python keeps engine 8.1.0 for other
work. Tooling dependencies are installed
from the existing locks. Remote CI has not been exercised for this update.

Post-commit audits found no confirmed task orphans. Preserve unrelated processes
and reusable MSBuild workers. Original source, captures and analysis artifacts
remain ignored locally. No original-game run or release publication occurred.
Push through the verified canonical remote as [AGENTS.md](../AGENTS.md) requires.
