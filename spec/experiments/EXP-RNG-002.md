---
id: EXP-RNG-002
title: Isolated environment adjustment parsing preserves missing values and wraps numeric components
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11, Unicorn 2.1.4 x86-32, Capstone 5.0.9
starting_state: emulated-call
recording: null
repetitions: 24
fixture: EXP-RNG-002.json
---

## Question

Do the name, sign, numeric-component and returned-pointer paths read in
FND-RNG-007 produce the described outputs on fabricated strings, including
missing numbers, clipped names and arithmetic overflow?

## Setup

Load the fingerprint-verified BLD-GOG-EN LE image and its relocations. Supply
three arguments: a fabricated null-terminated ASCII string, a separate
thirty-two-byte name destination initially filled with question marks, and
a separate adjustment word initially 123456. Each case has fresh ordinary
RAM and a fresh bounded call stack. The CPU model explicitly supplies authored
flat thirty-two-bit descriptors for CS, DS, ES and SS; their base is zero,
their limit is 4 GiB and their descriptor table is read-only. This does not
reproduce the original OS descriptor table.

There are no import, service or interrupt stubs, port models, video RAM,
window, timer, input or audio devices. Any such access stops the call.
The parser and its actual decimal helper must return within 3,000 instructions.
There is no seed input or RNG draw to vary.

## Procedure

Run tools/emu/verify_seed_adjustment.py with GAME_DIR naming the owned evidence
directory containing disc-root/CONQUER.EXE and an ignored local output path.
It skips without GAME_DIR and rejects another source identity. Use the
independently authored inputs and expected outputs listed in the fixture.
Compare the entire fabricated RAM region, including source bytes and name
destination tails. Require the returned pointer to identify the prescribed
remaining suffix. Admit writes only to the terminated name, the adjustment
word and the bounded call stack, and instruction execution only within
the two routines read in FND-RNG-007.

## Observations

All cases passed. Empty, name-only and sign-without-number values left the
initial adjustment unchanged. A leading colon was skipped, whereas an interior
colon was copied as a name byte. Positive and negative adjustments followed
the component arithmetic. Empty minute fields remained zero, including the
case followed by a seconds component. Minute and second values of ninety
were accepted without a range error.

Names at and beyond thirty bytes produced a thirty-byte terminated output;
the returned pointer still reflected all consumed input. Short and longer
names covered zero and nonzero dword counts and byte tails. Decimal and
subsequent component arithmetic wrapped at thirty-two bits. The fixture
records exact adjustment values, consumed positions and remaining suffixes.
Full-region comparisons and write/path restrictions passed without a stub.

## Results

The copied names, output values and returned positions agree with the helper
reading in FND-RNG-007, including the repeat-prefixed copy's observed behavior
in this emulator model. No environment lookup or live original session ran.

## Conclusion

These controls corroborate the isolated parser reading. They do not establish
the original operating system's segment state, processor behavior for every
repeat-prefix form, environment initialization, suffix-rule semantics,
calendar normalization or the complete seed-source conversion. Q-RNG-001
remains open, and no rule or parity status is promoted.
