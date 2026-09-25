---
id: FND-ASSAULT-010
title: Each transition body writes a failure mode and a success mode, and helper 0x4E5F0 moves one of them into the current mode
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E5F0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E6C0..0x0004E9C7
tool: Ghidra 12.1.3
environment: null
---

## Observation

The transition bodies between `0x0004E6C0` and `0x0004E9C7` each write two constants into the
combatant record: one to field `+0x20` and one to field `+0x1C`. The state routine passes the
Boolean result of the current mode's predicate to helper `0x0004E5F0`, which copies field
`+0x1C` into field `+0x18` when the result is true and field `+0x20` when it is false. When
the mode it installs changes the actor's state, the helper cancels the actor's live effect.

The bodies and the pair each writes (`+0x1C` on success / `+0x20` on failure):

| Body | Success | Failure |
|---|---:|---:|
| `0x0004E6C0` | 4 | 3 |
| `0x0004E6D3` | 11 | 1 |
| `0x0004E6E6` | 7 | 2 |
| `0x0004E6F9` | 8 | 1 |
| `0x0004E70C` | 7 | 5 |
| `0x0004E71F` | 8 | 6 |
| `0x0004E732` | 1 | 5 |
| `0x0004E745` | 11 | 6 |
| `0x0004E758` | 9 | 6 |
| `0x0004E76B` | 5 | 2 |
| `0x0004E77E` | 11 | 13 |
| `0x0004E7A4` | 2 | 10 |
| `0x0004E7CA` | 17 | 17 |
| `0x0004E7DD` | 16 | 17 |
| `0x0004E805` | 4 | 2 |
| `0x0004E851` | 6 | 10 |
| `0x0004E88C` | 4 | 6 |
| `0x0004E89F` | 7 | 1 |
| `0x0004E8B2` | 8 | 2 |
| `0x0004E8C5` | 1 | 6 |
| `0x0004E8D8` | 6 | 1 |
| `0x0004E983` | 11 | 4 |

## Interpretation

Field `+0x18` is the actor's current mode, `+0x1C` the mode it moves to when its test succeeds,
and `+0x20` the mode it moves to when the test fails. The retainer commands (FND-ASSAULT-008)
use `+0x1C` to hold the requested mode.

## Alternatives

The helper's cancellation was once read as the strike being rejected in mode 11. The
cancellation belongs to the helper and happens on any state change that ends the live effect
(FND-ASSAULT-024).

## How to reproduce

Load the LE image with object 1 at `0x00010000`. The table dwords are relocated, so read each
entry through the fixup record whose source is the entry's address (for example the fixups at
sources `0x0004E6A7`, `0x0004E6BC`, `0x0004E801` and `0x0004E888` point into the tables), then
open the transition body the fixup targets. Each body is a few instructions long.
