---
id: FND-CONFIG-004
title: WAR_MODE 640 keeps the field battle at 640 by 480; any other value or none tries 1024 by 768 first
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00025C53..0x00025D86
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006FF50..0x00070083
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000700C0..0x0007010D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00070120..0x0007024A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003023C..0x00030253
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A477..0x0002A49F
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

After loading `BATTLE.PCX` the field battle calls `0x0006FF50(spec, &mode)`, which stores 0 in
`mode` and looks through the `0x000A67D8` entries whose type byte at `0x000B0F20 + i` is 2, comparing
the name at `0x000B0F40 + 4 * i` without case against `spec` through `0x00073363`. On a match it
reads a decimal number into `mode` and returns 1. When it returns 0, `0x00062EA0("WAR_MODE")`
supplies the value, converted with `0x00069810`, or -1 when the key is missing.

When `mode` is 640 the display is left alone. Otherwise `0x000700C0` frees the three display
buffers at `0x000B0840`, `0x000B0844` and `0x000B083C`, and:

- unless `mode` is 800, `0x00070120(2)` is tried; on success the clip rectangle becomes the whole
  screen and `0x000A9CC8` is set to 1024;
- when `mode` is 800 or mode 2 failed, `0x00070120(1)` is tried; on success `0x000A9CC8` is set to
  800;
- otherwise `0x00070120(0)` is called and `0x000A9CC8` stays 0.

`0x00070120(n)` sets the screen globals for display mode `n` and calls `0x000834F0` with its VESA
mode; on success it stores 2 at `0x000B0BF0` and returns 1:

| `n` | Width `0x000B0854` | Height `0x000B0868` | VESA mode | Font `0x000B085C` | `0x000B0870` | `0x000B0860` |
|---|---|---|---|---|---|---|
| 0 | 640 | 480 | `0x101` | `FONT` | 8 | 10 |
| 1 | 800 | 600 | `0x103` | `FONT` | 8 | 10 |
| 2 | 1024 | 768 | `0x105` | `FONT1024` | 13 | 16 |

Each also stores 256 at `0x000B0858`. Startup (`0x0002A49D`) and the return from a full-screen movie
(`0x00030246`) call `0x00070120(0)` and stop the game with `Conq-Vid: Initializing video system.
Aborting.` when it fails.

The installed `CONQUER.INI` has `WAR_MODE=1024`; the disc's copy has `WAR_MODE=640`.

## Interpretation

The field battle uses the largest VESA mode up to 1024 by 768 that the card offers unless
`WAR_MODE` is 640, and 800 limits it to 800 by 600. A missing key behaves like 1024. The rest of
the game runs at 640 by 480. `0x0006FF50` searches a table of named options, probably the command
line, before the INI file.

## Alternatives

Where the table `0x0006FF50` searches is filled (`0x00071400` onwards) and which name `spec` holds
were not traced; the command line is a guess. FND-BATTLE-018 earlier said a mode is tried only
when the key is present; the branch at `0x00025CE9` shows -1 goes to the same attempt.

## How to reproduce

Disassemble the listed ranges in `CD:CONQUER.EXE`; compare the `WAR_MODE` lines of `CONQUER.INI`
and `CD:CONQUER.INI`.
