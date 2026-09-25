---
id: FND-BATTLE-020
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
    address: 0x00084A4C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00084887..0x000848A5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063098..0x00063101
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063114..0x0006325E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006306C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00069BDD..0x00069C03
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00072329..0x00072332
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0008492C..0x00084935
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The mouse callback `0x0008497C` masks the event bits with `0x1E`, takes each set bit from low to high,
and maps bits 1 to 4 to kinds 0 to 3 (primary down and up, secondary down and up). It appends at most
30 entries of 20 bytes at `0x000B0530`: the count at `0x000B6830`, x, y and the kind at offsets 0, 4, 8
and `0x0C`. `0x00084A4C` adds 1 to that count on each INT 1Ch before chaining. On registering the
250 Hz callback, the timer service keeps a 16.16 accumulator that adds `0x123333 / 250 = 4771` and
chains the old IRQ 0 when it passes `0x10000`. `0x00063098` removes the first entry and lowers the
count at `0x0009DF38`; with the count at 0 it returns 0. `0x00063114` pops one entry, returns 0 when
there is none or its kind is above 3, and otherwise returns its x, y and time and a code: kinds 0 and 2
store the time at `0x0009DF3C`/`0x0009DF40` and give 1 and 5; kinds 1 and 3 give 2 and 6 when the
unsigned hold is at least `0x0009DF4C`, storing 0 at `0x0009DF44`/`0x0009DF48`, and otherwise 4 and 8
when the unsigned time since the stored release is strictly below `0x0009DF50`, storing 0 there, or 3
and 7, storing the release time there. The pointer dispatcher calls it once a pass.
Both limits are set to 4 through `0x0006306C`.

## Interpretation

Codes 2 and 6 are long presses, 3 and 7 clicks, and 4 and 8 double clicks. The timestamps count BIOS
ticks at about 18.2 Hz.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0008497C..0x00084A4B`, `0x00084A4C`, `0x00084887..0x000848A5`, `0x00063098..0x00063101`, `0x00063114..0x0006325E`, `0x0006306C`, `0x00069BDD..0x00069C03`, `0x00072329..0x00072332`, `0x0008492C..0x00084935`.
