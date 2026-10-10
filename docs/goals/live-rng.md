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

- Stage: Survey; research-side runtime tooling session.
- Last gate: 2026-10-10 canonical -NoRestore validation passed with
  EVIDENCE_PYTHON=artifacts/validation-python-latest/Scripts/python.exe,
  engine 18.0.0 and session 0.3.0. Published-session Windows tests and consumer
  recorder/input/event-log/terminal-validator controls passed. No full-game gate passed.
- Active original probe: exec session 8919, controller PID 87724, emulator
  PID 43948, session token 02075155a048, debugger session ses-1. The quiet
  startup control under artifacts/runtime-tools/native-rng-session03-startup-fixed10000-20261010-a
  owns the machine lock. It uses session 0.3.0, normal core and fixed 10000 cycles.
  Observe the same handle; do not launch another probe or remove its lock.
- Earlier six-cycle, auto-core, GOG-cycle, entry-control and AGE comparisons
  are terminal and incomplete; their owned processes and locks were cleaned up.
  EXP-UI-001 records the narrow successful entry-control observation.
  Q-PERSON-021 remains open; the newer AGE capture and transport diagnostic
  remain local. No WIP branch.
- Shared lifecycle, guarded writes, event logging and module hashes are adopted
  from published session 0.3.0. Toolkit issues #385, #388 and #392 were validated
  and closed. #403 received synthetic logging-adoption results and remains open
  for the native startup control. Verified checkpoint reuse remains deferred in #420.
- Unfinished: native 0.3.0 startup verification, Q-PERSON-021 reconciliation,
  repeated youth/dubbing traversal, complete caller ownership, prescribed full-game
  state, two full-game recordings and actual rebuild replay.
- Blockers: none established. The pre-existing untracked .claude/settings.json
  remains untouched. No push is authorized.
- Next: finish the owned 0.3.0 startup control, validate the package log and
  matching terminal journal, audit cleanup, and provide the native result to #403.
- Next: Q-PERSON-021 / RULE-PERSON-004 reconciliation before changing the
  recorder's prescribed completion contract.
- Next: Q-STRATEGY-043 / RULE-STRATEGY-012 downstream consumer review.
- Next: Q-RNG-001 / RULE-RNG-001 seed-source provenance where required.
- Next: two prescribed full-game recordings and trace completeness, followed
  by actual draw-by-draw rebuild replay. Startup controls do not satisfy this.
