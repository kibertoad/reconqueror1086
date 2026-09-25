---
id: FND-JOUST-006
title: The practice worker waits for a click or key after its first pass and after the result, and picks a miss message from the signs of the signed totals
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000412A0..0x00041312
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041929..0x00041A33
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004144E..0x00041468
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041B59..0x00041CD8
tool: Ghidra 12.1.3
environment: null
---

## Observation

At `0x000412A0`..`0x0004130D` the worker draws an opening prompt; the call at `0x00041312` only
clears the input count. At the end of its first pass, at `0x00041929`..`0x000419DB`, it loops
until event decoder `0x00063114` returns 3 or keyboard poll `0x00065380` returns non-zero,
starts two sounds when sound is on, and at `0x000419E0`..`0x00041A33` clears the prompt; later
passes skip this. At `0x0004144E`..`0x00041468` each pass first ends the loop when the movie's
frame counter equals its frame count less one. After the result, at `0x00041B59`..`0x00041C93`,
it draws the message at `0x00096218` (a prompt to press a key or button), then for result 1 the
message at `0x0009623C` (a win), and for results 0 and 2 one of eight messages chosen by the
signs of the signed totals `sx` and `sy`: `sx > 0, sy = 0` too far left (`0x00096250`);
`sx < 0, sy = 0` too far right (`0x0009626C`); `sx = 0, sy < 0` too low (`0x00096288`);
`sx = 0, sy > 0` too high (`0x00096298`); `sx > 0, sy < 0` left and low (`0x000962A8`);
`sx > 0, sy > 0` left and high (`0x000962C4`); `sx < 0, sy > 0` right and high
(`0x000962E4`); `sx < 0, sy < 0` right and low (`0x00096304`). When both are 0 it draws no
miss message. At `0x00041CAE`..`0x00041CD8` it then waits again for event 3 or a key.

## Interpretation

The run is held on its first frame, with the prompt shown, until the player clicks or presses a
key. A miss tells the player which way the lance was off, from where the target lay relative to
the lance summed over the three contact frames; the opponent's roll is never announced.

## Alternatives

An earlier reading took the call at `0x00041312` as the only possible wait and concluded that
the run starts without one. The wait at `0x00041929` on the first pass rules that out.

## How to reproduce

Open `0x00041929`; `edi` holds 1 only on the first pass, and the loop calls `0x00063114` and
`0x00065380`.
