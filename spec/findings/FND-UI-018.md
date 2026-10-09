---
id: FND-UI-018
title: Startup performs display preparation before testing the animation switch and entering the initial screen loader
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A815..0x0002A828
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002AC08..0x0002AD5B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024CA0..0x00024D12
tool: Capstone 5.0.7, bounded 32-bit decoding of the fingerprinted LE code object
environment: null
---

## Observation

The startup call at `0x0002A815` enters `0x0002AC08`. On its return,
the caller passes screen 0, draw 0 and mode 1 to the initial screen loader
of FND-UI-017 at `0x0002A820`.

The startup helper saves three registers and reserves 80 stack bytes.
Before testing the animation flag, it calls the resource/display helpers
at `0x0005B584`, `0x00024DB8`, `0x00063270`, `0x0005B3B0` and
`0x0005B790`. It conditionally copies the returned buffer through
`0x00064BCF`, calls `0x0005B554`, and calls `0x00024CA0`.
These calls occur even when the animation flag is zero; their internal
completion and timing are not established by this reading.

Only after those calls does the helper test `0x0009ADB8`, the animation
option read in FND-SOUND-004. Zero branches directly to the epilogue
at `0x0002AD54`. Nonzero follows movie setup and calls the playback
function of FND-MEDIA-009 with coordinates (0, 0), repeat count 15,
palette enabled, its input-stop callback and direct output enabled.
After playback it calls `0x00024CA0` again and four further service
helpers before the same epilogue. The epilogue releases the 80 bytes,
restores the three registers and returns at `0x0002AD5A`.

The helper at `0x00024CA0` preserves the dword at `0x000B084C`, invokes
its service calls in order, temporarily writes zero there, then restores
the preserved value before returning. This reading does not identify the
whole service interface or establish its hardware-dependent outcome.

## Interpretation

Turning animations off bypasses movie playback in this helper, but does
not bypass the preceding preparation and reset calls. Reaching its
epilogue is a separate observable boundary before screen loading, not
evidence that a screen object is loaded or ready for input.

## Alternatives

A pending screen-loader breakpoint alone cannot distinguish unfinished
preparation, playback or another path. A breakpoint at the helper's
epilogue can distinguish completion of this startup helper; the loaded
object still requires the return checks of FND-UI-017.

## How to reproduce

Verify BLD-GOG-EN identity and decode the listed ranges. Track the three
saved registers, 80-byte local allocation and argument cleanups. Follow
both outcomes of the animation test into the shared epilogue and the
caller's next screen-loader call. This is a bounded reading, not a
complete reading of the called services or all startup paths.
