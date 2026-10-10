---
id: FND-UI-024
title: Dubbing's full-screen callback clears its stored sound value and returns after village replacement
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019C80..0x00019CB2
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image
environment: null
---

## Observation

FND-UI-013 associates the full-screen region of screen 6 with the callback
at `0x00019C80`. It reads the four-byte value at `0x0009A708` through DS.
If that value is zero, it skips the call at `0x00019C94`. Otherwise it passes
that value, 4, 0 and `0x7FFF` to `0x0005B3B0`, then drops all four arguments.
FND-SOUND-003 records that callee's sample-playback behavior.

Both paths then pass 11, 0 and 1 to `0x000596C0`. Before that call, the
callback stores a four-byte zero through DS at `0x0009A708`. The call is
at `0x00019CA9`; its three arguments are dropped at `0x00019CAE`, followed
by the return at `0x00019CB1`. No register saves or local reservation change
the callback's stack depth. On either path, the return is at the same stack
depth as the entry. FND-UI-013 identifies screen 11 as the village exterior;
FND-UI-020 records the screen-replacement procedure's return boundary.

## Interpretation

The full-screen dubbing click optionally requests sample playback, clears
the stored handle and replaces the current screen with the village exterior.
The callback return follows the replacement call and its argument cleanup.
Its optional sample-playback result and replacement result are not tested by
this callback; neither branches its return path.

## Alternatives

The zero store does not identify how the stored value was created or which
sample it represents. This bounded callback reading does not establish all
screen-entry presentation, sound output, timer progress or physical input
delivery. Q-UI-026 retains screen-entry presentation; Q-UI-044 tracks the
sample's identity and stored value's origin.

## How to reproduce

Verify the BLD-GOG-EN executable identity, relocate its LE code object and
decode the listed half-open range in x86-32 mode. Follow the zero/nonzero
branch, DS-relative accesses and each argument cleanup to the shared return.
Compare the region binding and screen request with FND-UI-013, the sample
callee with FND-SOUND-003 and replacement boundaries with FND-UI-020.
