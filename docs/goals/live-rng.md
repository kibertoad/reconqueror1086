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
- Last gate: 2026-10-09 mapping, lifecycle and emulator synthetic checks passed;
  the complete documentation gate passed without skips. EXP-RNG-001 and the
  recorder safety checks passed in the following research batch.
- Unfinished: rule ownership, full-game recording/replay, prescribed observable
  gameplay state, and reproducible upstream/shared-helper proposal. No WIP branch.
- Blockers: none established. RUNTIME.md is the current capability assessment.
- Next: Q-RNG-001 / RULE-RNG-001, finish the static time-source reading; then
  extend the verified EXP-RNG-001 bounded recorder to additional supported
  callers, rejecting unowned calls. Validate recorder completeness and ordering
  in two different-seed runs before proposing upstream extraction.
- Evidence references: EXP-RNG-001, FND-RNG-004 and FND-RNG-003. Per-run checks live in
  tools/live_mapping.py; tools/Probe-LiveMapping.py verifies native seed/draw
  state identity. Capture paths and hashes belong to the finding, not this file.
