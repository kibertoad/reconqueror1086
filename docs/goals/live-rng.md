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
artifacts/validation-python-latest/Scripts/python.exe for evidence gates; update that
private environment with the hash-locked requirements, never the shared one.

## Handover

- Stage: Survey; research-side runtime tooling.
- Last gate: 2026-10-10 canonical -NoRestore validation passed with
  EVIDENCE_PYTHON=artifacts/validation-python-latest/Scripts/python.exe,
  executable-reader 4.0.0, engine 18.2.0, checker 4.2.0 and session 0.3.0.
  No full-game gate passed.
- Active original control: exec 19256, controller PID 59896, emulator
  PID 80336, token 5007366d0cf2, debugger ses-1, under
  artifacts/runtime-tools/native-rng-youth-v5-fixed100000-20261010-a.
  It owns the machine lock and uses quiet normal-core fixed 100000 cycles,
  session 0.3.0, v5 AGE checkpoints, five cycles and guarded entry/update
  traversal. Observe the same handle; do not restart or remove its lock.
- Earlier original controls are terminal; owned processes and locks were
  cleaned up. EXP-UI-002 and FND-UI-027 are committed. The v4 control
  ended incomplete; its failure log/journal and numeric-prefix checks passed.
- Q-PERSON-021 is closed. FND-PERSON-012, FND-PERSON-013,
  EXP-PERSON-001, EXP-PERSON-002 and RULE-PERSON-007 are committed;
  Q-PERSON-022 remains open. EXP-UI-001 remains the entry-control reference.
- Q-STRATEGY-043 remains open after FND-STRATEGY-049 and EXP-STRATEGY-003.
  RULE-STRATEGY-012 remains disputed. Documentation and evidence checks passed.
- Toolkit issues #319, #385, #388, #392 and #403 were consumer-validated and closed.
  PR #419 is adopted. #402, #412, #413 and #420 remain open with their
  original-game confirmation, feature or upstream prerequisites unresolved.
  #416 awaits its next session release and killed-owner control. The flat
  isolated-call segment-model controls are committed; their reusable setup
  result was added to existing toolkit issue #7 after duplicate checking.
- Q-RNG-001 remains open after FND-RNG-007 and EXP-RNG-002. Documentation,
  parser controls and evidence/package checks passed; no parity status changed.
- The AGE-checked v4 and registered-update v5 tooling contracts are committed.
  Canonical validation and final consumer controls passed; native v5 is running.
- Unfinished: native youth/dubbing traversal, complete caller ownership, prescribed full-game
  state, two full-game recordings and actual rebuild replay. No WIP branch.
- Blockers: none established. The pre-existing untracked .claude/settings.json
  remains untouched. No push is authorized.
- Next: observe and verify the owned v5 native control for
  RULE-PERSON-004 / SCR-UI-019.
- Next: Q-PERSON-022 lifecycle review.
- Next: Q-STRATEGY-043 / RULE-STRATEGY-012 downstream consumer review.
- Next: Q-RNG-001 / RULE-RNG-001 seed-source provenance where required.
- Next: prescribed full-game recordings and trace completeness, followed
  by actual draw-by-draw rebuild replay.

