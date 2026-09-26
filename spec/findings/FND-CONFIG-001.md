---
id: FND-CONFIG-001
title: Startup finds CONQUER.INI in four places and loads it into a list of keys and values
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A38A..0x0002A3D3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000715D0..0x00071837
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062B80..0x00062D92
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00062FC0..0x00062FFD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000636D0..0x00063747
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

At `0x0002A38A` startup prints `Conq-INI: Initializing system dependant items.` and calls
`0x000715D0(0, name, 1, 1)` with the name `CONQUER.INI`. `0x000715D0(variable, name, use_sorcery,
use_path)` tests candidate paths with `0x0006A190(path, 0x18, 0)` and returns, for the first that
exists, the address `0x0009E2BC` of two buffer pointers: the full path and, at `0x0009E2C0`, its
directory. The candidates are, in order: the directory in the environment variable named by
`variable`, when that is not 0; the directory in the environment variable `SORCERY` when
`use_sorcery` is not 0, both joined as `%s/%s`; `./%s`; the directory of the program, the first
argument string at `0x000A680C` cut after its last `\`; and, when `use_path` is not 0, the
directories of `PATH`, which `0x00071838` splits at spaces, tabs and `;`. It returns 0 when none
exists. Startup stores the result at `0x000A9DE0`. A result of 0 stops the game through
`0x000636D0(0, "Conq-INI: File not found. Aborting.")`; otherwise it calls `0x00062B80` with the full
path and then `0x0002AA54` (FND-RES-006, FND-SOUND-004).

`0x00062B80(path)` first calls `0x00062FC0`, then opens the file with mode `rt` through
`0x00063612`; a failure calls `0x000636D0(2, path)`. A first pass reads lines with `0x00065025(buffer,
0x400, file)`, splits each with `0x00062ED0` (FND-CONFIG-002), and for each line that has a key adds
1 to a count and the lengths of key and value, each plus 1, to the dword at `0x000B0524`. It then
allocates `count * 12` bytes at `0x000B0520` and `0x000B0524` bytes at `0x000B0528` through
`0x00063D20` (`0x000636D0(1, path)` on failure), seeks to the start with `0x0006836C(file, 0, 0)`
(`0x000636D0(3, path)` on failure) and reads the file again. For each line with a key the second
pass copies key and value into the pool one after the other and fills the next 12-byte node: the key
address at `+0`, the value address at `+4`, and at `+8` the address of the following node, 0 in the
last. The nodes follow the file's order inside the one block. It closes the file with `0x00063937`
and, when the dword at `0x000B052C` is 0, registers `0x00062FC0` with `0x0006A149` and stores 1
there. `0x00062FC0` frees the block at `0x000B0520` and the pool at `0x000B0528` when they are not 0
and stores 0 in both.

`0x000636D0(code, text)` picks the message for `code` from the 16 addresses at `0x0009DF54`, or
`Unknown` and code 50 when `code` is outside 0 to 15. It prints `ERROR: %s %s.\nProgram Aborted\n`
with the message and `text`, or `ERROR: %s.\nProgram Aborted.\n` when `text` is 0, prints
`\tPress any key\n`, calls `0x0007C5C6` and `0x0006B4A8`, and exits through `0x00064FF5` with
`code + 100`. The messages from code 0 on are an empty string, `Allocating Memory`, `Opening File`,
`Seeking File`, `Reading File`, `Writing File`, `VESA`, `Insufficient Display Memory`, `Object
Overflow Loading`, `CD ROM Drive Not Found`, `CD ROM Access Failure`, `Hard Drive Access Failure`,
`Allocating DOS Memory`, `Sound System Fault`, `Initializing Mouse` and `General`.

## Interpretation

The game takes its settings from the first `CONQUER.INI` it finds, starting with a directory named
by `SORCERY`, then the current directory, the program's directory and `PATH`. Under the GOG
configuration the current directory is `C:\`, which holds the installed file, so the disc's copy
is never read. The settings stay in memory for the whole run. `0x000636D0` is the game's fatal
error routine: every failure it reports ends the program with an exit code of 100 plus the code.

## Alternatives

`0x0006A190(path, 0x18, 0)` is taken to test that the file exists; its flags were not traced. That
the key passed is the name `CONQUER.INI` rests on the startup string pool; the stack buffer it is
copied into was not followed. How `0x00071838` walks `PATH` after the first directory was not
traced.

## How to reproduce

Disassemble the listed ranges in `CD:CONQUER.EXE` and read the strings at object-2 offsets
`0x3880`, `0x38B0`, `0x9774` to `0x978C`, `0x8FCC` and the table at `0xDF54`.
