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
- Last gate: 2026-10-10 full documentation check passed with checker 4.1.1;
  41 pointer-input, recorder, journal, event-log and mapping safety tests passed.
  Prior canonical configuration/build gates remain recorded in VALIDATION.md.
- Native startup input and guarded initial loaded-screen return passed in two
  fresh quiet runs with different seeds. Both durable prefixes match their
  journals and numeric replay. Their owned processes and locks were cleaned up.
  No full-game or actual rebuild replay gate has passed.
- An owned quiet title-continuation probe is running with the machine lock
  held. Preserve its existing operation. It uses fixed cycles 1000, disabled
  animations, startup checkpoints, startup click, loaded-screen continuation
  and title click. Source fingerprints identify its eagerly loaded controllers.
  Title-to-options traversal and original continuation remain unverified.
- Unfinished: complete rule ownership, full-game recording/replay and prescribed
  observable gameplay state. Shared helper extraction is not implemented.
  Reproducibility guidance and helper boundaries were sent upstream in toolkit
  issue #403 after duplicate checking; docs/dosbox-x-helper-proposal.md links it.
- Blockers: none established. RUNTIME.md is the capability assessment. Audio
  remains muted except for an explicit sound investigation. No WIP branch.
- Next: observe the existing title-continuation operation for its guarded input
  or next unaccepted caller; verify returned screen state before further writes.
- Then extend supported input reachability and RNG caller policies from direct
  findings, using the local call-survey report only as leads. Keep unknown
  callers rejected and journals explicitly incomplete.
- Q-STRATEGY-043 / RULE-STRATEGY-012: finish the downstream consumer review
  from FND-STRATEGY-043/044/045/046 before resolving its disputed status.
- Q-RNG-001 / RULE-RNG-001: continue seed-source provenance from
  FND-RNG-005/006 when complete clock/environment semantics are needed.
- Complete two prescribed full-game runs, trace ordering/completeness and actual
  draw-by-draw rebuild replay; startup verification does not meet this condition.
