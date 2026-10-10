---
id: EXP-RNG-003
title: Isolated suffix parsing retains unselected fields and supplies default clock components
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.9
starting_state: emulated-call
recording: null
repetitions: 19
fixture: EXP-RNG-003.json
---

## Question

Do the suffix selector, optional components and partially retained output
record read in FND-RNG-008 produce the prescribed words and remaining source
positions for complete, incomplete and wrapping decimal inputs?

## Setup

Load the fingerprint-verified BLD-GOG-EN LE image with relocations. Pass a
fabricated null-terminated ASCII string and a separate nine-word record,
whose initial words are the distinct integers 101 through 109. Every case
uses fresh ordinary RAM and a fresh bounded stack. The CPU model supplies
authored flat thirty-two-bit CS, DS, ES and SS descriptors with base zero,
4 GiB limits and a read-only descriptor table. No original OS descriptor
table, environment or global rule initialization is reproduced.

No import, service or interrupt stub, port model, video RAM, window, timer,
input or sound device is provided. Any such access fails. The suffix parser
and its actual decimal helper must return within 3,000 instructions. There
is no RNG draw or seed to vary.

## Procedure

Run tools/emu/verify_seed_suffix.py with GAME_DIR naming the owned evidence
directory containing disc-root/CONQUER.EXE and an ignored local report path.
It skips without GAME_DIR and refuses another executable identity. Execute
the authored cases listed in the fixture. Compare the entire fabricated RAM
region, including unchanged source and record words, and the returned consumed
position. Admit writes only within the nine-word record and bounded stack;
reject instruction paths outside the suffix parser and decimal helper.

## Observations

All cases passed. Numeric and `J` forms stored selectors -1 and one and the
first numeric result at offset 28. `M` stored selector zero, its first number
minus one at offset 16, zero at offset 28, and optional dot components at
offsets 12 and 24. Missing dots retained those incoming fields; present empty
components stored zero. `JM` selected the `M` path. Offset 20 remained unchanged
in every case, as did other fields not written on the chosen path.

Without a slash, clock words were seconds zero, minutes zero and hours two.
A slash without digits changed hours to zero. Colon components and empty
components followed the digit-helper reading. A minus after the slash remained
unconsumed rather than becoming a signed hour. Oversized clock components and
decimal wrapping were accepted. Unrecognized prefixes also remained unconsumed
while the selector and default clock were still written.

## Results

The fixture records every initial and resulting word, consumed position and
remaining suffix. Full-region comparisons, instruction paths and bounded writes
passed without a stub.

## Conclusion

These controls corroborate the isolated parser's writes and returned pointer.
They do not establish the meanings of every output field, validity of original
environment values, wrapper initialization, calendar normalization or the full
seed source. Q-RNG-001 remains open and no rule or parity status changes.
