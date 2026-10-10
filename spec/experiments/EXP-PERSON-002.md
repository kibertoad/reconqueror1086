---
id: EXP-PERSON-002
title: A fresh youth traversal observes AGE 13 before its first answer and AGE 18 after five
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, pinned DOSBox-X Agent Debug No Heavy SDL2, normal core, fixed 100000 cycles, S3, 16 MB
starting_state: new-game
recording: null
repetitions: 1
fixture: EXP-PERSON-002.json
---

## Question

Does the fifth-answer discrepancy of Q-PERSON-021 arise from AGE already
exceeding the loaded value before the first watched answer, from an extra
answer, or from more than one AGE year added by an observed answer?

## Setup

Start BLD-GOG-EN with private writable C: and read-only owned media, holding
the machine run lock. Keep host sound muted and MOVIE, CREDITS and ANIMATIONS
off as FMT-CONFIG-001 permits. The pinned checkout revision is
b6abbd5980a885f5f310a4088c59a8688d1b116c; the emulator SHA-256 is
fa4dced1ed9bfa30a9a438905ba576c0ee21be8fb2671f9a77b681bfc7b23f4d.
These independently identify checkout and binary, without claiming their build
relationship. Lifecycle and supported writes use published session 0.2.0.
Observe the original's startup seed 895829666 without replacing it.

Follow the prescribed startup, title, new-game and generation short clicks,
then the first answer and Continue at each youth step. At the youth screen
return and every answer/Continue entry and return, read row zero's AGE through
the header, row-pointer list and attribute pointer of FND-PERSON-001/003.
Bound pointers and counts, require the same attribute pointer after the read,
and record AGE with current RNG state and completed Continue count.

## Procedure

Use the guarded native traversal with --youth-age-checkpoints. Verify the
relocated code/data descriptors, unchanged code, callback stack frames,
prescribed input order and each RNG state transition. Supply pointer payloads
and count only through FMT-BATTLE-002's supported contract, publishing count
last. Do not write AGE, calendar, timers, flags or RNG. Input readiness is an
observed state, not a fixed-time assumption.

The local run is native-rng-shared-youth-age-fixed100000-20261010-a under
artifacts/runtime-tools. The fixture retains only independently named inputs,
numeric draw facts and AGE observations. Original memory and diagnostics stay
local. The larger invocation requested six cycles and later dubbing controls;
that request is distinct from this experiment's AGE question.

## Observations

AGE was 13 at the first youth screen return and at the first answer entry.
The five prescribed answer returns observed 14, 15, 16, 17 and 18, with
the corresponding entry ages 13 through 17. Each observed answer added
exactly one; its entry and return RNG values agreed. The four completed
Continue returns retained their incoming AGE while selecting the next
dilemma. The fifth Continue entry saw AGE 18 and reached the dubbing entry;
no fifth Continue return was captured on this run.

The later traversal ended incomplete with a named-pipe timeout during a
diagnostic attempt at the second dubbing wait. Its pending screen, Continue
and entry fields remained true. Owned processes exited and the run lock was
absent. That failure does not invalidate the preceding recorded AGE samples,
but it supplies no terminal traversal or second-entry-input success.

## Results

The fixture compares the ordered AGE observations and numeric draw facts
exactly, ending the recorded AGE question at 18. Numeric RNG replay of the
completed prefix agrees with 1523890254. This is one controlled sequence,
not a random distribution or CPU-speed benchmark.

## Conclusion

The additional year precedes the first observed answer; it is not an extra
year inside one of these answers or an unwatched answer between them.
FND-PERSON-012 identifies the pre-input March producer, and EXP-PERSON-001
corroborates its AGE 12 to 13 case. Together they settle Q-PERSON-021's
initial-year discrepancy. They do not establish every AGE/calendar writer,
all input choices or rerolls, terminal dubbing traversal, full-game coverage
or actual rebuild replay.
