---
id: EXP-RNG-006
title: Empty-name classification clears negative input and controls secondary adjustment
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.9
starting_state: emulated-call
recording: null
repetitions: 7
fixture: EXP-RNG-006.json
---

## Question

Does FND-RNG-013's empty-name exit clear classification, and does conversion
distinguish negative, zero and positive inputs for its secondary adjustment?

## Setup

Load fingerprint-verified BLD-GOG-EN with LE relocations. Pass a fabricated
nine-word calendar record with FND-RNG-010's fields. g_000A70CA points to an
authored zero byte followed by nonzero RAM sentinels. The environment array
is null; first adjustment is 18000 and secondary adjustment 3600. These are
call inputs, not native startup observations. The fixture lists every word.
Direct classifier cases supply classification -2, -1, zero and one. Converter
cases supply -1, zero and one for 1970-01-01 midnight, with distinct negative
weekday/year-day sentinels.

Every case has fresh ordinary RAM and bounded stack. Authored flat 32-bit CS,
DS, ES and SS descriptors have base zero and 4 GiB limits, with a read-only
table; they are not original OS selectors. No stub, import/service/interrupt
model, port model or video RAM is supplied. There is no timer, input, window,
audio or random draw. Hardware/service accesses fail. Calls must return
within 6,000 instructions.

## Procedure

Run tools/emu/verify_empty_classification.py with GAME_DIR containing
disc-root/CONQUER.EXE and an ignored local report path. It skips without
GAME_DIR and rejects another executable identity. Compare unsigned results
and the entire fabricated RAM region; require all prescribed globals unchanged.
Direct calls may write only classification and stack. Converter calls may
write normalized calendar fields and, for negative input, classification,
plus stack. Admit only the converter/helper paths from EXP-RNG-004 and
FND-RNG-013's empty-name entry/exit. Other classifier paths fail.

## Observations

All seven cases passed. Every direct call returned zero and wrote classification
zero, preserving all other words and sentinels. Conversion normalized weekday
to Thursday four and year-day to zero. Negative classification became zero;
zero input remained zero. Both returned 18000. Positive input remained one and
returned 14400, subtracting the secondary adjustment. Prescribed globals and
the empty name's surrounding bytes remained unchanged.

## Results

The fixture records initial/final words and exact unsigned returns. Full-RAM,
global, path, bounded-write and instruction-budget checks passed without stubs.
No nonempty-name classification branch executed.

## Conclusion

The results corroborate the empty-name exit and stored-field decision in
FND-RNG-013. An empty name does not suppress secondary subtraction when positive
classification is already supplied. Startup inputs, nonempty-name rules,
selector provenance and the broader converter domain remain unresolved under
Q-RNG-001. No rule or parity status changes.
