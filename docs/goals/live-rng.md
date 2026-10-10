# live-rng

## Condition

The live CONQUER.EXE code/data mapping is verified independently against the
fingerprinted LE image and its relocations; a scripted full-game recorder
captures initial seeds, reseeds and every RNG draw as rule-tagged bound/result
records, rejects unmapped draws, and captures divergence/state diagnostics
locally. At least two original runs with different seeds verify the recorder
from a prescribed observable state, including trace completeness and ordering.
Synthetic safety/transport/recording checks and the documentation check pass.
RUNTIME.md gives repeatable commands and accurately records capability limits.
Completion requires the actual running game, not only loader or isolated calls.
After verified use, provide reproducible setup/build guidance, identify shared
helper boundaries and send an upstream integration proposal after duplicate
checking. Keep game-specific mapping and rule ownership in this repository.

## Scope

Areas: RNG, UI, MEDIA, SOUND, RES, SAVE, PERSON, STRATEGY, ASSAULT, BATTLE,
VIEW, ESTATE, TALK, DRAGON, JOUST, TOURNEY, CONFIG. Batches: research and
research-side tooling only. Inspect other areas only for evidence-backed RNG
caller ownership, necessary state reachability, and instrumentation validation.
Current runtime target: CONQUER.EXE. Other runtime analysis remains Later.

## Must not touch

Rebuild gameplay code under src/ and its implementation tests. Do not promote
claims from tooling success, modify the original executable or its installation,
commit proprietary bytes/captures/disassembly, or push without owner request.

## Dead ends

DOSBox-X debugger entry fails with redirected console handles, -noconsole or
CREATE_NO_WINDOW. Use a hidden native console. The portable Release build has
only native command transport; the pinned Agent Debug SDL2 build adds structured
operations. RPC readiness before AUTOEXEC completion can launch without D:;
observe the guest-written drive-setup marker before starting the original.
Debugger memory commands require numeric addresses and explicit dump filenames.
The default Python environment is shared with toolkit development and may hold
an editable engine. Use command-scoped EVIDENCE_PYTHON pointing to
artifacts/validation-python/Scripts/python.exe for evidence gates; update that
private environment with the hash-locked requirements, never the shared one.

## Handover

- Stage: Survey; research-side runtime tooling session.
- Last gate: 2026-10-10 canonical validation with -NoRestore passed using
  EVIDENCE_PYTHON=artifacts/validation-python/Scripts/python.exe. Shared session
  0.2.0 published-source Windows suite, adapter, pointer-input, native-recorder
  and terminal-validator checks passed. No full-game gate passed.
- Active original probe: exec session 11602, controller PID 65940, emulator
  PID 62364, debugger session ses-1. The quiet combined traversal under
  artifacts/runtime-tools/native-rng-shared-dubbing-entry-fixed10000-20261010-a owns
  the machine lock. It uses session 0.2.0, normal core and fixed 10000 cycles.
  Observe the same handle; do not launch another probe or remove its lock.
- Earlier six-cycle, auto-core, GOG-cycle and first shared fixed-cycle comparisons are terminal and incomplete;
  their owned processes and locks were cleaned up. Dated diagnostic limits
  are in docs/VALIDATION.md; captures remain local.
- Shared lifecycle and guarded writes are adopted from published session 0.2.0
  (PRs #414/#419); obsolete local helper and raw write extension were removed.
  Native traversal completion remains pending. Toolkit issue #403 received the
  release/synthetic and terminal native consumer results; event logging remains issue #409. Verified startup
  reuse feasibility is tracked in issue #420.
- Unfinished: repeated youth and dubbing traversal, complete caller ownership,
  prescribed full-game state, two full-game recordings and actual rebuild replay.
- Blockers: none established. No WIP branch. The pre-existing untracked
  .claude/settings.json remains untouched.
- Next: finish the owned combined traversal with FND-UI-026 entry input guards and validate its terminal
  journal and published 0.2.0 guarded pointer-write record.
  Record process/lock cleanup and send the terminal consumer result to #403.
- Next: verify youth traversal and dubbing with the terminal validator's
  matching outcome contracts; resolve any observed cycle-count mismatch before accepting completion.
- Next: Q-STRATEGY-043 / RULE-STRATEGY-012 downstream consumer review.
- Next: Q-RNG-001 / RULE-RNG-001 seed-source provenance where required.
- Next: verify two prescribed full-game recordings and trace completeness,
  then actual draw-by-draw rebuild replay. Startup checks do not satisfy this.
