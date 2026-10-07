---
id: FND-RES-053
title: INST.EXE loads every message and help text from install.txt, install.hlp and the other .txt and .hlp files into one keyed dictionary marked by double-backslash lines
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 20F3:2D61..20F3:2DA5
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:000C..2852:004D
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:004E..2852:00BC
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:00BD..2852:048D
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:048E..2852:0577
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:0578..2852:0A8A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:0A8B..2852:0B70
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:0B71..2852:0C2C
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2852:0D33..2852:0DCA
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 27C0:0510..27C0:0533
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2FD0:04B0..2FD0:05EC
  - build: BLD-GOG-EN
    file: CD:INSTALL.TXT
    offset: 0x00000000..0x00002575
  - build: BLD-GOG-EN
    file: CD:INSTALL.HLP
    offset: 0x00000000..0x000054F3
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; Python complete line and key classification of the two text files
environment: null
---

## Observation

The unpacked file and its notation are as FND-RES-054 gives them. Run-time
routines named below with "(not read)" are known only by their arguments.

The object at DS:53AC. 1E1A:0AA0 builds it through 2852:004E, which sets a
far pointer array at +0, its count at +4 to 0, two strings at +6 and +0xA,
and the word at +0xE to 0. Main, at 20F3:2D61, calls 2852:00BD with the
object and `argv[0]` before it calls vtable entry +0x1C of the installer
object (FND-RES-054).

The loader, 2852:00BD (this, path):

- It splits the path through 1000:46ED (not read), makes its drive current
  through 1000:0900 (not read) and its directory current through 1000:02C2.
- When `getenv("LANGUAGE")` (DS:2F0F, DS:2F18, through 1000:5063) is not
  null, it copies the value into DS:5557 with no bound and adds `\`
  (DS:2F21) when the last byte is not one. Otherwise DS:5557 is left as it
  is; no other write to it was looked for.
- It forms DS:5557 followed by `install.sip` (DS:2F23). When find-first
  (1000:45DF) finds it, it opens it through 30C9:018F and keeps it at +0x10.
- It passes DS:5557 followed by `install.txt` (DS:2F2F), then DS:5557
  followed by `install.hlp` (DS:2F3B), to the parser 2852:0578.
- It runs find-first and find-next (1000:4612) over DS:5557 followed by
  `*.txt` (DS:2F47) with attribute 0. Each name found is compared through
  1000:63B6, without case, with DS:5557 followed by `install.txt` (DS:2F4D),
  and when they differ the bare name, without DS:5557, goes to the parser.
  It does the same for `*.hlp` (DS:2F59) against `install.hlp` (DS:2F5F).
- It sorts the array through 1000:5C24 (qsort shape) with 4-byte elements and
  the comparator 2852:000C, sets +0xE to 1 and closes the archive at +0x10
  when there is one.

2852:000C compares the keys of two entries through 1000:63B6.

The parser, 2852:0578 (this, name):

- It opens the name through 1000:48EF (`fopen` shape) with `rt` (DS:2F6B);
  when that fails it looks for the name in the archive at +0x10 through
  30C9:0394 and reads the member through 3216:04A4 and 3216:08E3 (not read)
  in a second copy of the steps below.
- It reads through 1000:4543 (`fgets` shape) into a 100-byte buffer, so one
  call returns at most 99 bytes. Calls are skipped until one starts with two
  backslashes (1000:643A comparing 2 bytes with DS:2F6E).
- At such a call, the rest after the two backslashes is the key, trimmed
  through 27C0:0510.
- The text is every following call up to the next one that starts with two
  backslashes or the end of the file. When a call's result ends in `\`
  followed by LF, those two bytes are removed. The results are appended in
  order, and one final LF is removed from the whole.
- It passes the key and the text to 2852:048E, then calls 263F:0009 (not
  read) with 5, and carries on at the marker line that ended the text.

27C0:0510 calls 2FD0:05C0 with the set space, tab, LF and CR (DS:2E71).
2FD0:05C0 calls 2FD0:04B0, which moves past every leading byte in the set
(found through 1000:6295) and copies the rest to the start of the string
through 1000:6302, then 2FD0:052D, which writes NUL over each trailing byte in
the set, walking back from the last byte and stopping before the first.

2852:048E (this, key, text) looks the key up through 2852:0A8B. When there is
an entry, 2852:0D33 sizes its text buffer at +4 to the new text's length plus
one (realloc through 1000:307F, or malloc through 1000:2F18 when it is null),
calls 2E08:0034 with line 0x139 when that fails, and copies the new text in
through 1000:6302. Otherwise it makes an entry through 2852:0C71 (not read)
with the serial DS:2F00, which it then raises, and appends it to the array.

2852:0A8B (this, key) searches the array with the comparator 2852:000C:
through 1000:41E0 (bsearch shape) when +0xE is not 0, and through 1000:5161
(lfind shape) when it is.

2852:0B71 (result, this, key, flag) looks the key up through 2852:0A8B and
returns a copy of the entry's text. When there is none and the flag is not 0,
it formats `FATAL ERROR: Can't find text entry for '%s'` (DS:2F7B) with the
key and calls 2E08:0034 (not read) with it, the file name `textdict.cpp`
(DS:2FF2) and the line 0x115. With no entry it returns the empty string at
DS:3004.

The two files, read whole from the owned disc and matching BLD-GOG-EN's XXH3
values (`CD:INSTALL.TXT` 88ce30e3c64583987624bd608afc1d06,
`CD:INSTALL.HLP` 7dd660f1c45378e01d06ae7357b95375), split at CRLF with no
bare CR or LF:

| File | Lines | Marker lines | Keys after trimming, without case | Lines ending in `\` | Lines over 98 bytes | Tabs | 0x1A |
|---|---|---|---|---|---|---|---|
| CD:INSTALL.TXT | 359 | 138 | 138 | 21 | 4 | 0 | 1, on line 358 |
| CD:INSTALL.HLP | 753 | 157 | 147 | 0 | 0 | 40 | 1, on line 752 |

In both, line 1 is a marker line, and the 0x1A line follows the last marker
line, inside its text. The two files share no key. Every key INST.EXE names in
the code read so far (FND-RES-055, FND-RES-047, FND-RES-049, FND-RES-051) is
in INSTALL.TXT, and none is in INSTALL.HLP. One HLP marker line ends in a
space. For each of the four long TXT lines, the 99-byte pieces `fgets`
returns neither start with two backslashes nor end in `\`.

## Interpretation

INST.EXE keeps all its texts in one dictionary. A line starting with `\\`
starts an entry, the rest of that line is its key, and the lines up to the
next such line are its text, joined with LF and with the last LF dropped; a
line ending in `\` is joined to the next one with no line break. Keys lose
leading and trailing spaces, tabs, CR and LF, and match without case. A key
loaded again replaces the earlier text, so a later file overrides an earlier
one: `install.txt` first, then `install.hlp`, then the other `.txt` and
`.hlp` files in the order DOS finds them. With `LANGUAGE` set, the two named
files come from that directory, while the other files found there are opened
by bare name from the program's directory, and the exclusion compares a bare
name with a path, so `install.txt` and `install.hlp` are loaded again; that
changes no text. A file that cannot be opened is looked for in `install.sip`.
INSTALL.HLP holds no key the code read so far uses; who looks up its keys is
open. The texts in INSTALL.TXT and INSTALL.HLP keep their tabs, and the last
entry of each holds the 0x1A line unless the text-mode `fgets` path stops at
it.

## Alternatives

- The double-backslash lines are section headings or links inside one help
  text: ruled out; each starts a separate keyed entry.
- A key keeps its trailing space: ruled out; 2FD0:052D clears trailing space,
  tab, CR and LF.
- The first of two entries with one key wins: ruled out; 2852:0D33 replaces
  the stored text with the later one.
- INSTALL.TXT is free text for the player to read: ruled out; it is the
  installer's message dictionary.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-054 gives. Read the strings at DS:2E71,
DS:2F0F to DS:2F77, DS:2F7B, DS:2FF2 and DS:3004. Disassemble the ranges in
Locations as 16-bit code with the load image at segment 0x1000 and relocation
targets marked. Split each text file at CRLF, take the lines starting with two
backslashes, trim space, tab, CR and LF from the rest and lower-case it, and
cut each line plus LF into 99-byte pieces. Keep listings in ignored local
storage.
