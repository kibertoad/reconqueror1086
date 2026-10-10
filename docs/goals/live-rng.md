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
- Last gate: 2026-10-10 full documentation and recorder-related safety checks
  passed with checker 4.1.1. Prior canonical configuration/build gates remain
  recorded in VALIDATION.md.
- Native startup input and guarded initial loaded-screen return passed in two
  fresh quiet runs with different seeds. A subsequent title-to-options probe
  verified ordered initial/replacement returns and matching journal replay.
  Those owned processes and locks were cleaned up. No full-game or actual
  rebuild replay gate has passed.
- The guarded New Game probe passed character-options return with matching
  durable events and numeric replay. Its emulator and lock were cleaned up.
- An owned quiet generation probe is running with the machine lock held.
  Preserve its existing operation. It uses fixed cycles 1000, disabled
  animations, startup checkpoints/click, loaded-screen continuation, title
  input, screen checkpoints, new-game input and generation input, targeting
  youth generation. Its unified session is 88537 and emulator PID is 63084.
  Its eagerly loaded controllers are identified by local source fingerprints.
- Unfinished: native dilemma traversal, complete rule ownership,
  prescribed gameplay state and full-game recording/replay. Shared helper
  extraction is not implemented. The proposal is toolkit issue #403, linked
  from docs/dosbox-x-helper-proposal.md.
- Blockers: none established. RUNTIME.md is the capability assessment. Audio
  remains muted except for an explicit sound investigation. No WIP branch.
- Next: observe the existing generation operation for its verified target or
  next unaccepted caller. Keep unknown callers rejected and journals incomplete.
- Extend supported input reachability from FND-UI-020, SCR-UI-002/003 and
  FND-PERSON-004. The three dilemma caller policies have synthetic validation;
  their original traversal is not yet verified.
- Before scripting youth answers or Continue, recheck SCR-UI-004's button
  rectangles against file-order callback indices from FND-UI-002. Do not infer
  a callback index from the HAT record's stored ID; these differ in CHARGEN.
- Q-STRATEGY-043 / RULE-STRATEGY-012: finish the downstream consumer review
  from FND-STRATEGY-043/044/045/046 before resolving its disputed status.
- Q-RNG-001 / RULE-RNG-001: continue seed-source provenance from
  FND-RNG-005/006 when complete clock/environment semantics are needed.
- Complete two prescribed full-game runs, trace ordering/completeness and actual
  draw-by-draw rebuild replay; startup/menu verification does not meet this condition.
