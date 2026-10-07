---
id: FND-RES-026
title: AUTOPLAY reads AUTORUN.INF and an installed LANGUAGE.INF through the Windows profile API
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x0041003C..0x00410043
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x0041029A..0x004102AC
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x004104CD..0x004104D2
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00410543..0x00410574
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00410594..0x004105DA
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x004105DC..0x00410704
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00410704..0x00410785
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00410788..0x004107DC
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x004107DC..0x004109B8
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x004109B8..0x00410B1E
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    address: 0x00410B20..0x00410C87
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    kind: file-data
    offset: 0x00005360..0x00005384
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.EXE
    kind: file-data
    offset: 0x0000538C..0x0000539A
  - build: BLD-GOG-EN
    file: CD:AUTORUN.INF
    offset: 0x00..0x2DE
  - build: BLD-GOG-EN
    file: CD:LANGUAGE.INF
    offset: 0x00..0x787
tool: capstone 5.0.7 bounded disassembly of the listed AUTOPLAY ranges; Python bounded PE import-table walk; Python bounded ISO 9660 root read
environment: null
---

## Observation

AUTOPLAY's complete length (26,624 bytes) and canonical fingerprint matched
BLD-GOG-EN before reading. FND-RES-024 gives its PE32 section mapping:
BEGTEXT (code) at 0x00410000 from file offset 0x400, DGROUP (initialized
data) at 0x00420000 from file offset 0x4E00, and `.idata` at 0x00440000 from
file offset 0x5800.

Import slots were named by walking the import directory: each descriptor's
DLL name and its import lookup table in step with its import address table,
up to each null entry, with caps of 128 descriptors and 1,024 thunks and
256-byte name reads; no cap was reached, and every descriptor has a zero time
stamp. The positive controls are slots 0x0044016C (`KERNEL32.DLL`
`GetPrivateProfileSectionA`) and 0x00440170 (`KERNEL32.DLL`
`GetPrivateProfileStringA`), which FND-RES-024's independent traversal named
the same way. The walk names the other slots this finding uses: 0x00440180
`GetUserDefaultLangID`, 0x004401A0 `SetCurrentDirectoryA`, 0x004401C4
`WinExec`, 0x00440140 `FindFirstFileA` and 0x00440144 `FindNextFileA`, all in
`KERNEL32.DLL`, and 0x00440290 `MessageBoxA` and 0x0044029C `SendMessageA` in
`USER32.DLL`. Each import is reached through a six-byte indirect jump in
BEGTEXT; the calls below are direct calls to those jumps.

The toolkit's `imports` report (executable-reader 2.3.0) and every engine
12.0.0 `pe32` report refuse this file before running a query, because the
`.bss` section has a raw-data pointer of 0 and a raw size of 0xE00. Toolkit
issue 324 records the case. The reading below is a bounded disassembly of the
listed ranges, each read from its first instruction to its return.

Startup. `fn_00410010` stores 0x004103D8 as the window procedure of the class
it registers (at 0x0041003C), creates its window and, at 0x004102A7, sends
message 0x12C to that window with `SendMessageA` before its message loop.
Earlier exits of `fn_00410010`, at 0x004100A5, 0x004100B2 and 0x004100CC,
skip the send; their conditions are not read here. The window procedure at
0x004103D8 compares the message number and, for 0x12C, calls `fn_00410594`
at 0x004104CD. Neither 0x004103D8 nor the routine at 0x00410318 is a start in
the committed AUTOPLAY inventory.

`fn_00410594` calls `fn_004107DC`. When that returns nonzero, it calls
`fn_004105DC` with the prefix `run`, and on a nonzero result `fn_00410704`.
When `fn_004107DC` returns 0, it calls `fn_004105DC` with the prefix
`install`, and on a nonzero result `fn_00410788`.

`fn_004107DC` passes the address of the DGROUP string `.\autorun.inf` (file
offset 0x538C) as the file name of three `GetPrivateProfileStringA` calls in
section `Sierra`, each with an empty default: key `ExeName` into a 25-byte
buffer, `Title` into 50 bytes and `DirName` into 20 bytes. A return of 0 from
any of them shows a message box through `fn_004102E8` and the routine goes
on. It then reads key `SierraDir` of section `Sierra` from the file name
`SIERRA.INI`, with no directory, into a 256-byte buffer; a return of 0 makes
it return 0. Otherwise it masks the result of `GetUserDefaultLangID` to its
low 10 bits and walks the six-byte table at file offset 0x5360 (a `UINT16LE`
language number and a `PTR32` to a section name): it keeps each entry's name
pointer, stops at a null pointer, and otherwise stops at the first equal
number. The table holds 9 `english`, 12 `french`, 10 `spanish`, 7 `german`
and 16 `italian`, then a null entry. With a name, it calls
`GetPrivateProfileSectionA` on that section of `.\autorun.inf` into a
1,024-byte buffer. When there was no name or that call returned 0, it reads
key `DefaultLang` of section `Sierra` from `.\autorun.inf` into the same
buffer and uses that text as the section name instead. It appends `\` to the
`SierraDir` text and returns the result of `fn_004109B8` on it with a flag
of 1.

`fn_004109B8` appends `*.` to the directory it is given and enumerates it
through a library routine that reaches `FindFirstFileA` and `FindNextFileA`.
For each entry whose attribute byte has bit 0x10 set and whose name is
neither `.` nor `..`, it builds directory, name and `\`. When its flag is
nonzero and the name compares equal to the `DirName` text, it calls
`fn_00410B20` on that path and returns 1 if that returns nonzero. Otherwise,
or when that call returns 0, it calls itself on the path with a flag that is
1 when the name compares equal to `SIERRA` and 0 otherwise, and returns 1 if
that does. It returns 0 when the enumeration ends.

`fn_00410B20` appends `LANGUAGE.INF` to the path it is given and reads key
`Title` of section `Ident` from that file with `GetPrivateProfileStringA`,
with a null default, into a 260-byte buffer. A return of 0, or text that does
not compare equal to the `Title` read from `.\autorun.inf`, returns 0. It
then reads key `NecFiles` of section `Sierra` from `.\autorun.inf`, with an
empty default, into 255 bytes. When that returns nonzero, it cuts the text at
each comma and passes the path followed by each piece, and 0, to a library
routine at 0x00411304; a result of -1 from it returns 0, and any other result
is passed to a second library routine at 0x004114AD. Neither library routine
is read here. It then copies the path it was given into the found-directory buffer
at 0x00430734 and returns 1.

`fn_004105DC` builds keys from its prefix, `Text` and the decimal digits of
1, then 2, up to 9. For each it calls `GetPrivateProfileStringA` on the
chosen section of `.\autorun.inf`, with the default one space, into an
80-byte buffer, and stops at the first return of 0. The first value starts
the message and each later one is added after a line feed, in a 260-byte
buffer. It shows the message with `MessageBoxA`, caption `Sierra` and type 1
(OK and Cancel), and returns 1 only when the result is 1.

`fn_00410704` joins the found directory and the `ExeName` text, makes the
found directory current with `SetCurrentDirectoryA`, and calls `WinExec` on
the joined text with show value 1. `fn_00410788` calls `WinExec` on
`setup.exe` with show value 1. Both show a message box when `WinExec`
returns less than 32.

Second search. A search of every byte of the file for the little-endian
address of `.\autorun.inf` (0x0042058C) found seven hits, and each is the
immediate operand of one of the seven decoded instructions that load it for
the calls above. The address of `LANGUAGE.INF` (0x00420181), whose location
FND-RES-024 computed from the section table without disassembly, is found
once, as the operand of the instruction at 0x00410B3F, which is the positive
control. No hit lies in DGROUP or any other data. The search covers 32-bit
absolute operands and stored pointers. It does not cover addresses built by
arithmetic.

Shipped files, read from the owned disc image's ISO 9660 root (2,352-byte
raw sectors, user data at byte 16 of each) with a one-MiB size cap.
`CD:AUTORUN.INF` holds, in this order, section `autorun` with keys `OPEN`
and `ICON`; section `Sierra` with keys `Title`, `DirName`, `NecFiles`,
`ExeName` and `DefaultLang`; and sections `english`, `french` and `german`,
each with keys `runText1`, `installText1`, `installText2` and
`installText3`. The `DefaultLang` value is `english`, and the `NecFiles`
value holds two commas. All four bytes above 0x7F in the file are in values
of the `french` and `german` sections. `CD:LANGUAGE.INF` has a section
`Ident` whose `Title` and `DirName` values equal byte for byte the `Title`
and `DirName` values of the `Sierra` section of `CD:AUTORUN.INF`.

No reference to `CONQUER.INF` or `SIERRA.INF` was found in AUTOPLAY, as
FND-RES-023 records for literal bytes. The string `SIERRA` it compares
directory names with, and the file `SIERRA.INI`, are not those files.

## Interpretation

AUTORUN.INF is a Windows profile file to AUTOPLAY: every read goes through
`GetPrivateProfileStringA` or `GetPrivateProfileSectionA`, so the syntax
(sections, keys, comments, case, white space) is the operating system's,
and the game adds none. The file name has the prefix `.\`, so the file read
is the one in the process's current directory.

The LANGUAGE.INF that AUTOPLAY reads is in an installed directory found
under the `SierraDir` that `SIERRA.INI` names, not on the disc. That the
installer copies `CD:LANGUAGE.INF` there is suggested by the equal `Title`
and `DirName` values, which is circumstantial.

The high bytes in AUTORUN.INF reach the screen only through `MessageBoxA`,
with no conversion in AUTOPLAY, so the code page that interprets them is the
one Windows uses for ANSI text on the machine running the launcher.

Nine values of 79 bytes with eight line feeds would pass the 260-byte message
buffer; the shipped values are much shorter.

## Alternatives

- AUTOPLAY could have its own INF parser: ruled out for the reads listed,
  since each one is a call through a named profile import with these file
  names.
- The LANGUAGE.INF read could be the disc's copy: ruled out by the path,
  which is built from a directory found under `SierraDir`.
- AUTOPLAY could read CONQUER.INF or SIERRA.INF through a name built at run
  time: not ruled out by the searches, which cover literal names and 32-bit
  addresses only. The routines read here build no such name.

## How to reproduce

Verify AUTOPLAY's manifest size and XXH3-128. Map the sections as
FND-RES-024 does, walk the import directory with the caps above, and check
both positive-control slots. Disassemble each listed range as 32-bit code from
its first address and follow each call to its import jump. Search the whole
file for the little-endian dwords 0x0042058C and 0x00420181 and match each
hit to a decoded operand. Read `AUTORUN.INF` and `LANGUAGE.INF` from the disc
image's root directory with the stated sector layout and cap, and list section
and key names, value lengths and the sections holding bytes above 0x7F. Keep
listings and file bytes in ignored local storage.
