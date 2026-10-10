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
artifacts/validation-python-183-session04/Scripts/python.exe for new evidence gates.
Preserve validation-python-latest for the historical native v5 recording;
never update the shared environment.

## Handover

- Stage: Survey; research-side runtime tooling.
- Stopped: owner asked to wrap up for today and paused the goal. Its condition
  remains unmet. No new item or session starts during this wrap-up.
- Last gate: canonical validation passed on 2026-10-10 with engine 18.3.0,
  executable-reader 4.0.0, checker 4.2.0 and session 0.4.0, including the final
  wrap-up rerun. Use command-scoped EVIDENCE_PYTHON pointing to
  artifacts/validation-python-183-session04/Scripts/python.exe.
- The owned native v5 control under
  artifacts/runtime-tools/native-rng-youth-v5-fixed100000-20261010-a is terminal
  and incomplete. Its failed log, ordered numeric prefix and AGE prefix were
  checked with its historical session-0.3.0 interpreter. Controller/emulator
  processes exited and the owned machine lock was released. Local captures stay
  ignored; no successful dubbing-return or full-game gate passed.
- FND-RNG-007 through FND-RNG-014 and EXP-RNG-002 through EXP-RNG-007 are
  committed; Q-RNG-001 remains open and RULE-RNG-001 remains supported.
- Q-PERSON-021 is closed; Q-PERSON-022 remains open. EXP-UI-002 and FND-UI-027
  are committed. Q-STRATEGY-043 remains open; RULE-STRATEGY-012 remains disputed.
- Shared library adoption and issue review are committed. Toolkit #319 and
  #406 were validated and closed; upstream closed #416 with session 0.4.0.
  #402, #412, #413 and #420 retain their unmet acceptance requirements.
- Toolkit #456 retains the protected-mode gating acceptance gap. The standalone
  timer-wait helper is not integrated into the recorder. Consult the issue's
  latest response before adopting a future shared gate.
- Unfinished: verified youth/dubbing traversal, complete caller ownership,
  prescribed full-game state, two different-seed full-game recordings, trace
  completeness/order checks and actual draw-by-draw rebuild replay. No WIP branch.
- Blockers: no new owner decision required. The pre-existing untracked
  .claude/settings.json remains untouched.
- Next: verify timer-wait instrumentation for RULE-PERSON-004 / SCR-UI-019,
  then run a fresh owned v5 control after explicit resumption.
- Next: Q-PERSON-022 lifecycle review.
- Next: Q-STRATEGY-043 / RULE-STRATEGY-012 downstream consumer review.
- Next: Q-RNG-001 / RULE-RNG-001 remaining seed-source provenance.
- Next: prescribed full-game recordings, completeness and actual rebuild replay.
