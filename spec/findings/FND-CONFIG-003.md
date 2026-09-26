---
id: FND-CONFIG-003
title: The other keys: CD_PATH at eleven sites, the view window size, SLOWMACHINE, USE_CYBERMAN, FULL_MOVIE, DELAYVGA and an unused GRAPHICS
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00050E02..0x00050FBD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00054912..0x0005495D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B827..0x0005B8F3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003020E..0x0003022D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000586D7..0x000586FB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000607F6..0x00060812
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00010601..0x0001062E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001A193..0x0001A1CD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001B3AC..0x0001B404
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002AC93..0x0002ACB6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002ADAE..0x0002ADEA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003013B..0x00030163
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E808..0x0003E837
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003FD1D..0x0003FD8A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000411E8..0x00041237
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B8EE..0x0005B916
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00061CFF..0x00061D3C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018334..0x00018391
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004EF88..0x0004EF9C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00056387..0x000563FD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00071954..0x0007199C
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Every reader calls `0x00062EA0` (FND-CONFIG-002). Besides the startup keys of FND-RES-006 and
FND-SOUND-004, the sound driver keys of FND-SOUND-004 and `WAR_MODE` (FND-CONFIG-004):

- `POVWINSIZE` at `0x00050E26`, before the viewer loads: `NORMAL` stores x 3, y 6, width 210 and
  height 152 at `0x0009CB64`, `0x0009CB68`, `0x0009CB6C` and `0x0009CB70`; `FULLSCREEN` stores 0,
  0, 320 and 200 there and also 320, 200, 0 and 0 at `0x0009CDBC`, `0x0009CDC0`, `0x0009CDC4` and
  `0x0009CDC8`; `SMALL` 11, 12, 194 and 140; `SMALLER` 19, 18, 178 and 128; `SMALLEST` 23, 22, 170
  and 120. A missing key or another value keeps the values stored just before the read.
- `SLOWMACHINE` at `0x00050F95`: `0x0009D5BC` is set to 0, then to 1 when the value is `TRUE`. Its
  only reader, `0x0004EF88`, skips the block up to `0x0004F067` when it is 1.
- `USE_CYBERMAN` at `0x0005492C`: `0x000A958C` is set to 0, then to 1 when `[0x000A9DE0]` is not 0,
  the value is `ON` and `0x00018334` returns 1. `0x00018334` clears `0x000A9580` and `0x000A9560`,
  queries the device through `0x00018080(0x000A957C, 4)` and `0x000181E8`, and returns 1 when the
  first byte of the answer is 1. The movement code at `0x00056387` then reads the device through
  `0x00018294` and scales its axes into the turn and move rates at `0x000AFA40`, `0x000AFA24` and
  `0x000AFA44`.
- `FULL_MOVIE` at `0x0005B83E`, in the entry routine of screen 0 (`TITLE.HAT`): when `MOVIE` is on
  (`0x0009ADC4`) the opening movie is `movie3f.smk` if the key is present with any value and
  `movie3.smk` otherwise. With `CREDITS` on (`0x0009ADC8`) it then plays `kncredit.smk`.
- `DELAYVGA` at `0x00030218` and `0x000586EA`, after a full-screen movie and after the view: when
  the value is `ON`, `0x00072119(3)` is called (FND-MEDIA-009). Neither site tests the result
  for 0 before handing it to `0x00069890`.
- `GRAPHICS` at `0x00060800`, in the entry routine of screen 11 (`VOPTS.HAT`): the result is not
  used.
- `CD_PATH`, joined with a movie name: at `0x00010606` with `trandrag.smk` when `ANIMATIONS` is on,
  `0x0001A198` with `%s%d%c.smk`, `0x0001B3E6` with `%sDRJSTRUN.SMK`, `0x0002AC98` with
  `%sSWSLOGO.SMK`, `0x0002ADB3` with `avg_end.smk`, `0x00030140` with the name passed to the
  full-screen player, `0x0003E80D` with a name from the table at `0x0009B750`, `0x0003FD4F` with
  `%sJOUST%d.SMK`, `0x00041214` with `jousprac.SMK`, `0x0005B8F3` with `kncredit.smk` and
  `0x00061D04` with a store item's movie (FND-UI-014). A missing key gives an empty directory at
  `0x00010606`, `0x0001A198`, `0x0001B3E6` and `0x00061D04`, `.\CD\` at `0x0002AC98`, `0x0002ADB3`,
  `0x00030140` and `0x0005B8F3`, `.//` at `0x0003E80D`, and `.\` at `0x0003FD4F` and `0x00041214`.
- `DIG_SPEECH` is also read at `0x0001B3BC`, `0x0003FD24` and `0x000411EC`, each testing for 0 and
  comparing with `ON` before it opens a movie.

`0x00071954(key, name)` reads `key`, and when it is present and `0x000719A0(name)` finds no `:`, `/`
or `\` in `name`, joins them through `0x00071838`; otherwise it clears the two buffers at
`0x0009E2C0` and `0x0009E2BC` and returns 0. No direct call to `0x00071954` was found.

## Interpretation

`POVWINSIZE` sets the rectangle of the first-person view inside the 320 by 200 area it is drawn in;
`FULLSCREEN` fills that area. `SLOWMACHINE` turns off part of the view's drawing. `USE_CYBERMAN`
enables Logitech's CyberMan 3D controller when the mouse driver reports one. `FULL_MOVIE` chooses
a longer version of the opening movie; the GOG disc has both files. `GRAPHICS` is left over from an
earlier setting. The `CD_PATH` defaults differ by site, so a file without the key finds the movies
in different directories depending on which one plays.

## Alternatives

That the device behind `0x00018334` is the CyberMan rests on the key's name; the query itself was
not traced. What the block skipped for `SLOWMACHINE` draws was not traced. Whether `movie3f.smk`
is longer than `movie3.smk` was not compared.

## How to reproduce

Disassemble the listed ranges and read the strings at the object-2 offsets pushed before each call
to `0x00062EA0`.
