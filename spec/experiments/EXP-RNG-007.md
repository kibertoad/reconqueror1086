---
id: EXP-RNG-007
title: Nonzero suffix-selector ordinals and signed ordering ignore year
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.9
starting_state: emulated-call
recording: null
repetitions: 24
fixture: EXP-RNG-007.json
---

## Question

Do FND-RNG-014's nonzero-selector path and two-record order helper return
the prescribed wrapping ordinals and signed comparison at integer edges,
independently of year?

## Setup

Load fingerprint-verified BLD-GOG-EN with LE relocations. Fabricate records
with offset-28 ordinal and offset-32 selector from FND-RNG-008; all other RAM
bytes are nonzero sentinels. Supply one record to the ordinal helper or two
to the order helper, plus year measured from 1900. Each case runs at year
zero and 124. Seven direct cases cover selector one, minus one and other
nonzero selectors, ordinal zero, INT32_MIN, INT32_MAX, 365 and 400. Five
comparisons cover equality, strict greater/less, selector-one wrapping and
signed edges. The fixture lists all records, years and expected returns.
These inputs are authored and not native parser output or valid-input claims.

Each call has fresh ordinary RAM and bounded stack. Authored flat 32-bit CS,
DS, ES and SS descriptors have base zero and 4 GiB limits, with a read-only
table. No OS selector model, stub, import/service/interrupt model, port model
or video RAM is supplied. There is no timer, input, window, audio or random
draw. Unexpected hardware/service accesses fail. Calls must return within
1,000 instructions.

## Procedure

Run tools/emu/verify_suffix_order.py with GAME_DIR containing
disc-root/CONQUER.EXE and an ignored report path. It skips without GAME_DIR
and rejects another executable identity. Compare unsigned returns and all
fabricated RAM bytes. Allow writes only to bounded stack; admit only the
nonzero-selector entry/tail and order helper from FND-RNG-014. A month/weekday
calculation or other helper path fails.

## Observations

All twenty-four calls passed. Selector one returned ordinal minus one modulo
2^32; other nonzero selectors returned the unchanged ordinal. In particular,
zero with selector one returned 4294967295 and INT32_MIN with selector one
returned 2147483647. The order helper returned one for first ordinal zero
versus selector-one ordinal zero, since the adjusted second quantity is minus
one. Equal adjusted values returned zero, including the wrapping INT32_MIN
versus unchanged INT32_MAX case. Unchanged INT32_MIN compared below one.
All paired years gave identical returns and all fabricated RAM was unchanged.

## Results

Exact records, years and unsigned returns are in the fixture. Full-RAM,
bounded-write, path and instruction-budget checks passed without stubs.
Only the nonzero-selector paths ran.

## Conclusion

The results corroborate FND-RNG-014's ordinal and signed-order reading.
They do not establish native input bounds, zero-selector month/weekday
calculation, leap handling elsewhere or complete nonempty-name classification.
Q-RNG-001 remains open and no rule or parity status changes.
