---
id: FND-CONFIG-002
title: Keys match case-sensitively, and a write rewrites every stored line of CONQUER.INI
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062ED0..0x00062FBF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062EA0..0x00062ECE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062D94..0x00062E9C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00037CA3..0x00038472
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00052405..0x00052498
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005860C..0x000586A5
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00062ED0(line, &key, &value)` skips bytes whose entry in the character class table at
`0x0009A26C` has bit 2 set, the table's white-space class. When the line then ends or starts with
`#` or `;` it stores 0 in both results. Otherwise the key starts there and ends at the first `=`;
without one it calls `0x000636D0(15, "Bad cfg line, missing '='")`. It cuts white space before the
`=`, and the value starts after the `=` and the white space that follows it and ends before the
white space at the end of the line, including the line end. White space inside a value, and any
later `=`, stay in the value.

`0x00062EA0(key)` walks the nodes from `0x000B0520`, compares each key with `key` through
`0x00069890`, and returns the value address of the first match, or 0 when none matches or no file
was loaded.

`0x00062D94(path, key, value)` opens `path` with mode `wt` through `0x00063612`; a failure calls
`0x000636D0(5, path)`. For each node in order it compares the key with `key` through `0x00069890`.
On a match it frees the node's value through `0x00063D78` unless the value lies inside the pool
from `0x000B0528` to `0x000B0528` plus the dword at `0x000B0524`, allocates `strlen(value) + 1`
bytes for it through `0x00063D20` (`0x000636D0(1, path)` on failure) and copies `value` there. Every
node, matched or not, is then written with `0x000697EB(file, "%s=%s\n", key, value)`. The file is
closed with `0x00063937`.

Every caller passes the path at `[0x000A9DE0]`. The options screen calls `0x00062D94` from 17
places between `0x00037CA3` and `0x00038472`, storing `ON` or `OFF` under `MOVIE`, `ANIMATIONS`,
`CREDITS`, `SOUND_EFFECTS`, `DIG_SPEECH`, `CDMUSIC` and `MIDIMUSIC` (FND-UI-012). The routines at
`0x00052405` and `0x0005860C` store `POVWINSIZE` from the width at `0x0009CB6C`: `FULLSCREEN` for
320, `NORMAL` for 210, `SMALL` for 194, `SMALLER` for 178, `SMALLEST` for 170 and `NORMAL` for any
other width.

## Interpretation

`0x00069890` is the C library's `strcmp`, so keys and the values compared with `ON`, `TRUE` or a
window size match only in the case the file uses. A write keeps no comments or blank lines and no
spacing around `=`: the file becomes one `key=value` line per stored key, in the order they were
read, and a key the file did not have is never added. A key that appears twice is read from its
first line and changed on both. The file is opened in text mode, so each line ends with CR and LF.

## Alternatives

That `0x00069890` compares case-sensitively rests on its use as `strcmp` throughout; its body was not
traced. When the routines at `0x00052405` and `0x0005860C` run was not traced.

## How to reproduce

Disassemble the listed ranges; the strings are at object-2 offsets `0x8FD0`, `0x8FD4`, `0x8FDC` and
`0x7E10` to `0x7E40`.
