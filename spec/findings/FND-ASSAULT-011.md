---
id: FND-ASSAULT-011
title: A dispatcher selects one of seven 17-entry transition tables by actor kind, and kind 7 uses the kind-2 table
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E41C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E43C..0x0004E5F0
tool: Ghidra 12.1.3
environment: null
---

## Observation

The dispatcher at `0x0004E41C` (object 1 offset `0x3E41C`) selects a transition table by actor
kind. The tables start at `0x0004E43C` (kind 0), `0x0004E480` (kind 1), `0x0004E4C4` (kind 2),
`0x0004E500` (kind 3), `0x0004E53C` (kind 4), `0x0004E578` (kind 5) and `0x0004E5B4` (kind 6).
Each holds 17 relocated dwords, one for each mode 1 to 17 in order, and each dword points at a
transition body (FND-ASSAULT-010). Kind 7 is sent to the kind-2 table.

For modes 9 and 10 the entries of all seven tables give these success/failure pairs:

| Kind | Mode 9 | Mode 10 |
|---:|---|---|
| 0 | 9 / 6 | 5 / 2 |
| 1 | 9 / 6 | 4 / 2 |
| 2 | 6 / 1 | 4 / 3 |
| 3 | 9 / 1 | 10 / 5 |
| 4 | 6 / 1 | 4 / 3 |
| 5 | 9 / 1 | 10 / 2 |
| 6 | 9 / 1 | 10 / 1 |

No entry in the kind 0, 1, 2 and 4 tables other than mode 9's own has 9 as its success or
failure mode.

## Interpretation

The actor's kind decides which transitions its modes take. For the kinds the shipped scenes use,
mode 9 can only be entered from mode 9.

## Alternatives

None known.

## How to reproduce

Load the LE image with object 1 at `0x00010000`. The table dwords are relocated, so read each
entry through the fixup record whose source is the entry's address (for example the fixups at
sources `0x0004E6A7`, `0x0004E6BC`, `0x0004E801` and `0x0004E888` point into the tables), then
open the transition body the fixup targets. Each body is a few instructions long.
