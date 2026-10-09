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

- Stage: Survey; this goal adds runtime research capabilities.
- Last gate: 2026-10-09 tooling synthetic checks and documentation passed.
- Unfinished: live mapping audit and full-game rule-tagged draw recording.
  Verify-LiveSnapshot.py finds a unique RNG identity control and unanimous code
  relocation target bases in local live-rng-map-e/physical-memory.bin. Residual
  code/data writes and selectors still require audit; no live map is established.
- Blockers: none established. A diagnostic exit breakpoint identified digital
  driver loading before disc setup completed. Guest readiness gating fixed the
  setup ordering. The 1,000-cycle probe reached the startup seed entry described
  in FND-RNG-003, captured its stack argument and exited with the run lock clear.
- Next: finish startup instrumentation verification; audit residual code writes
  and live selector identity. Candidate breakpoints and a bounded instruction
  trace now reach the loaded game and its initial seed; no draw recording exists
  yet. Then map RNG
  callers from spec evidence and validate deterministic full-game recording.
- Checks: four lifecycle and nine synthetic emulator checks passed;
  local snapshot comparison completed. These do not establish gameplay coverage.
  Analysis-readiness, repository and documentation gates passed without skips.
