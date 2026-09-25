---
id: FND-ASSAULT-012
title: The kind-0 and kind-1 transition tables map each friendly mode to its next modes
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E43C..0x0004E4C4
tool: Ghidra 12.1.3
environment: null
---

## Observation

Entries of the kind-0 table at `0x0004E43C` and the kind-1 table at `0x0004E480` that have been
read, each as the body it points at and its success/failure pair (FND-ASSAULT-010):

| Mode | Kind 0 entry | Kind 0 | Kind 1 entry | Kind 1 |
|---:|---|---|---|---|
| 1 | `0x0004E6C0` | 4 / 3 | not identified | 4 / 2 |
| 2 | not identified | 11 / 1 | not identified | 10 / 1 |
| 3 | `0x0004E6E6` | 7 / 2 | `0x0004E6E6` | 7 / 2 |
| 4 | `0x0004E6F9` | 8 / 1 | `0x0004E6D3` | 11 / 1 |
| 5 | `0x0004E70C` | 7 / 5 | `0x0004E70C` | 7 / 5 |
| 6 | `0x0004E71F` | 8 / 6 | `0x0004E745` | 11 / 6 |
| 7 | `0x0004E732` | 1 / 5 | not identified | 3 / 5 |
| 8 | `0x0004E745` | 11 / 6 | not read | not read |
| 9 | `0x0004E758` | 9 / 6 | `0x0004E758` | 9 / 6 |
| 10 | `0x0004E76B` | 5 / 2 | `0x0004E805` | 4 / 2 |
| 11 | not read | not read | not read | not read |
| 12 | not read | not read | not read | not read |
| 13 | `0x0004E7A4` | 2 / 10 | `0x0004E851` | 6 / 10 |
| 14 | `0x0004E77E` | 11 / 13 | not read | not read |
| 15 | `0x0004E77E` | 11 / 13 | not read | not read |
| 16 | `0x0004E7CA` | 17 / 17 | `0x0004E7CA` | 17 / 17 |
| 17 | `0x0004E7DD` | 16 / 17 | `0x0004E7DD` | 16 / 17 |

"Not identified" marks an entry whose pair was read from its body without recording the body's
address. The entries are at `0x0004E43C + 4 * (mode - 1)` and `0x0004E480 + 4 * (mode - 1)`;
for example mode 8 of kind 0 is at `0x0004E458` and mode 10 of kind 1 at `0x0004E4A4`.

## Interpretation

These pairs are the friendly state graph. Follow alternates between modes 16 and 17 whatever
the mode-16 test returns. Mode 13 is a health check that returns a friendly to mode 2 (kind 0)
or mode 6 (kind 1) or sends it to mode 10.

## Alternatives

None known.

## How to reproduce

Load the LE image with object 1 at `0x00010000`. The table dwords are relocated, so read each
entry through the fixup record whose source is the entry's address (for example the fixups at
sources `0x0004E6A7`, `0x0004E6BC`, `0x0004E801` and `0x0004E888` point into the tables), then
open the transition body the fixup targets. Each body is a few instructions long.
