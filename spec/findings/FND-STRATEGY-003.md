---
id: FND-STRATEGY-003
title: The hostile pass at 0x0003C088 generates first, then moves slots 0 to 4 and resolves each arrival
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003C088..0x0003C28E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000437CC..0x000437EB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000437EC..0x0004381D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043868..0x0004389B
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003C088` calls `0x0003BE58`, then visits the records at `0x000AAB48 + 0x118 * i` for `i`
from 0 to 4. For a record whose dword `+0x00` is 1 it clears a local flag and calls, by the dword
`+0x34`, `0x00038D78(i, &flag)` for 1, `0x0003908C(i, &flag)` for 3, and
`0x0004A300(record, &flag)` otherwise. When the flag is then not 0 and `+0x00` is still 1, it
truncates the floats `+0x5C` and `+0x60`, projects them through `0x000629B0` into the dwords
`+0x44` and `+0x48`, and asks `0x000437CC` for a person at `(x, y)`, `(x, y - 2)`, `(x + 1, y)`,
`(x - 1, y - 1)` and `(x, y + 1)` of that cell, taking the first answer that is not 0.

`0x000437CC(x, y)` returns the value `0x0003E708` gives for the cell when it is 1 to 176, and 0
otherwise. `0x00043868(p, &a)` returns 0 for `p` 0 or above 176 and leaves `a` alone; otherwise it
stores the byte at `0x0009BA57 + 18 * p` in `a` and returns 1. `0x000437EC(p)` returns 0 for `p` 0
or above 176, and otherwise bit 0 of the byte at `0x0009BA56 + 18 * p`.

When `0x00043868` returns 1, the person is not 0 and its byte is 0, and `0x000437EC` returns 1, the
pass calls `0x00039900(i, p)`, and in that case, or when the person's flag bit is clear, returns
when the dword at `0x0009ADC0` is 1. Next, when the byte at `0x0009B8EC + 15 * o` for the origin
`o` in `+0x28` equals the person's byte and the person is not 0, it adds the dwords `+0x1C`, `+0x20`
and `+0x24` to the byte at `0x0009B8F8 + 15 * o` and calls `0x0003A6A0(i)`; otherwise it calls
`0x0003A9BC(i)`. The local is not written when `0x00043868` returns 0.

## Interpretation

The three handlers move one record each and set the flag when it has arrived or stopped. At
arrival the pass looks for a person in the cells around the record. A person with assignment 0 and
flag bit 0 set is met through `0x00039900`. A person whose assignment matches the origin's state
byte means the force has come home: its troops join the origin's garrison and the record is
destroyed. Any other arrival, including none, retargets the force. The local assignment is only
compared when a person was found, so its stale value never decides anything.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003C088..0x0003C28E`, `0x000437CC..0x000437EB`, `0x000437EC..0x0004381D`, `0x00043868..0x0004389B` in `CD:CONQUER.EXE`.
