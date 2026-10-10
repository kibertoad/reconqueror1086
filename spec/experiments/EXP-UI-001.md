---
id: EXP-UI-001
title: Two controlled short clicks let the animations-disabled dubbing entry return
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, pinned DOSBox-X Agent Debug No Heavy SDL2, normal core, fixed 10000 cycles, S3, 16 MB
starting_state: new-game
recording: null
repetitions: 1
fixture: EXP-UI-001.json
---

## Question

Can ordinary short primary input at each of FND-UI-026's two waits let the
animations-disabled dubbing entry finish, rather than waiting for its
return before providing input?

## Setup

Start BLD-GOG-EN from a private writable copy with read-only owned disc
media. Set MOVIE, CREDITS and ANIMATIONS off as FMT-CONFIG-001 permits.
Keep the configured guest sound driver but mute host output. Acquire the
machine run lock. The emulator's pinned source revision is
b6abbd5980a885f5f310a4088c59a8688d1b116c; the executable SHA-256 is
fa4dced1ed9bfa30a9a438905ba576c0ee21be8fb2671f9a77b681bfc7b23f4d.
These independently identify checkout and binary, without proving how that
binary was built. The owned lifecycle and supported-field writer are the
published dinorefurb-dosbox-session 0.2.0 release.

Observe the game's own startup seed, 895829156; do not replace it. Record
every accepted RNG draw and its rule, bound and result. Reach the title,
new-game menu, character options and youth screen with the fixture's
prescribed short clicks. Select the first answer and then Continue at each
prescribed youth step. The recorded run reached dubbing after the fifth
pair, which is a separate unresolved traversal-count observation.

## Procedure

Verify the relocated code/data descriptors and original code throughout.
At each entry wait verify the active screen-6 request, entry stack depth,
completed RNG state and pointer classification readiness. Queue exactly one
short primary click at (10, 10) per wait using FMT-BATTLE-002's documented
payload fields and count; leave reserved fields, timing globals, RNG and
other state untouched. Publish the count last with expected hashes and
readback verification. Repeat readiness observations without a fixed-time
input policy. Require a return value of one and an empty queue at each
accepted-input boundary, then restored entry stack depth at final return.

The local scripted invocation was tools/Probe-LiveMapping.py with normal
core, --cycles 10000, --record-native, --animations-off, guarded startup/menu
and youth inputs, --youth-cycles 6, --dubbing-click and --dubbing-entry-input.
Its output directory was native-rng-shared-dubbing-entry-fixed10000-20261010-a
under artifacts/runtime-tools. Original captures remain local and are not
part of the fixture.

## Observations

Both entry waits accepted the prescribed input, both queues drained, and
the final entry and loaded-screen returns passed their stack and RNG guards.
The loaded screen identity and first history element were both 6. The
fixture records the actual seed, draw sequence, input order and final RNG
state. No screenshot, sound output or pixel observation was made.

The larger traversal ended incomplete because its fifth Continue returned
after dubbing replacement while the diagnostic required six completed
cycles. The later full-screen dubbing callback was not exercised. All
owned processes exited and the machine run lock was absent after cleanup.

## Results

The entry returned after its two controlled inputs with RNG state
3735047536. No RNG draw occurred between entry and its final return on this
run. The complete local journal had 37 seed/draw events and its numeric
replay agreed with that state. The fixture is an exact ordered comparison;
it is not a distribution or a timing benchmark.

## Conclusion

This run corroborates the two entry waits and restored return boundary of
FND-UI-026. Input scheduled only after loader return cannot satisfy those
waits. It does not establish rendered pixels, audible output, every input
path, six-cycle youth completion, the later dubbing click, full-game caller
coverage or replay against the rebuild. The traversal-count discrepancy
must be investigated independently rather than relaxing its contract.
