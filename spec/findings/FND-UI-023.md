---
id: FND-UI-023
title: The first youth-answer callback enables Continue before its restored-stack return
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014B38..0x00014CDE
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

The first-answer callback saves three registers and reserves four local bytes
at entry. FND-PERSON-005 records its answer scoring and modifier application.
After the call at `0x00014C5F`, its presentation tail exchanges picture index
4 with 5 and 5 with 4; every other index is kept. It calls `0x00065130`
with the object at `0x0009A5AC`, that index, y 310 and x 62.

The tail calls `0x00059D34(5, 1)` and then that helper with indices 0, 1
and 2 and value zero. FND-UI-021 records the file-order index interpretation:
this enables Continue and disables the three answer controls. It stores one
at `0x000A7550`, calls `0x00063270(1, 0)`, drops those arguments and the
four local bytes, restores the three saved registers, and returns at
`0x00014CDD`. This epilogue has the callback's entry stack depth.

## Interpretation

The callback return occurs after its control-state changes and redraw call.
The earlier bounded scoring reading alone did not locate this return.

## Alternatives

The picture-index exchange does not establish the pixels or semantic meaning
of the frames. This finding does not complete every callee's reading, physical
input dispatch, or timer behavior and does not establish the entire youth rule.

## How to reproduce

Verify the BLD-GOG-EN executable identity, relocate its LE code object and
decode the listed range. Follow entry reservations, each argument cleanup and
the final restored registers. Compare index selection with FND-UI-021 and
the earlier answer body with FND-PERSON-005.
