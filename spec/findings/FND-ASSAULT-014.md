---
id: FND-ASSAULT-014
title: Two relocated tables give each mode its test and its handler
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F414..0x0004F49C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F644
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004F8AC
tool: Ghidra 12.1.3
environment: null
---

## Observation

Two 17-dword tables follow each other at `0x0004F414` (tests) and `0x0004F458` (handlers), one
entry for each mode 1 to 17. The fixups at sources `0x0004F644` and `0x0004F8AC` point into
them. Their entries:

| Mode | Test | Handler |
|---:|---|---|
| 1 | `0x0004F648` | `0x0004FDAB` |
| 2 | `0x0004F6F7` | `0x0004FDAB` |
| 3 | `0x0004FC34` | `0x0004FDAB` |
| 4 | `0x0004F98D` | `0x0004FDAB` |
| 5 | `0x0004FC34` | `0x0004FDCD` |
| 6 | `0x0004F98D` | `0x0004FDCD` |
| 7 | `0x0004F8B0` | `0x0004FE76` |
| 8 | `0x0004F7A2` | `0x0004FE76` |
| 9 | `0x0004FC34` | `0x00050020` |
| 10 | `0x0004F98D` | `0x00050020` |
| 11 | `0x0004F89B` | `0x0005010B` |
| 12 | `0x0004FD7B` | `0x0004FF53` |
| 13 | `0x0004FD9A` | `0x0005051A` |
| 14 | `0x0004F89B` | `0x000503EC` |
| 15 | `0x0004F89B` | `0x000504C0` |
| 16 | `0x0004F8B0` | `0x0004FE76` |
| 17 | `0x0004FB39` | `0x0004FDCD` |

Handler `0x0005051A` returns at once.

## Interpretation

Every mode has one test and one handler; the kind only changes the transition between them.

## Alternatives

None known.

## How to reproduce

Read the two tables at `0x0004F414` through the fixups whose sources are listed, as for
FND-ASSAULT-011.
