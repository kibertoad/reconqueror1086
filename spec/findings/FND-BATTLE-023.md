---
id: FND-BATTLE-023
title: Pointer events are queued with a timer count and classified into eight codes
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008497C..0x00084A4B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00084A4C..0x00084A61
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00084887..0x000848A5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063098..0x00063102
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063114..0x00063267
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006306C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00069AAC..0x00069C05
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006A025..0x0006A0EB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00072329..0x00072332
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008492C..0x00084935
tool: Capstone 5.0.7, bounded 32-bit decoding of the fingerprinted LE code object
environment: null
---

## Observation

The mouse callback `0x0008497C` masks the event bits with `0x1E`, takes each set bit from low to high,
and maps bits 1 to 4 to kinds 0 to 3 (primary down and up, secondary down and up). It appends at most
30 entries of 20 bytes at `0x000B0530`: the count at `0x000A6830`, x, y and the kind at offsets 0, 4, 8
and `0x0C`. `0x00084A4C` adds 1 to that count on each INT 1Ch before chaining. On registering the
250 Hz callback, the timer service computes the BIOS chaining step as
`0x123333 / (0x1234DC / pit_divisor)`, with unsigned integer division at
each step. With `pit_divisor = 0x1234DC / 250 = 4772`, the inner quotient
is 250 and the step is 4771. The dispatcher adds the step to its
accumulator, tests bit 0 of its high word, clears that high word when
set, and calls the registered far callback. This is not a subtraction of
65536 on arbitrary accumulator values. `0x00063098` removes the first entry and lowers the
count at `0x0009DF38`; with the count at 0 it returns 0. `0x00063114` pops one entry, returns 0 when
there is none or its kind is above 3, and otherwise returns its x, y and time and a code: kinds 0 and 2
store the time at `0x0009DF3C`/`0x0009DF40` and give 1 and 5; kinds 1 and 3 give 2 and 6 when the
unsigned hold is at least `0x0009DF4C`, storing 0 at `0x0009DF44`/`0x0009DF48`, and otherwise 4 and 8
when the unsigned time since the stored release is strictly below `0x0009DF50`, storing 0 there, or 3
and 7, storing the release time there. The pointer dispatcher calls it once a pass, as recorded in FND-UI-008.
Both limits are set to 4 through `0x0006306C`.

## Interpretation

Codes 2 and 6 are long presses, 3 and 7 clicks, and 4 and 8 double clicks. The timestamps count BIOS
ticks at about 18.2 Hz.

## Alternatives

This supersedes FND-BATTLE-020 in full. The earlier finding located the
pointer timestamp counter at `0x000B6830`; both the mouse callback load
and INT 1Ch increment instead use `0x000A6830` after the DS setup helper
of FND-RNG-004. The earlier formula divided the chaining numerator
directly by 250; the executable first derives a frequency from the PIT
divisor. The verified step remains 4771 for the requested 250 Hz case.
The classifier return epilogue is now included in the exclusive range.
The bounded readings retain the queue and classification observations;
this is not a complete reading of timer registration or input dispatch.

## How to reproduce

Disassemble `0x0008497C..0x00084A4B`, `0x00084A4C`, `0x00084887..0x000848A5`, `0x00063098..0x00063102`, `0x00063114..0x00063267`, `0x0006306C`, `0x00069AAC..0x00069C05`, `0x0006A025..0x0006A0EB`, `0x00072329..0x00072332`, `0x0008492C..0x00084935`.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.
