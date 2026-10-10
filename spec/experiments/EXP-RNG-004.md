---
id: EXP-RNG-004
title: Isolated zero-classification conversion reaches the 1970 epoch and exposes a post-century discrepancy
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.9
starting_state: emulated-call
recording: null
repetitions: 7
fixture: EXP-RNG-004.json
---

## Question

Under an explicitly absent environment array and classification zero, do the
converter's returned epoch quantity and normalized calendar agree with the
bounded reading in FND-RNG-010, including its pre-epoch adjustment edge and
four-year year-count term?

## Setup

Load the fingerprint-verified BLD-GOG-EN LE image with relocations. Pass a
pointer to a fabricated nine-word calendar record: second, minute, hour,
day, zero-based month, year minus 1900, weekday, year-day and classification.
Weekday and year-day start at distinct negative sentinel values; classification
is zero. The fixture lists every initial and expected word. The environment
array pointer is explicitly null, the first adjustment is the chosen fixture
value, and the secondary adjustment is zero. These are authored call inputs,
not observed startup defaults or values from a native session.

Each case has fresh ordinary RAM and a fresh bounded stack. Authored flat
thirty-two-bit CS, DS, ES and SS descriptors have base zero and 4 GiB limits;
their table is read-only. No OS descriptor table, import/service/interrupt
stub, port model or video RAM is supplied. There is no window, timer, input
or sound device. The converter uses its actual normalizer, leap predicate,
environment wrapper and null-array lookup path. Unexpected paths or hardware
accesses fail. Each call must return within 6,000 instructions.

## Procedure

Run tools/emu/verify_seed_calendar.py with GAME_DIR naming the owned evidence
directory containing disc-root/CONQUER.EXE and an ignored local report path.
It skips without GAME_DIR and refuses another executable identity. Compare
the full fabricated RAM region and unsigned thirty-two-bit result. Admit
writes only to the first eight calendar words and bounded stack; require
classification and all three prescribed globals to remain unchanged. Reject
instructions outside the actual converter and listed helper paths.
Compute an independent Gregorian reference with date/time subtraction,
without host time-zone conversion, and compare every accepted return with it.
The prescribed post-century difference is one day; the other accepted cases
have zero difference. The before-epoch rejection has no accepted difference.

## Observations

All cases passed. With adjustment zero, 1900-01-01 returned the minus-one
bit pattern after normalizing its record. 1970-01-01 returned zero, with
Thursday weekday four and year-day zero. Adjustment 18000 gave that return
while leaving the normalized local record unchanged. The last second of
1969-12-31 with adjustment 3600 returned 3599, also retaining its normalized
local calendar rather than writing the adjusted 1970 date.

The 2000 leap-day and 2100 March controls returned the epoch quantities in the
fixture. For 2101-01-01 the returned quantity was 4134067200 and the normalized
record became 2101-01-02, Sunday zero and year-day one. The independently
computed Gregorian quantity for the input date is 4133980800. The one-day
discrepancy agrees with FND-RNG-010's year-count reading.

## Results

Exact initial words, adjustments, unsigned return values and normalized
words are in the authored fixture. Full-region, global, instruction-path and
bounded-write checks passed without a stub.

## Conclusion

These isolated cases corroborate the bounded epoch/normalization reading.
They establish neither live date inputs nor actual startup adjustment values,
environment initialization, negative-classification behavior or the complete
calendar input domain. No full-game recording or rebuild replay ran; Q-RNG-001
remains open and no rule or parity status changes.
