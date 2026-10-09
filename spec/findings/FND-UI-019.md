---
id: FND-UI-019
title: Startup preparation waits on the battle clock and lets a click or key end the wait
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002AC50..0x0002AC64
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B790..0x0005B7BD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005BB34..0x0005BB49
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024D14..0x00024D54
tool: Capstone 5.0.7, bounded 32-bit decoding of the fingerprinted LE code object
environment: null
---

## Observation

The preparation caller of FND-UI-018 passes duration 6144 and callback
`0x0005BB34` to the wait helper at `0x0005B790`, returning to
`0x0002AC61`. The helper preserves ESI and reads the clock through
`0x00018420` (FND-BATTLE-021). It forms a deadline by adding the duration
to that first clock value in 32 bits. Each pass reads the clock again:
an unsigned value strictly above the deadline returns 1. Equality still
takes the waiting path. With a null callback that path immediately
repeats. Otherwise it calls the supplied callback and repeats when the
callback returns nonzero; zero returns zero from the helper.

The supplied callback invokes `0x00024D14` and returns 1 exactly when
that input poll returned zero. Thus accepted input returns zero from
the callback and ends the wait. The caller drops the wait result and
continues the remaining preparation before its animation test.

The input poll pops an event through `0x00063114`. Codes 3 or 7 accept
input immediately; otherwise a nonzero keyboard availability result
also accepts input. The accepted path clears keyboard input and the
pointer queue, then returns 1. Other paths return zero. FND-BATTLE-023
identifies codes 3 and 7 as short primary and secondary releases that
are not classified as double clicks; down events alone are not accepted.

## Interpretation

The timed preparation can be ended through a normal short click. A
controlled queue input must preserve the event layout and classification
timing of FMT-BATTLE-002 and RULE-BATTLE-012. Bypassing this wait does not
bypass the remaining preparation or establish screen readiness.

The clock reader advances in steps of four. Without accepted input, the
strict comparison requires a value above the computed deadline, rather
than equal to it. This is a code comparison, not a measurement of wall
time or hardware pacing. Addition can wrap, and this reading does not
establish all other callers' duration bounds or wrap handling.

## Alternatives

A queued down event alone cannot end this wait through the pointer path.
A release classified as a double click is also not one of the accepted
pointer codes. The supplied callback reverses the input poll's Boolean;
its nonzero result continues, rather than ends, the wait.

## How to reproduce

Verify BLD-GOG-EN identity and decode the listed ranges. Follow the two
arguments and return into the caller, ESI through deadline formation and
both loop exits, then the callback's Boolean inversion and the input
poll's two accepted pointer codes. Use FND-BATTLE-021 for the clock reader
and FND-BATTLE-023 for event classification. No complete reading of
hardware timing, keyboard services or other wait callers is claimed.
