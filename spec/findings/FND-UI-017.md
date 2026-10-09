---
id: FND-UI-017
title: Initial screen loading stores a screen object and pushes its number into a five-slot history
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000595C0..0x00059660
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059CE4..0x00059D11
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00063D20..0x00063D3E
tool: Capstone 5.0.7, bounded 32-bit decoding of the fingerprinted LE code object
environment: null
---

## Observation

The entry at `0x000595C0` saves three registers and passes the address of
`0x000AFE50` and request value 24 to `0x00063D20`. That wrapper forwards
the request unchanged to `0x00073D6A`, stores its returned pointer through
the first argument, and returns 1 for a nonzero pointer or 0 otherwise.
The screen-loading caller drops that Boolean result without testing it.
This reading does not establish the allocator's units, header or capacity.

The caller writes -1 into five consecutive dwords at offsets 4, 8, 12,
16 and 20 of the record reached through `0x000AFE50`. Its screen, draw and
mode arguments are respectively at stack offsets 16, 20 and 24 after the
three saved registers. It passes the registered screen filename and mode
to the HAT loader of FND-UI-002, then stores that loader's returned object
pointer at record offset 0 before calling the registered setup routine.

If draw is nonzero, it draws the background. It then calls the object's
entry callback at offset 172 and passes the original screen argument to
`0x00059CE4`. This history routine copies record offset 16 to 20, 12 to
16, 8 to 12 and 4 to 8, in that order, and stores the argument at offset 4.
It writes the record pointer back to `0x000AFE50` before returning.

After that history call, a nonzero draw argument causes a call to
`0x00063270` with arguments 1 and 1. Both draw paths join at
`0x0005965C`, restore the three saved registers and reach the return
instruction at `0x0005965F`. The entry stop used in earlier probes
precedes all these writes and calls.

## Interpretation

The record separates the current screen object pointer from a five-slot
screen-number history. Its initial writes and the history push provide
specific storage to inspect after loading. Reaching the entry alone does
not show that an object was loaded or its setup and entry callback ran.

## Alternatives

This is a reading of direct stores and their order, not a complete reading
of the loader or its callbacks. A callback could replace the record or
object, change history or load another screen. Consequently the returned
record and object still need to be checked against the requested screen
in a controlled run; the direct writes do not prove their final identities.
Allocator failure is not guarded by the caller's Boolean test, because
that test is absent. No claim about recovery from that failure is made.

## How to reproduce

Verify BLD-GOG-EN source identity, apply its LE mapping, and decode the
three listed ranges as 32-bit code. Follow the stack through the three
saved registers and both argument cleanups; enumerate each iteration of
the initialization and history-copy loops. FND-UI-001 identifies the screen
registration and callbacks; FND-UI-002 records the returned object's layout.
