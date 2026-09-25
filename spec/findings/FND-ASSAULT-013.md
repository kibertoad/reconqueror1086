---
id: FND-ASSAULT-013
title: The kind-2 and kind-4 transition tables map each hostile mode to its next modes
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E4C4..0x0004E500
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E53C..0x0004E578
tool: Ghidra 12.1.3
environment: null
---

## Observation

Entries of the kind-2 table at `0x0004E4C4` (also used by kind 7) and the kind-4 table at
`0x0004E53C` that have been read:

| Mode | Kind 2 entry | Kind 2 | Kind 4 entry | Kind 4 |
|---:|---|---|---|---|
| 1 | `0x0004E88C` | 4 / 6 | `0x0004E6C0` | 4 / 3 |
| 2 | `0x0004E6D3` | 11 / 1 | `0x0004E983` | 11 / 4 |
| 3 | `0x0004E89F` | 7 / 1 | `0x0004E6E6` | 7 / 2 |
| 4 | `0x0004E8B2` | 8 / 2 | `0x0004E6F9` | 8 / 1 |
| 5 | `0x0004E70C` | 7 / 5 | `0x0004E70C` | 7 / 5 |
| 6 | `0x0004E71F` | 8 / 6 | `0x0004E71F` | 8 / 6 |
| 7 | `0x0004E8C5` | 1 / 6 | `0x0004E732` | 1 / 5 |
| 8 | `0x0004E745` | 11 / 6 | `0x0004E745` | 11 / 6 |
| 9 | `0x0004E8D8` | 6 / 1 | `0x0004E8D8` | 6 / 1 |
| 10 | `0x0004E6C0` | 4 / 3 | `0x0004E6C0` | 4 / 3 |
| 11 | `0x0004E77E` | 11 / 13 | `0x0004E77E` | 11 / 13 |
| 12 | not read | not read | not read | not read |
| 13 | `0x0004E851` | 6 / 10 | `0x0004E851` | 6 / 10 |
| 14 | `0x0004E77E` | 11 / 13 | `0x0004E77E` | 11 / 13 |
| 15 | `0x0004E77E` | 11 / 13 | `0x0004E77E` | 11 / 13 |
| 16 | not read | not read | not read | not read |
| 17 | not read | not read | not read | not read |

## Interpretation

The hostile kinds share acquisition in mode 6, pursuit in mode 8 and the strike in mode 11, and
differ in their formation and defence fallbacks (modes 1 to 4 and 7).

## Alternatives

None known.

## How to reproduce

Load the LE image with object 1 at `0x00010000`. The table dwords are relocated, so read each
entry through the fixup record whose source is the entry's address (for example the fixups at
sources `0x0004E6A7`, `0x0004E6BC`, `0x0004E801` and `0x0004E888` point into the tables), then
open the transition body the fixup targets. Each body is a few instructions long.
