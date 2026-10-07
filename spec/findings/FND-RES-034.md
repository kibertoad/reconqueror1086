---
id: FND-RES-034
title: CONFIG.EXE runs the INSTALL.DAT script, whose only use of CONQUER.INF is an @exists test through DOS find-first
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1A72:040F..1A72:0488
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:00C2..1B80:00F4
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2377:1170..2377:118C
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 220F:000E..220F:0103
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2E86:0418..2E86:04DD
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2E86:086C..2E86:0A5F
  - build: BLD-GOG-EN
    file: CD:INSTALL.DAT
    offset: 0x3A8..0x3F4
tool: capstone 5.0.7 bounded 16-bit disassembly with relocation targets marked; Python search of every file on the disc image
environment: null
---

## Observation

`CD:CONFIG.EXE` is an MZ file with a 0x4000-byte header whose load image ends
at file offset 0x35650; its data segment is at segment 0x27EF of the load
image (37EF in the notation). It is not packed. Addresses are `segment:offset`
with the load image at segment 0x1000.

Script file. The data segment holds the strings `X:\INSTALL.DAT` (DS:15DF),
`%s:%s\INSTALL.DAT` (DS:1628), `%s\INSTALL.DAT` (DS:51B6), `%c:\INSTALL.DAT`
(DS:52BE), a message that the script file `INSTALL.DAT` cannot be reopened
(DS:17AE) and one naming an expected name from INSTALL.DAT (DS:4DA2). Each
is loaded by one `mov ax` in code: DS:15DF at 1A72:044A and DS:1628 at
1A72:0477, DS:17AE at 1B80:00EB, DS:51B6 at 2D43:026B, DS:52BE at 2D43:046D
and DS:4DA2 at 2C2F:0674; the last three were located only, not read. At
1A72:040F to 1A72:0488 the code takes a directory string, drops a trailing
backslash, allocates the length of that string plus the length of
`X:\INSTALL.DAT` plus one through 27EF:0435 into the far pointer at
DS:7B82, and formats `%s:%s\INSTALL.DAT` into it through 1000:5EBD with a
drive and the directory. At 1B80:00C2 to 1B80:00F4 a call to 1000:0A1F with
a handle, the far value at DS:177F and 0 that returns DX:AX equal to
0xFFFF:FFFF passes the reopen message to 213C:0007. Where the file is first
opened, and how its lines are tokenised, was not read.

`@EXISTS`. The string `@EXISTS` is at DS:3880. At 2377:1170 to 2377:118C
the code passes the far context at [bp+6], DS:3880, 2 and the far address
2377:43C8 to 3044:0465, the routine that each of the other `@` names
around it is passed to with its own handler. The handler at 2377:43C8
reads one argument into a string, through 221F:1125 when the next token is
0x28 and through 221F:0B5C otherwise, calls 220F:000E with it, and stores
the byte that call returns, widened to 32 bits, at offsets 6 and 8 of its
result. 220F:000E calls 220F:0070 and returns 1 when that returns a pointer
that is not null, and 0 otherwise. 220F:0070 clears 43 bytes at DS:7BCC
through 1890:0281, builds the attribute 0x37 one bit at a time in a
cleared byte, and calls 2E86:086C with the path, the far address DS:7BCC
and that byte in AL; it returns DS:7BCC when that call returns 0 and null
otherwise.

2E86:086C issues three DOS calls through 2E86:0418, each with interrupt
0x21 and register blocks in its own frame: AH 2Fh (get the disk transfer
address), AH 1Ah with DS:DX set to a 43-byte block in its frame, and AH 4Eh
(find first) with CX the attribute byte and DS:DX the path's segment and
offset. 2E86:0418 copies the input registers, calls 1000:2DAB with the
segment registers when a segment block is given and 1000:2D7A otherwise,
copies the result back, and returns the word at offset 0x0C of the output
block, the carry flag. 2E86:086C keeps that word; when it is 0 it copies
0x15 bytes and then the attribute at 0x15, the time, date and 32-bit size
at 0x16 to 0x1D and the 13-byte name at 0x1E of the transfer block into
the caller's block. It then restores the earlier transfer address and
returns the kept word. So `@exists(path)` gives 1 when DOS find-first
with attribute 0x37 (read-only, hidden, system, directory and archive)
finds a match for the path, and 0 otherwise; the file is not opened.

Use in the script. `CD:INSTALL.DAT` names `CONQUER.INF` once, at 0x3D4, in
the condition of an `@if` at the start of the `MENU:` block. The condition
is `@exists` of `CONQUER.INF` in the startup drive and directory; its
comment says that the install is then running from the CD, and its branch
sets option 20 and clears options 30 and 40, leaving install and exit.

Search. Every file of the disc image, 2,849 files in all directories, the
15 members of `CD:SETUP.SOL` expanded as RULE-RES-005 gives, and `CD:INST.EXE`
unpacked as the build's file list gives (189,616 bytes, XXH3-128
`bf7404f9532b9075dbe7004337db4119`) were searched for `CONQUER.INF`,
`SIERRA.INF` and `INSTALL.DAT`, ignoring letter case, as 8-bit text and as
UTF-16LE. `CONQUER.INF` occurs only at `CD:INSTALL.DAT` 0x3D4.
`INSTALL.DAT` occurs only in `CD:CONFIG.EXE`, six times, at the strings
above. The positive control `SIERRA.INF`, located by reading the files
directly (FND-RES-029, FND-RES-033), is found at `CD:SETUP.EXE` 0x5132 and
0x514A, in the expanded `_SETUP.EXE` at 0x22AF2, and as UTF-16LE in the
expanded `SETUP32.EXE`, and in the `SETUP.EXE` of three demo directories.
The search covered names written whole; it did not cover names built from
parts at run time or stored compressed inside another file's own format.

## Interpretation

CONFIG.EXE is the interpreter of INSTALL.DAT: it builds INSTALL.DAT's path
from a drive and directory and reports a failure to reopen its script file
under that name. CONQUER.INF is a marker whose presence the script tests;
no located code reads its contents. INST.EXE, the DOS installer, names
neither file.

## Alternatives

- INST.EXE reads CONQUER.INF: its unpacked image names neither CONQUER.INF
  nor INSTALL.DAT.
- Another program reads CONQUER.INF's contents under a name it builds at
  run time: not ruled out by a search for whole names, and no such program
  was located.
- `@exists` opens the file: 2E86:086C only calls find-first and copies the
  transfer block.

## How to reproduce

Disassemble `CD:CONFIG.EXE` as 16-bit code with the load image at segment
0x1000 and relocation targets marked, at the ranges in Locations and at
2377:43C8 onward; read the strings at the data
segment offsets named. Unpack `CD:INST.EXE` with `tools/evidence/unlzexe.mjs`
from commit 0f1ac61. Walk the disc image's ISO 9660 directories from the
primary volume descriptor, reading each file whole, and search each file,
each expanded SETUP.SOL member and the unpacked INST.EXE for the three names
in 8-bit and UTF-16LE text, ignoring case; report offsets only. Keep
listings in ignored local storage.
