---
id: EXP-UI-002
title: Five AGE-checked youth pairs and both dubbing waits precede the automatic update callback
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, pinned DOSBox-X Agent Debug No Heavy SDL2, normal core, fixed 10000 cycles, S3, 16 MB
starting_state: new-game
recording: null
repetitions: 1
fixture: EXP-UI-002.json
---

## Question

Does the observed fresh-game AGE trajectory reach dubbing after five
answer/Continue pairs, and which path invokes the village transition after
both entry waits return? The relevant readings are RULE-PERSON-004,
RULE-PERSON-007, FND-UI-024, FND-UI-026 and FND-UI-027.

## Setup

Start verified BLD-GOG-EN from a private writable copy with the owned disc
read-only. Set MOVIE, CREDITS and ANIMATIONS off, preserve the guest sound
driver and mute host output. Hold the machine run lock. The pinned emulator
source is b6abbd5980a885f5f310a4088c59a8688d1b116c and its executable
SHA-256 is fa4dced1ed9bfa30a9a438905ba576c0ee21be8fb2671f9a77b681bfc7b23f4d.
Use the published session 0.3.0 lifecycle and guarded field writer.
Observe the game's seed 895832521 without replacing it.

## Procedure

Follow the fixture's short clicks through preparation, title, new-game menu,
character options, five first-answer/Continue pairs and both dubbing entry
waits. Input follows observed readiness, never a fixed-time schedule.
Verify relocated code, descriptors, native RNG draws and each callback's
entry/return stack throughout. Read row-zero AGE at the initial youth-screen
return and at each answer and Continue entry/return. At each dubbing wait,
require accepted input and a drained queue, then restored entry stack.

The local invocation uses tools/Probe-LiveMapping.py, fixed 10000 cycles,
--record-native, --animations-off, guarded startup/menu input,
--youth-cycles 5, --youth-age-checkpoints, --dubbing-entry-input and
--dubbing-click. The local directory is
native-rng-youth-v4-fixed10000-20261010-a under artifacts/runtime-tools.
Capture bytes remain local; the fixture contains authored event labels,
input coordinates and compact numeric observations only.

## Observations

The initial youth-screen AGE was 13. The five answers returned at 14, 15,
16, 17 and 18; every Continue preserved its entry AGE. The fifth Continue
returned after verified screen-6 identity and history. Both dubbing entry
waits accepted their prescribed clicks and drained the queue; entry returned
with its restored stack. No extra click for the village transition was queued.

The next transition entry was `0x00019C80`. Its saved caller return was
`0x00059BCD`, independently normalized with this run's verified code mapping.
FND-UI-027 locates that return after the per-screen update call. The recorder
refused this callback because its old guard required a separately queued
click. It stopped before following the callback or village replacement.

All pending operation flags were false in the resulting incomplete journal.
The shared log's failure outcome agreed with the ordered numeric events,
AGE-observation digest and terminal journal. Owned processes exited and the
machine lock was absent after cleanup.

## Results

The completed prefix has one seed and 36 draws, with numeric replay ending
at 683206245. The fixture records all 21 AGE observations and both entry
inputs in order. These facts corroborate the five-pair trajectory and the
registered update call reaching transition entry; the broader traversal
remains incomplete and no village callback return was observed.

## Conclusion

The separate-click-only guard omitted a directly evidenced original caller.
Correct the recorder from FND-UI-027 and repeat a fresh run to verify the
callback and village return. This run does not establish full-game coverage,
rendered appearance, sound output, timing distributions or actual rebuild replay.
