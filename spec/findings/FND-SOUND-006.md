---
id: FND-SOUND-006
title: CD music plays tracks 2 to 6 of the disc through MSCDEX requests and loops by replaying the range
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B368..0x0005B3AF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000703E0..0x0007047D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00070480..0x0007054A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00070550..0x000705C3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000705D0..0x0007060F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002ACF0..0x0002AD1E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00026040..0x00026050
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002625C..0x00026276
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000378A2..0x000378F3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B948..0x0005B96A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005BA1D..0x0005BA3F
  - build: BLD-GOG-EN
    file: game.ins
    offset: 0x00..0x112
  - build: BLD-GOG-EN
    file: CD:track02
    offset: 0x00..0x45B31F
  - build: BLD-GOG-EN
    file: CD:track03
    offset: 0x00..0x9729BF
  - build: BLD-GOG-EN
    file: CD:track04
    offset: 0x00..0x15F9A2F
  - build: BLD-GOG-EN
    file: CD:track05
    offset: 0x00..0x2648A0F
  - build: BLD-GOG-EN
    file: CD:track06
    offset: 0x00..0xE0A9BF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0005B368` returns the dword at `0x0009DBA8` and `0x0005B370` sets it (`CDMUSIC`, FND-SOUND-004).
`0x0005B37C(first, last, loop)` calls `0x00070480` with the same arguments only when it is set, and
`0x0005B3A0` stops the CD through `0x000705D0` when `0x000703E0` reports it playing.

The four CD routines do nothing when the dword at `0x000A67E0` is 0. They build requests in the
buffer the pointer at `0x000B11E0` holds and pass them to `0x00083AC0`:

- `0x000703E0` sends an input control request for the audio status (command 3, control code
  `0x0F`) and returns 1 when bit 9 of the returned status word, busy, is set.
- `0x00070480(first, last, loop)` stops the CD with `0x000705D0`, sets the dword at `0x000B11D8` to
  1 when `1 <= first`, `first <= count`, `1 <= last` and `last <= count` (`count` is the dword at
  `0x000B11D0`) and to 0 otherwise, stores `loop` at `0x000B11C8`, and plays either way. The start is
  the dword at `0x000B0FCC + 4 * first`, moved by the dword at `0x000B1160` when that is above 0;
  the length is the start of track `last + 1`, less the start, less 1, less that same dword, all
  converted to sectors by `0x00083BB0`. It stores them at `0x000B11D4` and `0x000B11CC` and plays
  them with `0x00083C80(start, length)`.
- `0x00070550` does nothing unless `loop` and the valid flag are set and the CD is not busy; then it
  resumes with `0x00083CD0` when the dword at `0x000B11DC` is set, and otherwise stops and plays the
  stored range again. It has nine callers in screen loops.
- `0x000705D0`, when the CD is busy, sends command `0x85` (stop audio) and clears `loop` and
  `0x000B11DC`.

The callers ask for single tracks with `loop` 1: track 2 at `0x0002AD08` in the start-up sequence,
track 3 at `0x0005BA35` and track 6 at `0x0005B960` in the title routine that plays the credits
(`kncredit.smk`, FND-MEDIA-009) with track 6 behind them, track 4 at `0x00026046` and
`0x0002625C` in the field battle loop, and track 5 at `0x000378A8` and `0x000378E9` in the game
options screen. Each caller first calls `0x000703E0` and calls the service routine instead when
the CD is already playing.

`game.ins` is the cue sheet of the GOG image: track 1 MODE1/2352 at 00:00:00, then audio tracks 2
to 6 at 54:19:20, 54:45:12, 55:41:24, 57:51:71 and 61:39:38. The five audio tracks are 4,567,584,
9,906,624, 23,042,544, 40,141,584 and 14,723,520 bytes, 25.9, 56.2, 130.6, 227.6 and 83.5 seconds
of 44,100 Hz 16-bit stereo audio.

## Interpretation

The game drives CD audio through MSCDEX device driver requests: `0x00070480` plays a track range
once and `0x00070550`, called from the screen loops, replays it when it ends, so a track loops with
a short gap. Track 2 is the opening music, track 3 the title music, track 4 the field battle, track
5 the game options screen and track 6 the credits.

## Alternatives

Where the track table at `0x000B0FCC`, the count at `0x000B11D0` and the adjustment at `0x000B1160`
are filled (the MSCDEX set-up near the string `CD track # out of range!` at `0x000A6874`) was not
traced; the names of the screens come from the callers' own routines (FND-UI-001, FND-UI-012,
FND-BATTLE-018).

## How to reproduce

Disassemble the listed ranges in `CD:CONQUER.EXE`; read `game.ins` and measure the audio tracks.
