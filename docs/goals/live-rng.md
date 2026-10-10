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
- The quiet generation probe passed screen 3 return, durable-log agreement and
  numeric replay, including its initial RULE-PERSON-003 draw. Session 88537
  completed; the owned emulator/controller exited and the machine lock is absent.
  Shutdown's pending transport diagnostic is retained locally.
- The first-answer probe passed its matching callback return and durable-log/
  numeric-replay checks. Session 92758 completed and its processes/lock exited.
- An owned quiet Continue probe is active in unified session 27133,
  output native-rng-youth-continue-20261010-a. Preserve its existing operation
  and machine lock; its source fingerprints identify the loaded controllers.
- Unfinished: native dilemma traversal, complete rule ownership,
  prescribed gameplay state and full-game recording/replay. Shared helper
  extraction is not implemented. The proposal is toolkit issue #403, linked
  from docs/dosbox-x-helper-proposal.md.
- Blockers: none established. RUNTIME.md is the capability assessment. Audio
  remains muted except for an explicit sound investigation. No WIP branch.
- Next: observe session 27133 for its Continue return or rejected caller.
  Keep unknown callers rejected and journals incomplete. Source edits do not
  alter the active run's loaded controllers.
- Continue instrumentation is committed: it waits at a temporary pointer
  classifier entry hook for supported queue/timing readiness, queues the
  prescribed click, then verifies its matching callback return. Native
  verification is pending. Its 37 recorder and seven pointer tests and full
  documentation gate passed.
- Extend supported input reachability from FND-UI-020, SCR-UI-002/003 and
  FND-PERSON-004. The three dilemma caller policies have synthetic validation;
  initial dilemma traversal passed; Continue and Reroll remain unverified.
- FND-UI-021 corrects SCR-UI-004's callback-index/rectangle association. Use
  file-order indices; CHARGEN's stored IDs differ from its last two indices.
- FND-UI-022 records both Continue returns and the next-dilemma control reset.
  Full documentation regeneration/check passed for this research batch.
  Queue the next input only after a completed callback and an observed empty
  queue with suitable pointer timing; never treat its RNG return as UI readiness.
- FND-UI-023 explicitly locates the first-answer epilogue beyond the earlier
  scoring finding. Full documentation regeneration/check passed; the probe's
  code control and return comment now cite that precise record.
- Q-STRATEGY-043 / RULE-STRATEGY-012: finish the downstream consumer review
  from FND-STRATEGY-043/044/045/046 before resolving its disputed status.
- Q-RNG-001 / RULE-RNG-001: continue seed-source provenance from
  FND-RNG-005/006 when complete clock/environment semantics are needed.
- Complete two prescribed full-game runs, trace ordering/completeness and actual
  draw-by-draw rebuild replay; startup/menu verification does not meet this condition.
