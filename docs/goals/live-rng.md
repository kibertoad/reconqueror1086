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

## Handover

- Stage: Survey; research-side runtime tooling session.
- Last gate: 2026-10-10 analysis-ready configuration, full documentation check,
  native-recorder, supported-pointer, journal and terminal-validator checks passed. Canonical
  fast gate passed with -NoRestore; its build and test artifacts are local.
- Continue verification finished successfully; its terminal journal check,
  cleanup and limits are recorded in docs/VALIDATION.md. No full-game gate passed.
- Active probe: exec session 4782, quiet fresh six-cycle youth verification under
  artifacts/runtime-tools/native-rng-youth-six-cycles-20261010-a. Controller PID
  61028 owns emulator PID 19484 and the machine run lock. Local
  session-identity.json identifies debugger session ses-1. Startup mapping
  is underway; repeated-cycle and dubbing verification remain pending.
  Poll this same owning handle; do not launch another original or remove its
  lock while it is live. Diagnostic files and original content stay local.
- Unfinished: repeated youth traversal, complete caller
  ownership, prescribed full-game state, two full-game recordings and actual
  rebuild replay. Shared helper extraction remains unimplemented; toolkit
  issue #403 is linked from docs/dosbox-x-helper-proposal.md.
- Blockers: none established. No WIP branch. The untracked
  .claude/settings.json is pre-existing and was left untouched.
- Next: finish observing the active six-cycle probe and run
  tools/verify_native_recording.py with expected youth-sequence-return-reached;
  inspect matching cycle count, dubbing screen identity and process/lock cleanup.
- Next: Q-STRATEGY-043 / RULE-STRATEGY-012 downstream consumer review;
  Q-RNG-001 / RULE-RNG-001 seed-source provenance when required.
- Next: verify two prescribed full-game recordings and trace completeness,
  then actual draw-by-draw rebuild replay. Startup and menu checks do not
  satisfy this condition.
