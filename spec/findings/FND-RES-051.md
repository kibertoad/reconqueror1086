---
id: FND-RES-051
title: INST.EXE runs an unknown INSTALL.SCR line through spawn with one argument string, then the command interpreter, with standard output redirected for > and >>
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2DDA:000A..2DDA:02A7
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:1EA8..1C17:1F1A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:1F7F..1C17:202F
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2FBD:0009..2FBD:013E
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:11EE..3583:120D
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:3A7E..3583:3A89
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked
environment: null
---

## Observation

The unpacked file, its notation and the call from 1C17:05D5 are as
FND-RES-055 gives them: 1C17:05D5 builds an object on its stack through
1C17:1EA8, sets its vtable word to DS:1202, calls the vtable's first entry
with the object and a string made from the line, maps the result to a
message and stores the object's word at +8 in DS:5340. Run-time routines
named below with "(not read)" are known only by their arguments.

Objects:

- 1C17:1EA8 sets the vtable word to DS:11F6, makes the string at +2 empty
  through 2FD0:009B, sets the word at +6 to 0 and the word at +8 to 0xFFFF,
  and builds the object at +0xA through 1C17:1F7F.
- 1C17:1F7F passes 1 to 2FBD:0009, which stores it at +2, and sets that
  object's vtable word to DS:11EE.
- DS:1202 holds 2DDA:000A, 1C17:1FE8 and 1C17:200C; DS:11F6 holds 2DDA:000A,
  1C17:1FC8 and 1C17:1FD8, which return at once; DS:11EE holds 2FBD:0044
  and 2FBD:0103.
- 1C17:1FE8 calls 2B44:040C and 2A64:001F, and 1C17:200C calls 2A64:0002
  and 2B44:0477, each only when the word at +6 is 0. None of the four was
  read.

The runner, 2DDA:000A (this, line):

- It sets +6 to 0 and trims space and tab (DS:3A7E) from the line through
  2FD0:05C0 (not read).
- When the line holds a `>`: it sets +6 to 1 and stores a NUL over the
  `>`. When the next byte is `>` too it moves past it and the append flag
  is 1, otherwise 0. The rest of the line, trimmed the same way, is the
  file name. It calls the first entry of the object at +0xA with the name,
  the append flag and 0, and returns 1 when that returns 0.
- It calls vtable entry +4 of this.
- It copies the line into the string at +2 and looks for the first of `/`,
  space, tab or LF (DS:3A84) through 1000:64E7 (not read). The arguments are
  the text from that byte on, or an empty string when there is none, trimmed;
  a NUL then ends the program name at that byte.
- It calls 1000:3EEE (not read) with 0, the program name, the program name
  again, the arguments, and a null far pointer, and stores the result at +8.
  When the result is -1 it calls 1000:3F69 (not read) with the line, and
  the run counts as failed when that returns nonzero. Any other result
  counts as success.
- It calls vtable entry +8 of this.
- When +6 is 1 it calls the second entry of the object at +0xA and returns
  3 when that returns 0.
- It returns 2 for a failed run and 0 otherwise.

Redirection, 2FBD:0044 (this, name, append, binary), with the handle 1 at
+2:

- It calls 1000:07A9 (not read) with 1 and keeps the result at +4; -1
  returns 0.
- The mode is 0x8000 when `binary` is not 0 and 0x4000 otherwise, with 2
  added. With `append` it adds 0x0800. Without it, it first deletes the
  file through 1000:1A62 and adds 0x0100. It opens the name through
  1000:53CF with that mode and the permission word 0x0180; -1 returns 0.
- With `append` it calls 1000:0C2A (not read) with the handle, 0 and 2.
- It calls 1000:0772 (not read) with the handle and 1, and closes the
  handle through 1000:4271; -1 from either returns 0. Otherwise it returns 1.

2FBD:0103 (this) calls 1000:0772 with the handle at +4 and the one at +2,
and closes the handle at +4; -1 from either returns 0, otherwise 1.

## Interpretation

A script line whose first word is not a command runs as a program. Its
first word, up to a `/`, space, tab or LF, is the program, and the rest is
passed as one argument string, as a DOS command tail. When the program
cannot be started that way, the whole line, without its redirection, goes to
the command interpreter, which handles built-in commands and batch files.
`errorlevel` (FND-RES-048) then sees the spawned program's exit code, and
-1 after the command interpreter path, whose result is not kept. With `>`
the file is deleted and created anew; with `>>` it is opened for appending
and the open fails when the file does not exist, unlike `echo`'s `>>`
(FND-RES-055). Both keep standard output pointed at the file while the
program runs and restore it after. When the output is not redirected, the
installer calls two routines before and two after the program, which
probably give the screen to the program and take it back.

The mode bits are read as the Borland C library's `O_WRONLY` (2), `O_CREAT`
(0x0100), `O_APPEND` (0x0800), `O_TEXT` (0x4000) and `O_BINARY` (0x8000);
1000:53CF was not read, so that reading rests on the library's published
values.

## Alternatives

- The redirection is left to the command interpreter: ruled out for the
  spawned path; the runner opens the file and duplicates it over handle 1
  itself before the spawn.
- The arguments are split into several argument strings: ruled out; the
  spawn call gets the program name twice, one argument string and the null
  pointer.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-054 gives. Read the far pointers at DS:11EE,
DS:11F6 and DS:1202 and the strings at DS:3A7E to DS:3A89. Disassemble the
ranges in Locations as 16-bit code with the load image at segment 0x1000 and
relocation targets marked. Keep listings in ignored local storage.
