---
id: FND-UI-022
title: Youth Continue returns after replacing the screen or resetting the next dilemma's controls
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015054..0x000151F6
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

The Continue callback saves three registers, optionally calls the sound service
with the value at `0x0009A5BC`, and reads row zero's AGE through
`0x00015EF0(0, 18)`. FND-PERSON-004 records its age comparison and dilemma
selection. The age-18 path calls `0x0005B2C0`, stores minus one at
`0x0009AAC4`, calls `0x000596C0(6, 0, 1)`, drops those three arguments,
restores its saved registers and returns at `0x000150A7`.

The other path keeps the pointer-frame value returned by `0x00064008`,
selects pointer frame 1 through `0x00064030`, and calls `0x00018F54`.
It obtains the inclusive bound-4 selection described by FND-PERSON-004 and
calls `0x00018B60` with the resulting dilemma number and the seven arguments
recorded there. After calls that replace and draw the answer-picture object
and redraw the text and other presentation, it calls `0x00059D34(5, 0)`,
then that helper with indices 0, 1 and 2 and value 1. FND-UI-021 shows that
these indices select Continue and the three answers in file order.

It then calls `0x00063270(1, 0)`, stores zero at `0x000A7550`, calls
`0x00063F7C`, restores the saved pointer-frame value through `0x00064030`,
calls `0x00063F8C`, restores the three saved registers and returns at
`0x000151F5`. The inclusive RNG return inside this callback therefore
precedes the dilemma load and the control-reset calls.

## Interpretation

For another youth dilemma, the callback disables Continue and enables each
answer before its final return. That return is a distinct boundary from the
completed selection draw. At age 18, the other return follows the screen-6
replacement call. Both returns have restored the callback's entry stack depth.

## Alternatives

A completed selection draw alone does not show that the next dilemma has
loaded or its controls have reset: the listed calls occur afterward.
This bounded reading does not establish every presentation callee, the sound
service, the timer's progress, or physical input delivery. It does not raise
the youth screen or rule to established.

## How to reproduce

Verify the BLD-GOG-EN executable identity and decode the relocated code object
over the listed range. Track argument cleanup and the three saved registers
on each age branch. Compare the enable-helper calls with FND-UI-021 and the
selection arguments with FND-PERSON-004.
