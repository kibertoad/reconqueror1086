---
id: EXP-STRATEGY-003
title: Eight isolated new-game setups retain selected-person group and coordinate results
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.9
starting_state: emulated-call
recording: null
repetitions: 8
fixture: EXP-STRATEGY-003.json
---

## Question

Does the whole new-game setup, including its actual group, rating and anchor
callees, retain all eight selected people, especially home index 7/person 28?
The static question and argument tracking are FND-STRATEGY-049, with the selector
and list handling in FND-STRATEGY-043 through FND-STRATEGY-048.

## Setup

Load a fresh verified BLD-GOG-EN relocated LE image per case. Preserve its
selection table and code, but fabricate the selected person's group, cell
coordinates, assignment, rating and list link as recorded in the fixture.
Start with an empty person list and null fief root. Set both coordinate
parameters `g_000AB0F8` and `g_000AB100` to 80. Choose the first nonnegative
seed below 65,536 producing each inclusive result 0 through 7. Start each
case from a fresh memory copy; there is no saved native state.

Call the actual setup and all its callees. There are no stubs, port models,
video mappings, timer, driver, window or input. The synthetic stack is ordinary
RAM; hardware/service accesses fail. The instruction bound is 2,000.

## Procedure

Confine every executed instruction to the setup and the actual selector,
initialization, RNG, list, group, rating and coordinate callee ranges in the
cited findings. Require exactly one raw RNG draw and its expected final state.
Check the selected person, place, fallback person and group, coordinate outputs,
rating, assignment, list link, integer destinations and floating positions.
Check the six player, five hostile and three brigand reset records and the
three brigand orders against FND-STRATEGY-045's stores. Reject every write outside
those fields, the unchanged roots/coordinate parameter and the synthetic stack.

Set GAME_DIR to the local evidence directory holding disc-root/CONQUER.EXE
and run tools/emu/verify_home_setup.py with --output naming an ignored report.
Without GAME_DIR it skips. This is an isolated call, not a native game launch.

## Observations

All eight cases returned with their expected group and coordinates, one RNG
draw, rating 12, cleared assignment and a single terminated person-list entry.
Index 7 retained person 28 through the whole setup. Both even and odd fabricated
columns produced the expected anchors and exact single-precision positions.
All prescribed reset values and the write whitelist passed.

The first harness attempt failed its write whitelist because the expected
set omitted the RNG storage and used loop bases before their initial increment.
The reset values checked in that attempt matched, but those omissions prevented
acceptance. Re-reading the add-before-store order and the existing record
locations corrected the whitelist; the accepted run covers the actual fields.

## Results

The immediate complete setup does not reject or replace the eighth home.
These cases corroborate the selected-person argument path in FND-STRATEGY-049.
Fabricated groups and coordinates do not identify the shipped person's actual
group, later hostile routing or every producer of the coordinate parameters.
Q-STRATEGY-043 remains open and RULE-STRATEGY-012 remains disputed.

## Conclusion

This experiment does not run the main loop, timers, resource loaders or later
map/route consumers. It does not establish every caller or native reachability
of fabricated state, and no complete-reading or gameplay-validation claim follows.

