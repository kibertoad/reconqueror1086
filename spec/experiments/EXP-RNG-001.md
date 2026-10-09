---
id: EXP-RNG-001
title: Native startup character shifts yield thirty ordered rule-tagged draws from each recorded seed
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11 Pro 10.0.26200, DOSBox-X 2026.10.01 Agent Debug SDL2, normal core at 1000 cycles, S3 SVGA, 16 MiB RAM
starting_state: new-game
recording: null
repetitions: 2
fixture: EXP-RNG-001.json
---

## Question

Do the initial character-table shifts in RULE-PERSON-002 make the ordered draws
described by FND-PERSON-003, and can their bounds, native results and final
`rng_state` be recorded without putting code addresses or character content
into the fixture?

## Setup

Each case starts a fresh original process with the verified BLD-GOG-EN source,
not a restored save. No campaign or character choice has been made yet. The
starting point for the draw comparison is the return from the initial seed
routine, before character-table initialization. FND-RNG-003 locates that seed
call and FND-RNG-004 identifies its running state.

Use official DOSBox-X source revision
`b6abbd5980a885f5f310a4088c59a8688d1b116c`, configuration `Agent Debug SDL2`,
x64, built with MSVC v142 and Windows SDK 10.0.19041.0. Use a hidden native
console, normal CPU core at 1,000 cycles, S3 SVGA and 16 MiB memory. Give each
case a fresh private C drive with copies of the owned GOB, INI and fingerprinted
executable, and mount the owned disc read-only at D:. Set only MOVIE and CREDITS
to OFF in the private INI; keep the other owned settings. Observe guest drive
setup completion before launch and hold the exclusive machine run lock.

Derive the code and data mappings independently as FND-RNG-004 describes, and
validate the complete relocated code, descriptors and RNG pointer at the seed
stop. Check the saved return address against the initial seed caller. Read the
seed argument, step the unmodified routine to that return, and verify the state
write. Let the original choose its seed from the clock; the two launches yield
different observed states. No guest-memory write, keyboard or mouse input is
used. The fixture records the seed for each case so the draw comparison is exact.

## Procedure

Continue to each native draw-entry breakpoint. Reject a seed, exit or diagnostic
stop in place of a draw. Read the raw draw's saved return, the inclusive helper's
saved return, its argument and the attribute offset. FND-PERSON-003 locates the
two calls at `0x00016248` and `0x00016254`: the first supplies 1 and the second
8. They return to `0x0001624D` and `0x00016259`. Match the expected pair for
each attribute index from 0 to 14 and tag it RULE-PERSON-002. Reject an unowned
caller or unexpected argument/index before executing the draw.

Step the raw function to its saved return within 32 instructions, allowing only
the RNG body and its state-pointer helper. Capture its state transition and raw
result. Step the inclusive helper to its saved return within 16 instructions,
capture the native bounded result, and check that it changed no RNG state.
Reject an interrupt or instruction path outside those functions. Check every
event against the previous state, recurrence, reduction and expected order before
continuing. On divergence keep the stopped memory and registers locally.

Stop after the second result for attribute 14 returns. This occurs before the
caller's final attribute store, so the experiment compares draws and RNG state,
not the completed character table or its shipped values.

The committed probe performs this as `tools/Probe-LiveMapping.py --samples 8
--observation-ms 1000 --rng-break --cycles 1000 --seed-check
--record-startup-shifts --output <fresh-local-directory>`, with GAME_DIR set for
the command to the owned installation. `tools/Export-RngFixture.py` exports only
the rule IDs, bounds/results, starting seeds and final global state. Original
diagnostics remain outside Git.

## Observations

Both cases recorded one initial seed and thirty native draws: fifteen with
inclusive bound 1 and fifteen with inclusive bound 8, alternating by attribute
index. All caller, index, argument, state and result checks passed. The cases
have different seeds, recorded individually in the fixture.

Local journal SHA-256 values are
`71e58b0107ad8b150a7297b9bee027f1cfb7ba055d852288e5ec6c7e40ecd75f`
and `d2710bfe19bc7fdc508e4ee4c28f49b9b3b5c65e6e5d6986916c81ab7d106d1c`.
The original store retains them as
`captures/EXP-RNG-001/startup-a-journal.json` and
`captures/EXP-RNG-001/startup-b-journal.json`, together with the native probe's
memory and register diagnostics. The fixture contains no code address, code,
resource string or character attribute value.

## Results

The exact comparison checks every bound and bounded result in order, using each
run's recorded seed and RULE-RNG-001. The final states are 326636863 and
3965157982. These are deterministic comparisons; no outcome distribution or
significance test is inferred from two cases.

## Conclusion

The bounded startup case corroborates the ordered shifts of RULE-PERSON-002 and
verifies rule-tagged native capture for those calls. It does not establish all
RNG callers, interrupt-driven recording, a campaign state, full-game recording,
or replay through a completed reimplementation. No rule or parity status rises.
