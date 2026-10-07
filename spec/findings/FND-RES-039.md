---
id: FND-RES-039
title: _SETUP.EXE's FLAG runs the rest of its line when a numbered flag is set, and DISKSPACE_LT sets a flag when free kilobytes are below a value
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:SETUP.SOL
    offset: 0x1115F..0x47E42
  - build: BLD-GOG-EN
    file: CD:SIERRA.INF
    offset: 0x00..0xD6F
tool: Python decoder of RULE-RES-005; Python bounded NE relocation-chain walk; capstone 5.0.7 bounded 16-bit disassembly; Python walk of the shipped file written for this finding
environment: null
---

## Observation

Addresses, the routine 0004:3A1E, its position p, its object, its end flag
and the strings 0004:02EA, 0004:74BA and 0004:759C are as FND-RES-038 gives
them; the C library routines are those FND-RES-033 lists. 0001:3E34 returns
a pointer to the first byte of a string that is in a set. DS is segment 9;
the byte table at DS:111F marks the digits `0` to `9`, and only them, with
bit 2, and lower-case letters with bit 1. 0001:2374 returns a byte with bit
1 less 0x20, and any other byte unchanged.

Word readers.

- 0004:6E98 takes the object and a pointer to p. It copies the bytes from p
  up to the first space, tab or line feed (0004:74BA) into a new string,
  passes it with the object to 0003:C46A (not read here), moves p past
  those bytes and then past spaces, tabs and line feeds, and returns the
  string. The line feed is skipped too, so a missing word takes the first
  word of the next line.
- 0004:6F5E reads a word that way. When its first byte is a digit, the
  word's value from 0001:21D6 (32 bits) is stored through its pointer
  argument and it returns 1; otherwise it returns 0.
- 0004:6FBE reads a word that way. When its first four bytes are `FLAG`,
  compared through 0001:3D4C so letter case is ignored, it returns the low
  16 bits of 0001:21D2 on the bytes after them; otherwise 0xFFFF.

Flags are words at offset 0x56 + 2n of the object, for flag n.

`FLAG` (0004:3FE3). From p, 0001:3E34 finds the next space or tab and a
NUL is written there, without a test for null. n is the low 16 bits of
0001:21D2 on the text at p, and the word at offset 0x56 + 2n is tested,
with no bound on n. The NUL is replaced by a space. When the word is
nonzero, p moves to that space and past spaces and tabs, and the loop runs
the rest of the line as a command. When it is 0, p moves to the next line
feed (0001:3E34 with 0004:759C) and past spaces, tabs and line feeds.

`DIALOG` (0004:3EC4). p moves past spaces and tabs and a NUL is written over
the next line feed, without a test for null. The dialog array that
FND-RES-033's loader appends to at offset 0x48 holds its far pointers at
offset 0x4C and their count at offset 0x50. The first dialog in that order
whose name (the far pointer at offset 6 of its record) matches the argument
over the argument's length through 0001:3D4C is passed to 0003:9EE6 (not
read here) with 0, the object and the far pointer at offset 0x16C of the
object; a result of 0 sets the end flag. When no dialog matches,
`Can't find DIALOG %s` is formatted with the argument through 0001:27C8,
passed to 0002:A742 (not read here) with two zeros, and the end flag is
set. Either way the line feed is put back and p moves past it and past
spaces, tabs and line feeds.

`DISKSPACE_LT` (0004:5271). p moves past spaces and tabs. A value V is read
through 0004:6F5E, then a flag number n through 0004:6FBE. When there is no
value, `DISKSPACE_LT command w/o value`, and when the flag word is not
`FLAG`, `DISKSPACE_LT command w/o flag`, goes to 0002:A742 with two zeros
(0004:50E4), and the loop goes on from p. Otherwise 0004:D038 is called with
the first byte of the text at the far pointer at offset 0x12A of the object.
Its 32-bit result shifted right by 10 (0001:4F4E) is compared with V as
unsigned 32-bit numbers, and when it is lower, the word for flag n is set
to 1. Otherwise nothing is written; the command never clears a flag.

`WINDISKSPACE_LT` (0004:52FF) does the same with its own two messages,
`WINDISKSPACE_LT command w/o value` and `WINDISKSPACE_LT command w/o
flag`, and with the first byte of the Windows directory from KERNEL.134
into 0x104 bytes at DS:01FA.

0004:D038 takes a drive letter. When the word at offset 0xAE of the object
whose far pointer is at DS:0FB4 is nonzero, it returns 0xFFFFFFFF.
Otherwise it upper-cases the letter through 0001:2374, subtracts 0x40, and
calls 0001:4CE8, which issues DOS function 36h (through KERNEL.102 when bit
0 of CS:0010 is set, otherwise interrupt 21h) and, unless AX returns
0xFFFF, stores the total clusters, free clusters, sectors per cluster and
bytes per sector. Its result is not tested, so for a drive DOS rejects the
four words are what the stack held. D038 returns free clusters times
sectors per cluster (16 by 16 bits) times bytes per sector through
0001:4DF0, the low 32 bits of the product.

`TOGGLEGROUPON` (0004:536B). p moves past spaces and tabs and a value is
read through 0004:6F5E; with none, `TOGGLEGROUPON command w/o value` goes
to 0002:A742 as above. Otherwise 0003:AEAA (not read here) is called with
offset 0x22 of the object and the 32-bit value.

Shipped file. Each of the six `DIALOG` arguments in the shipped script,
four on their own lines and two after `FLAG`, is the whole name of one of
the eight dialogs in its `[Dialogs]` section. The two `DISKSPACE_LT` lines
give values 3000 and 37000 and set flags 10 and 20, which the following
`FLAG10` and `FLAG20` lines test.

## Interpretation

`FLAGn command` runs the command only when flag n is set, so flags chain:
`FLAG1 FLAG2 GOTO x` needs both. `DISKSPACE_LT V FLAGn` sets flag n when
the destination drive, named by the first letter of the destination
directory, has fewer than V kilobytes free; `WINDISKSPACE_LT` asks the same
of the Windows drive. Both answer "enough space" when the word at 0xAE of
the object at DS:0FB4 is set. Free space above 4 GiB wraps. `DIALOG name`
shows the first dialog whose name starts with the argument, letter case
ignored, and a dialog result of 0 or a missing dialog ends the script.

## Alternatives

- `FLAG` lines are assignments that set a flag: ruled out; the case only
  tests the flag word.
- A missing value or flag for `DISKSPACE_LT` ends the script: ruled out;
  the message is shown and the loop goes on.

## How to reproduce

Expand `_SETUP.EXE` from `CD:SETUP.SOL` as FND-RES-032 says. Disassemble
segment 4 as 16-bit code with relocation targets marked at 0004:3EC4..3FE2,
0004:3FE3..4085, 0004:50E4..5116, 0004:5271..53BF, 0004:6E98..7042 and
0004:D038..D07D; segment 1 at 0001:2374, 0001:3E34, 0001:4CE8, 0001:4DF0
and 0001:4F4E; read the strings named above and the 256 bytes at DS:111F of
segment 9. Walk the shipped `SIERRA.INF` for `DIALOG` and `DISKSPACE_LT`
arguments and the `[Dialogs]` names as FND-RES-033's loader splits them;
report counts and names only. Keep listings in ignored local storage.
