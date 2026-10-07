---
id: FND-RES-038
title: _SETUP.EXE runs the [Script] text line by line, matching 37 command names as prefixes, and ends silently at an unknown command
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

Addresses are `segment:offset` in `_SETUP.EXE` as FND-RES-032 identifies and
expands it, and the C library routines are those FND-RES-033 lists. The
routine read is 0004:3A1E..0004:5CBA with its jump table at 0004:5C55, and
the helpers 0004:5CBE and 0004:5CF4. It takes a far pointer to the loader's
object, whose offset 4 holds the far pointer to the script text that
FND-RES-033 builds from `[Script]`, and a far pointer to a file name. The
strings below are in segment 4: 0004:02EA is space and tab, 0004:02EE is
comma, space and tab, 0004:74BA is space, tab and line feed, 0004:7706 is
line feed, space and tab, and 0004:759C is a line feed.

Setup. The routine fills a table of 37 far pointers to command names, in
this order (index in brackets): `RUN` [0], `DIALOG` [1], `FLAG` [2], `COPY`
[3], `PICKDEST` [4], `END` [5], `GOTO` [6], `:` [7], `ADDPROGMANGROUP` [8],
`ADDPROGMANITEM` [9], `ADDTOINI` [10], `TESTMIDIEX` [11], `RESTARTWINDOWS`
[12], `REBOOTSYSTEM` [13], `VERSIONCHECK` [14], `RESETFLAGS` [15],
`TOGGLEON` [16], `DATECHECK` [17], `WINGPROFILE` [18], `WIN32CHECK` [19],
`NOTWINNT` [20], `REGISTER` [21], `WRITE` [22], `APPEND` [23],
`PHYSICALMEM_LT` [24], `EXIST` [25], `DISKSPACE_LT` [26], `WINDISKSPACE_LT`
[27], `TOGGLEGROUPON` [28], `COLORS_NEQ` [29], `DOWIN95REGISTRY` [30],
`INSTALLDIRECTX` [31], `LANGUAGE_EQ` [32], `DELETEFILE` [33],
`WIN95CD_NOTOPTIMAL` [34], `ONWIN95ONLY` [35], `RENAMEFILE` [36]. No name is
a prefix of a later one. It reads key `Debug` of section `Setup` from the
file it was given through the profile routine (KERNEL.128), default empty,
into 40 bytes; debug mode is on when 0001:9812 reports the text equal to
`TRUE`, so letter case is ignored. A skip flag and an end flag start at 0.

Loop. The position p starts at the script text, moved past bytes of
0004:74BA. Then, each time round:

1. A NUL at p ends the run through 0004:5CBE (below).
2. Each name in table order is compared with the text at p over the name's
   length through 0001:3D4C, so letter case of `A` to `Z` is ignored and
   the name need not be followed by a delimiter (`Flag3` and `run` match).
   The first match is taken.
3. No match: the script text is freed through 0001:1CF2, offset 4 of the
   object is set to null, and the routine returns. Nothing is shown.
4. p moves past the name and past bytes of 0004:02EA.
5. With the skip flag set and any command but `:`, p moves to the next line
   feed through 0001:3CD4, whose result is not tested for null, and past
   bytes of 0004:7706, and the loop goes back to step 1.
6. With the skip flag set and `:`, the target (below) is compared with the
   text at p through 0001:2196, exactly, over the number of target bytes
   before its line feed; equal clears the skip flag. Either way `:` goes on
   to step 7.
7. In debug mode, a message box (USER.1, style 1: OK and Cancel) titled
   `Script Debug` shows `Parsing--> `, the command name, a line feed,
   ` Args--> ` and the rest of the line. Cancel (2) turns debug mode off.
8. The index selects one of 37 cases through the table at 0004:5C55; the
   index is below 37 by step 2. After the case, a set end flag ends the run
   through 0004:5CBE; otherwise the loop goes back to step 1 without moving
   p past white space, so each case moves p on itself.

0004:5CBE sets the word at offset 8 of the object to 0, frees the script
text and sets offset 4 to null; the routine then returns as in step 3.

Cases read here:

- `RUN` (0004:3EA4) calls 0004:7126 with the object and p; a result of 0
  sets the end flag. 0004:7126 was not read.
- `END` (0004:3EBB) sets the end flag.
- `GOTO` (0004:452C) calls 0004:5CF4. That moves p past bytes of 0004:02EA,
  writes a NUL over the next line feed without testing for null, and
  compares the rest of the line with the text of each label record held at
  offset 0x120 of the object (a count at offset 0x124), in order, through
  0001:9812. On a match p becomes the record's stored position, the line
  feed is put back, and it returns 1. Otherwise the target becomes the
  argument's start, the line feed is put back, p moves past it and bytes of
  0004:02EA, and it returns 0. `GOTO` sets the skip flag when the result is
  0 and clears it otherwise.
- `:` (0004:4550) cuts the line at its line feed, allocates a 16-byte
  record through 0001:204C, stores in it a string of the rest of the line,
  puts the line feed back, moves p past it and bytes of 0004:74BA, stores p
  at offset 0x0C of the record, and passes the record and the count at
  offset 0x124 to 0001:BB7C with the list at offset 0x11C. A failed
  allocation leaves a null record, whose string is then built at offset 4
  of null. A label passed twice is stored twice; the search takes the
  first.
- `ADDPROGMANGROUP` (0004:4641): when the word at offset 0x12E of the
  object is set, the file name at offset 0x148 is first rebuilt from the
  text at offset 0x12A and `\LANGUAGE.INF`, as FND-RES-032 records. The
  argument, from p past bytes of 0004:02EA to a NUL written over the next
  line feed (not tested for null), is the key read at 0004:472C from
  section `Strings` of that file, default `Sierra`, into 0x50 bytes.
- `ADDPROGMANITEM` (0004:4767): after bytes of 0004:02EA, each `/` starts a
  two-byte switch; `/f` or `/F` sets a flag, any other letter is passed
  over, and bytes of 0004:02EA after the switch are skipped. Field 1 runs
  to the first comma. Field 2 starts after that comma and every following
  comma, space and tab (0004:02EE), and ends at the next comma or the line
  feed. Field 3, present only when that comma exists, starts after it and
  bytes of 0004:02EE and ends at the next comma. Field 4, present only when
  that comma exists, starts after it and bytes of 0004:02EA and runs to the
  line feed. Fields 1, 3 and 4 go to 0003:C46A with the object (not read
  here). Field 2 is the key read at 0004:496B from section `Strings` of the
  file at offset 0x148, default empty, into 0x50 bytes.

The other 31 cases were not read.

Shipped files. Of the 73 lines in the `[Script]` section of the shipped
`SIERRA.INF`, 46 are kept by the loader (the others start with `;`), and
each of the 46 matches a command name: `:` 5, `FLAG` 10, `PICKDEST` 1,
`DIALOG` 3, `DISKSPACE_LT` 2, `GOTO` 1, `TOGGLEGROUPON` 1, `COPY` 1,
`APPEND` 16, `ADDPROGMANGROUP` 1, `ADDPROGMANITEM` 3, `RUN` 1, `END` 1. The
labels are `Begin`, `MaxInst`, `InstCont`, `ADDICONS` and `End`, the last
two lines are `:End` and `END`, and the `GOTO` targets, inside `FLAG` lines
and in the one `GOTO` line, are `END`, `MaxInst` and `InstCont`. The
`ADDPROGMANGROUP` key is not a key of the shipped `LANGUAGE.INF`'s
`Strings` section. Each of the three `ADDPROGMANITEM` lines has four fields
and its second field is a key of that section.

## Interpretation

The `[Script]` text is a list of commands, one per line, each starting with
one of 37 names in any letter case. A line that starts with no known name
ends the script at once, without a message. `GOTO` jumps back to a label
already passed, compared without regard to case, or skips forward to the
first label line that starts with the target in exact case, recording
labels it passes. A forward `GOTO` whose label is never found runs to the
end of the text, which ends the run the same way `END` does. In the
shipped script a forward `GOTO END` does not match `:End`, so it skips the
rest of the script and ends there, the same outcome as reaching `END`.

The `Strings` key of `ADDPROGMANGROUP` is its whole argument, and of
`ADDPROGMANITEM` its second field; the shipped group name therefore falls
back to `Sierra`.

## Alternatives

- An unknown command is skipped or reported: ruled out; the run ends
  silently at step 3.
- Labels match in any letter case in both directions: ruled out for
  forward jumps, which use the exact comparison 0001:2196.

## How to reproduce

Expand `_SETUP.EXE` from `CD:SETUP.SOL` as FND-RES-032 says. Disassemble
segment 4 as 16-bit code with relocation targets marked over
0004:3A1E..0004:5E01, read the 37 words at 0004:5C55 and the strings named
above, and disassemble 0001:2196. Walk the `[Script]` section of the shipped
`SIERRA.INF` as FND-RES-033's loader keeps it, match each line against the
names in order as step 2 does, and check the two keys against the shipped
`LANGUAGE.INF`; report counts and names only. Keep listings in ignored
local storage.
