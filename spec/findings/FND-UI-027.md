---
id: FND-UI-027
title: Dubbing binds its village transition to the per-screen update as well as the click region
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019A50..0x00019A89
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059C24..0x00059C36
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059BC0..0x00059BD4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019C80..0x00019CB2
tool: Capstone 5.0.9 over the fingerprint-verified relocated LE code object
environment: null
---

## Observation

The screen-6 setup first binds the entry procedure of FND-UI-026 through
`0x00059C10`. It then pushes `0x00019C80` and calls `0x00059C24`.
That setter reads the current screen record through `0x000AFE50`, dereferences
its object pointer, and writes the argument dword at object offset 176.
The setup also binds the cleanup procedure of FND-UI-026 through
`0x00059C38`, then passes region index 0, callback slot 2 and the same
`0x00019C80` to the region callback setter of FND-UI-002.
All these arguments are cleaned up before the setup returns.

At the end of the input dispatcher, `0x00059BC0` reloads the current screen
record and its object. The indirect call at `0x00059BC7` calls the pointer
at object offset 176. Its following instruction is `0x00059BCD`.
This call is separate from the preceding region-input scans. The listed
call sequence supplies no argument and performs no null-pointer test.
This observation does not identify every path that reaches that sequence.

Both bindings therefore name the same transition procedure described by
FND-UI-024: optionally request the stored sound bank's sample, clear that
stored value, request screen 11, and return after replacement cleanup.
A separate pointer click is not a prerequisite for the per-screen update
call on the observed path. FND-UI-001 identifies how setup installs these
object callbacks, but left this update slot's invocation unread.

## Interpretation

After dubbing's entry waits finish, its per-screen update can transition to
the village without a new click. Treating the full-screen region as the only
caller excludes the actual registered update path. EXP-UI-002 corroborates
entry to that path after the observed five youth answer/Continue pairs.

## Alternatives

Leftover pointer input is not required to explain the observed callback:
the recorded caller returns to the direct object-update call's following
instruction, rather than a region callback call. The two entry waits each
confirmed their queue was drained. This does not establish every hardware
input source, every dispatcher branch, rendered pixels, sound output or the
observed village return; the controlled run stopped at transition entry.

## How to reproduce

Verify BLD-GOG-EN, relocate its LE objects and decode the listed ranges.
Follow the setup's pushed procedure through the setter to object offset 176;
compare the separate region binding. Follow the dispatcher's current-object
load and indirect call to its return boundary. Check the transition's two
sound paths against FND-UI-024. No jump-table or no-other-caller claim is made.
