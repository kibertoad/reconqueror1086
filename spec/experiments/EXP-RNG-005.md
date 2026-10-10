---
id: EXP-RNG-005
title: Isolated initializer dispatch selects priorities and updates completion markers
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.9
starting_state: emulated-call
recording: null
repetitions: 6
fixture: EXP-RNG-005.json
---

## Question

Does the initializer dispatcher from FND-RNG-012 select the smallest eligible
priority, choose the last row in ties, skip completed rows and mark null-target
rows completed without calling them?

## Setup

Load fingerprint-verified BLD-GOG-EN with its LE internal relocations. Replace
only the eight table rows described by FND-RNG-012 with authored marker,
priority and callback-presence inputs from the fixture. Each present callback
points to a distinct authored immediate-return stub, preserving all registers,
segments and memory. None performs initialization or other game behavior.
The shipped-priorities case retains the recorded priority pattern while
substituting these callback stubs. It does not run the shipped callbacks.

An authored wrapper supplies the dispatch threshold and invokes the unchanged
original dispatcher. Each case has fresh bounded stack and authored executable
RAM. Flat thirty-two-bit CS, DS, ES and SS descriptors have base zero and
4 GiB limits, with a read-only descriptor table. These descriptors are not the
original operating system's selectors. No import, interrupt, service or port
model and no video RAM is supplied. Unexpected hardware or service access
fails. There is no timer, input, window or audio; no random draws occur.
Each wrapper and dispatch must return within 4,000 instructions.

## Procedure

Run tools/emu/verify_initializer_dispatch.py with GAME_DIR naming the owned
directory containing disc-root/CONQUER.EXE and a local ignored output path.
The procedure skips without GAME_DIR and rejects another executable identity.
Compare the ordered stub-entry trace and all forty-eight final table bytes
with the fixture. Require the authored executable region to remain unchanged.
Allow writes only to the eight marker bytes and bounded stack, and executed
instructions only in the dispatcher or authored wrapper/stubs. No original
instruction or original callback is replaced or admitted as a stub.

## Observations

All six cases passed. With the shipped priorities and fresh markers, callback
row order was 1, 2, 7, 6, 5, 4, 3, 0, using zero-based row indices. With
threshold zero and priorities one, no callback ran and no marker changed.
Completed markers also yielded no calls. Equal-priority rows with markers
zero, one or three remained eligible and ran in reverse table order; marker
two rows were skipped. A null target became completed without a callback.
The mixed threshold-two case ran only priorities zero, one and two, each
group in reverse table order. Higher priorities remained unchanged.

## Results

The fixture records every initial row, threshold, exact callback order and
final marker. All full-table comparisons, executable-region checks, bounded
write guards, path guards and instruction budgets passed. The callbacks are
explicit preserving stubs, not evidence that actual callbacks preserve state.

## Conclusion

The controls agree with the bounded selection and marker reading in
FND-RNG-012. They do not establish native startup completion, source-selector
validity, actual callback effects, environment contents or the seed converter's
full provenance. Q-RNG-001 remains open; no rule or parity status changes.
