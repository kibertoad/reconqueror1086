---
id: FND-RES-035
title: CONFIG.EXE opens its script read-only, defaulting to INSTALL.DAT, and asks for a drive when the open fails
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1A72:03BB..1A72:05BD
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2D43:0003..2D43:05C3
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2FEA:04FD..2FEA:05A0
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1000:52C4..1000:5433
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1000:5433..1000:5482
tool: capstone 5.0.7 bounded 16-bit disassembly with relocation targets marked
environment: null
---

## Observation

Addresses are as FND-RES-034 gives them for `CD:CONFIG.EXE`. The routines
1000:5EBD, 2E86:0123, 2E86:0C89, 1000:0719, 1890:00A7, 1890:00F2,
1890:0119, 1890:032F and 17EF:0435 are named below by address and by the
arguments they are given; their bodies were not read for this finding.

Open. 1000:5433 takes a far path and an access word. It sets AL to 1 when
the access word has bit 1, to 2 when it has bit 2, and to 0 otherwise,
adds the access word's bits 4 to 7, and issues DOS function 3Dh (open)
on the path. On success it stores the access word, masked with 0xB8FF and
with bit 15 set, in a per-handle word table at DS:701A and returns the
handle; on carry it passes the DOS error to 1000:0921 and returns its
result. 1000:52C4 takes a far path, an access word and a mode; when the
access word has neither bit 14 nor bit 15 it takes them from DS:7042; with
access bit 8 clear it reaches 1000:5433 with the path and access word.
2FEA:04FD copies the path to an 86-byte frame buffer, calls 1000:52C4 with
it and its own access and mode arguments, and returns the handle; when the
handle is 0xFFFF and its far callback argument is null it returns 0xFFFF.

Script search, 2D43:0003, called with a context and a far path. On its
first call it allocates through 17EF:0435 the far buffers it keeps at
DS:5170 (80 bytes), DS:5174 (260), DS:5178 (5) and DS:517C (80). When the
byte at DS:5180 is set, a null path skips straight to the open, as does a
path that 1890:032F reports equal to the buffer at DS:5170. Otherwise:

- With a path, it passes the path to 1890:00A7, divides it through
  2E86:0123 into the drive buffer, the directory buffer and a name and
  an extension buffer in its frame, fills an empty drive from 1000:0719 plus 0x41,
  an empty directory from 2E86:0C89 into DS:5174 (copying from its third
  byte), an empty name with `INSTALL` and an empty extension with `DAT`,
  drops one trailing backslash of the directory, passes the context and
  the directory to 1B80:3793, and formats
  `%c:%s\%s.%s` from the drive letter, directory, name and extension into
  the buffer at DS:5170 through 1000:5EBD.
- With a null path, it fills the directory from 2E86:0C89, drops one
  trailing backslash, and formats `%s\INSTALL.DAT` with it into DS:5170.

At 2D43:051D it calls 2FEA:04FD with the buffer at DS:5170, access 0x8001
and a mode and callback of 0; access 0x8001 reaches DOS 3Dh with AL 0, so
the file is opened for reading only. A handle of 0xFFFF goes to 2D43:0282,
which calls 35C0:0DF7. When DS:5180 is set and the drive letter at
offset 0x0A of the record at offset 8 of the context equals the first
byte of the drive buffer, it writes messages to the stream at DS:954B,
among them the drive letter and the string at DS:1774, calls
35C0:08A5 with that stream and tries the open again. Otherwise it lists
the drives whose information record from 2DEF:0009 has bit 3
of byte 0x12 set, through a menu routine at 3412:000B (calling 35C0:082A with 0 when it
returns 0xFFFF), formats
`%c:\INSTALL.DAT` with the drive picked into DS:5170, and tries the open
again. When the byte at DS:5180 was clear before a success, the first byte
of the path is stored at offset 0x0A of the record at offset 8 of the
context and in the drive buffer. DS:5180 is then set to 1, and the path is
stored, through 17EF:06BE, in the script variable that 3044:016D finds
under the name `@SCRIPTFILE`. The handle is returned.

Caller. The routine at 1A72 that formats `%s:%s\INSTALL.DAT` into the far
pointer at DS:7B82 (FND-RES-034) takes the two strings from 2E86:0123 run
on the first entry of its argument vector at 1A72:03D8; it leaves DS:7B82
null when either string is empty. At 1A72:0574 it passes DS:7B82 to
2D43:0003 when the word at DS:7B90 is 3 or more and DS:7B82 is not null,
and a null path otherwise, stores the returned handle at DS:1310, and
passes it with the context to 1B80:184F.

Starting it. BLD-GOG-EN's build entry records that the GOG settings
configuration runs `CONFIG.BAT`, which runs `D:\config.exe`, the disc's
CONFIG.EXE.

## Interpretation

CONFIG.EXE is the program that opens and runs INSTALL.DAT. With no other
name, it opens `INSTALL.DAT` beside its own executable on DOS versions
that give a program its path (3 and later, if DS:7B90 is the DOS major
version), and otherwise in the current directory; when that fails it asks
for the drive holding `INSTALL.DAT`. 1B80:184F is the script runner.

## Alternatives

- INST.EXE reads INSTALL.DAT: FND-RES-034 finds that it does not name it.
- DS:7B90 is something other than the DOS major version: not read; it
  decides only which of two directories is tried first.

## How to reproduce

Disassemble `CD:CONFIG.EXE` as 16-bit code with the load image at segment
0x1000 and relocation targets marked, at the ranges in Locations and at
2D43:0282 to 2D43:0486; read the strings at DS:5181 to DS:52BE, DS:5352,
DS:15DF and DS:1628. Keep listings in ignored local storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.
