---
id: FND-ASSAULT-009
title: The template initializer gives the ten combatant templates their kinds and starting modes
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000542F8
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x000542F8` initialises combatant records 0 to 9. It gives templates 0 to 9 the actor
kinds 0, 0, 1, 2, 3, 4, 5, 6, 2, 7, and writes fields `+0x18`, `+0x1C` and `+0x20` as:

| Template | Kind | `+0x18` | `+0x1C` | `+0x20` |
|---:|---:|---:|---:|---:|
| 0 | 0 | 4 | 4 | 4 |
| 1 | 0 | 4 | 4 | 4 |
| 2 | 1 | 4 | 4 | 4 |
| 3 | 2 | 6 | 8 | 6 |
| 4 | 3 | 4 | 10 | 6 |
| 5 | 4 | 4 | 8 | 1 |
| 6 | 5 | 1 | 4 | 2 |
| 7 | 6 | 4 | 10 | 1 |
| 8 | 2 | 6 | 8 | 4 |
| 9 | 7 | 4 | 8 | 4 |

A cloned combatant starts with its template's values.

## Interpretation

Friendly templates use kinds 0 and 1; the placed hostile templates use kinds 2, 4, 2 and 7. No
placed template (FND-ASSAULT-002) starts in mode 9 or 10.

## Alternatives

None known.

## How to reproduce

Open `0x000542F8`; the ten records are written one after another.
