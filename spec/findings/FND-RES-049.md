---
id: FND-RES-049
title: INSTALL.SCR's copy command in INST.EXE extracts from drivers.sip and sierra.sip beside the source, then copies matching files, with /q, /s and a + concatenation form
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0FDD..1C17:1AC5
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:101B..3583:1159
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; Python checks of the build manifest and the shipped INSTALL.SCR
environment: null
---

## Observation

The unpacked file, its notation, the dispatch, the window output 2973:092F,
the message lookup 2852:0B71, the message box 1C17:0745, 1000:6295 and
1000:65EA are as FND-RES-055 and FND-RES-048 give them, and 2CBD:010A as
FND-RES-047 gives it. Run-time routines named below with "(not read)" are
known only by their arguments.

`copy`, 1C17:19B8 (text):

- It calls 1000:2583 (not read) and, when the 32-bit result is below 5000,
  1000:027A, which ends the run (FND-RES-047). It calls 1000:2583 again and
  sets the doubleword DS:5350 to that result minus 5000, at most 0x8000. While DS:5350 is above
  0x1000 it allocates that many bytes through 1000:0D4B into DS:5354 and,
  when that fails, lowers DS:5350 by 0x400 and tries again. When DS:5350
  starts at 0x1000 or less, no allocation is tried and DS:5354 keeps the
  value it had.
- When DS:5354 is null, it calls 263F:0133 with the text for `noMemCopy`
  (DS:1150), which ends the run.
- When the text holds a `+` it calls 1C17:11B1, otherwise 1C17:1291, and then
  frees DS:5354 through 1000:0328.

File copy, 1C17:0FDD (destination, source, append):

- It opens the source through 1000:53CF with the mode 0x8001. When that
  returns -1 it shows the text for `inputErr` (DS:101B) with the source name
  through 1C17:0745 and returns.
- When `append` is not 0 and 2E35:0318 (not read) returns nonzero for the
  destination, it opens the destination with the mode 0x8802; otherwise it
  creates it through 1000:42D2 (not read) with 0x0180. When that returns -1
  it shows `outputErr` (DS:1024) with the destination name, closes the
  source and returns.
- It reads up to the low word of DS:5350 bytes into DS:5354 through
  1000:5D1D and, while the count is not 0, writes that count through
  1000:6B2F. A write that returns another count shows `outputErr` (DS:102E)
  and ends the loop. A read that returns -1 is passed on as the count.
- It passes the source handle and a stack record to 1000:097B, and the
  destination handle and the record to 1000:1947 (neither read), then closes
  the destination and the source through 1000:4271.

Concatenation, 1C17:11B1 (text): it takes a first token through 1000:65EA
with `+` and space (DS:1038) and a second with `+`, space, tab and LF
(DS:103B). When the second is null it calls 2E08:0034 (not read) with
`Invalid format for copy concatenation command` (DS:1040), `code\command.cpp`
and 0x288. It writes the text for `concatenatingTo` (DS:107F) to the window
as a format with the second token and the first, calls 1C17:0FDD with the
first token as the destination, the second as the source and `append` 1,
and writes `\n` (DS:108F).

Copy, 1C17:1291 (text):

- It reads up to seven words with ` %s %s %s %s %s %s %s` (DS:1091): the
  source into an 82-byte buffer, the destination into an 82-byte buffer,
  and five option words into 10-byte buffers, each emptied first. It splits
  the source through 1000:46ED (not read) into drive, directory, name and
  extension buffers.
- When the destination's first byte is `/`, the destination is copied into
  the fifth option buffer and emptied. For each option word whose first
  byte is `/`, a second byte of `q` clears the report flag (set to 1 at
  the start) and `s` sets the quiet flag (0 at the start); other bytes are
  ignored. The bytes compared are lower-case only.
- When the destination is empty, the destination flag is 0 and the
  destination becomes `*.*` (DS:10A7); otherwise the flag is 1.

Archive phase. A table on the stack holds `drivers.sip` (DS:10AB),
`sierra.sip` (DS:10B7) and `audio.sip` (DS:10C2) with a count of 2, so the
third is never used. For each of the first two:

- The path is the source's drive and directory with the archive name
  (1000:6302, then 1000:6256, not read, twice). When 1000:45DF finds nothing
  for it with attribute 0, the next archive is tried.
- Otherwise the found flag is set and the archive is opened through
  30C9:018F with a null far pointer, the path, 0 and 2; a null result ends
  the archive phase.
- The source's name and extension are passed to 30C9:0CC3 with the
  archive. While it, then 30C9:0D62, returns a non-null member:
  - with the destination flag, the destination is split through 1000:46ED
    (at each archive's first member only) and its drive and directory joined and
    lower-cased; `Extracting %s%s%s to %s...` (DS:10CC, then DS:10FE) is
    written with the lower-cased source drive, directory and member and that
    directory, and 30C9:0983 is called with the archive, the member and the
    directory;
  - without it, `Extracting %s%s%s...` (DS:10E8, then DS:111A) is written
    and 30C9:09B6 is called with the archive and the member.
- The archive is closed through 30C9:02FE with 3.

File phase. 1000:45DF looks for the source with attribute 0 into a record
whose name field is at offset 0x1E. For each file found, until 1000:4612 (not
read) returns nonzero:

- The found flag is set. The source path is rebuilt through 1000:46C2 (not
  read) from the source's drive and directory and the found name, lower-cased
  and split again to give the found file's name and extension.
- The destination is split. When both its name and extension are empty,
  they become the found file's. Otherwise an empty name, or one starting
  with `*`, becomes the found file's name, and an extension equal to `.*`
  without regard to case becomes the found file's extension. The parts are
  joined through 1000:46C2 and lower-cased.
- Unless the quiet flag is set, the text for `copyingTo` (DS:1133) with the
  source and destination paths, with the destination flag, or for `copying`
  (DS:113D) with the source path, without it, is written to the window as a
  format, then `\n` (DS:1145).
- 1C17:0FDD is called with the destination path, the source path and
  `append` 0.

At the end, when the found flag is 0 and the report flag is 1, 1C17:0745
shows the text for `inputErr` (DS:1147) with the lower-cased source.

The build's manifest lists no file whose name ends in `.SIP`. Each of the
shipped `INSTALL.SCR`'s three `copy` lines has one word after `copy`, which
holds a `%` parameter, and no `+` or `/`.

## Interpretation

`copy source [destination] [/q] [/s]` copies every file matching the
source pattern to the destination, which may give a directory and a name
or extension pattern, and to the current directory under the same name when
there is none. When 1000:42D2, 1000:097B and 1000:1947 are the create, get
file-time and set file-time calls their arguments suggest, copies replace
existing files and keep the source's file times. `/s` hides the per-file messages and `/q` hides the
error when nothing matched. Before that, the installer extracts the members
matching the source's name from `drivers.sip` and `sierra.sip` beside the
source, when those archives exist; this disc has none. `copy a+b` appends
`b` to `a`. The buffer is as large as free memory allows, above 4 KB and at
most 32 KB.

## Alternatives

- The installer copies only from its archives: ruled out; the file phase
  runs after the archive phase whether or not an archive was found.
- `audio.sip` is searched too: ruled out; the loop's count is 2.
- `copy a+b` writes a new third file: ruled out; the first token is opened
  for appending and the second is read.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-054 gives. Disassemble 1C17:0FDD to
1C17:1AC5 as 16-bit code with the load image at segment 0x1000 and relocation
targets marked, and read the strings from DS:101B to DS:1159. Search the
build's manifest for paths ending in `.SIP`, and split the shipped
`INSTALL.SCR`'s `copy` lines into words. Keep listings in ignored local
storage.
