---
id: FND-RES-040
title: _SETUP.EXE's PICKDEST builds the destination from SierraDir and DirName, WRITE and APPEND write one line to a file, RUN ignores two words, and COPY takes no arguments
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
  - build: BLD-GOG-EN
    file: CD:LANGUAGE.INF
    offset: 0x00..0x787
tool: Python decoder of RULE-RES-005; Python bounded NE relocation-chain walk; capstone 5.0.7 bounded 16-bit disassembly; Python walk of the shipped files written for this finding
environment: null
---

## Observation

Addresses, the routine 0004:3A1E, p, the object, the end and skip flags, the
target, 0004:5CF4 and the strings 0004:02EA, 0004:74BA and 0004:7BD6 (a line
feed) are as FND-RES-038 gives them, and the word reader 0004:6E98 as
FND-RES-039 gives it; the C library routines are those FND-RES-033 lists.
The object's offset 0x12A holds the destination directory and offset 0x132
the directory FND-RES-032 finds SIERRA.INF in, offset 0x148 the LANGUAGE.INF
path, each a string record whose text is the far pointer at its offset 0.
The string routines in segment 2 were read: 0002:0B9C replaces a string's
text with another's, 0002:5C42 gives the first n bytes, 0002:5CDA the
position of the last occurrence of a byte, and 0002:0EEC the position of the
first byte in a set (the last two step through text with USER.472 when the
word at DS:2690 is set). 0004:7044 reads like 0004:6E98 but takes the bytes
up to the line feed, so spaces and tabs stay in the text. Profile calls
(KERNEL.128 reads, KERNEL.129 writes) name `SIERRA.INI` without a directory.

`PICKDEST` (0004:42EF).

1. `Sierra` `SierraDir` is read from `SIERRA.INI` into the destination
   string, at most 0x104 bytes, default empty. When the read returns 0, the
   destination is formatted as `%c:\SIERRA` with the drive letter KERNEL.92
   returns for 0.
2. `Ident` `DirName` is read from the LANGUAGE.INF path into 13 bytes,
   default empty, and `\` and that name are added to the destination.
3. 0004:5EB4 (not read) is called with the object and the name.
4. A result of 2 sets the end flag. A result of 7 calls 0004:5CF4 as `GOTO`
   does, so the rest of the `PICKDEST` line is a label: found, p moves to
   it; not found, the skip flag is set with the line as the target.
5. Any other result writes, to `SIERRA.INI`, `Sierra` `SierraDir` as the
   destination's text before its last `\`, an empty value under that same
   text as key in `SierraDirs`, and `Misc` `ProductDir` as the whole
   destination. p then moves to the next line feed and past spaces, tabs
   and line feeds.

`WRITE` (0004:5150) and `APPEND` (0004:5165) call 0004:6CEE with the object,
0 or 1, and p. It copies the bytes from p to the next line feed, or to the
end of the text when there is none, into a 224-byte buffer on the stack,
with no bound on the length, and moves p past the copied bytes and the line
feed. 0001:3EEE then splits the buffer: the bytes before the first space or
tab are the file name, and a second call with an empty delimiter set gives
the rest of the line after that one byte. Each goes through 0003:C46A (not
read here). The file is opened through 0001:0856, which passes its two arguments and
0 to 0001:081C (not read), with `wt` for `WRITE` and `at` for `APPEND`. If the open fails, `Can't open `, the name and ` for
WRITE/APPEND` go to 0002:A742 with two zeros, and the script goes on.
Otherwise the text is written through 0001:2538, then a line feed through
0001:1582, and the file is closed through 0001:071A.

`RUN` (0004:3EA4) calls 0004:7126 with the object and p. That reads a word
through 0004:6E98; when it equals `NOWAIT` with case ignored (USER.471), a
wait flag that starts at 1 is cleared and the next word read. The value of
that word is taken through 0001:21D2 and dropped. One more word is read and
dropped, and the rest of the line, through 0004:7044, is the command line.
When the part of the command line before its first space or tab has a `\`,
the text before its last `\` is upper-cased (USER.431), 0001:4902 is
called with a 0x104-byte buffer, and 0001:4756 with the text's first byte
less 0x40 and 0001:46C6 with the text; these three were not read, and
their calls fit saving the current directory and changing the drive and
directory. Without the
wait flag the command line goes to KERNEL.166 (WinExec) with 1, and a
result of 32 or more is success. With it, 0004:D3D8 (not read) is called
with the command line and 1, and its result decides. When they were called, 0001:4756 and 0001:46C6 are called again with the
buffer's first byte, upper-cased, less 0x40 and with the buffer. On failure 0004:C70A (not
read) is called with the far pointer at offset 0x16C of the object, 0xBB9,
0xBBA, 0, 0x10 and the command line. 0004:7126 always returns 1, so the
end flag that `RUN` sets on a result of 0 is never set.

`COPY` (0004:4086) reads `Setup` `AnimationDLL` into 13 bytes, default
empty, from the text at offset 0x132 with `\SIERRA.INF` added. It passes
the destination and the text at 0x132 through 0003:C46A, builds two lists
through 0003:AF90 from offsets 0x22 and 0x0A of the object, and calls
0004:626C with the object, 1, offset 0x3A and both lists. The result, the
`AnimationDLL` value, the far pointer at 0x16C and copies of both
directories go to 0003:4ECC and then 0002:30D8 (neither read here); a
result of 2 from 0002:30D8 sets the end flag. The lists are freed through
the second entry of each one's table, and USER.124 is called with the
window handle at offset 0x14 of the object returned by the routine at
offset 0x6C of the table of the object at DS:0FB4 (or of null when that
pointer is null). p moves past spaces, line feeds and tabs (0004:02D6)
only, so the rest of a `COPY` line would be read as the next command.

Shipped files. The shipped script's `PICKDEST` line names the label `End`,
which exists in exact case. Its `run` line has four words: a number and
two paths, so the command line is the last path. Its `COPY` line has
nothing after the name. Its 16 `APPEND` lines are at most 55 bytes long.
The shipped `SIERRA.INF` has no `AnimationDLL` key, and the shipped
`LANGUAGE.INF` sets `Ident` `DirName` to `CONQUER`.

## Interpretation

`PICKDEST` proposes `<SierraDir>\<DirName>`, with `SierraDir` remembered in
`SIERRA.INI` from earlier Sierra installs and `<drive>:\SIERRA` the first
time, so the shipped default is `\SIERRA\CONQUER` on the drive KERNEL.92
returns; the destination routine's result 7 jumps to the line's label, which
in the shipped script is `End`. After a choice it records the parent
directory and the product directory in `SIERRA.INI`. `WRITE file text`
replaces a file with one line and `APPEND file text` adds one. `RUN
[NOWAIT] n x command` starts the command, waiting unless `NOWAIT` is given, and
appears to run it from its own directory (the three library calls above); n and x are not used. `COPY` runs the copy of the
`[Files]` and `[Archives]` data to the destination.

## Alternatives

- `RUN` can end the script: ruled out; its routine returns 1 on every path.
- `RUN`'s second and third words are the command: ruled out; both strings
  are replaced before use.

## How to reproduce

Expand `_SETUP.EXE` from `CD:SETUP.SOL` as FND-RES-032 says. Disassemble
segment 4 as 16-bit code with relocation targets marked at 0004:4086..452B,
0004:5150..516D, 0004:6CEE..6E94, 0004:7044..7125 and 0004:7126..7387,
segment 2 at 0002:0B48, 0002:0B9C, 0002:0EEC, 0002:5C42 and 0002:5CDA, and
segment 1 at 0001:0856, 0001:071A, 0001:1582 and 0001:2538; read the
strings named above. Walk the shipped `SIERRA.INF` script lines and the
`Ident` section of `LANGUAGE.INF`; report shapes, lengths and the key's
value only. Keep listings in ignored local storage.
