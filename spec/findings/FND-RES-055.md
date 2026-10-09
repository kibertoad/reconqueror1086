---
id: FND-RES-055
title: INST.EXE runs install.scr line by line, with labels, goto, nine percent parameters, a table of named commands and any other line run as a program
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:01AE..1C17:0401
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0401..1C17:0504
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0504..1C17:0745
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0745..1C17:0818
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0818..1C17:0928
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0928..1C17:0B35
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0B35..1C17:0CA2
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:1C09..1C17:1C16
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:6295..1000:62D2
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:632B..1000:6370
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:63F7..1000:6416
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:0CAE..3583:0D75
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:0E2A..3583:0F32
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; Python byte searches of its load image and a Python line count of the shipped INSTALL.SCR
environment: null
---

## Observation

The unpacked file, its notation, the object at DS:55D8 and the script buffer
are as FND-RES-066 and FND-RES-045 give them: the whole of `install.scr` is
at the far pointer DS:534A, its length in DS:534E, with a 0 stored at the
byte after the last one read. Vtable entry +0x30 of the object is 1C17:01AE.

Run-time routines read here: 1000:6295 returns a far pointer to the first
byte equal to its character argument, testing equality before the end, or
null at the first NUL; 1000:63F7 returns the length up to the first NUL, and
0 for a null pointer; 1000:632B counts the leading bytes of its first string
that are not in its second. 1000:63B6 compares without case and 1000:643A
compares a given number of bytes keeping case, as FND-RES-066 gives;
1000:62D2 compares two strings keeping case.

Run loop, 1C17:01AE (this):

- When DS:534A is null it returns at once.
- It allocates 0x7D0 bytes through 1000:0D4B for an output line and, when
  that fails, calls 1000:2557 with the assertion format (DS:0E2A), `cmdLine`,
  `code\command.cpp` and 0x9D.
- It calls vtable entry +0x70 (1C17:0401, below), creates a window through
  2973:0039 into DS:5334, calls 2973:01A4 on it, and sets the stop word
  DS:0CAC to 0.
- The current pointer DS:5338 starts at DS:534A. The loop runs while DS:0CAC
  is 0 and the offset of DS:5338 is below the offset of DS:534A plus DS:534E,
  an unsigned 16-bit comparison of offsets only.
- Each pass replaces the first CR from DS:5338 with a NUL when there is one
  before a NUL, then does the same for the first LF. The next pointer DS:533C
  is DS:5338 plus the length of what is left, plus one.
- A line whose first byte is `:` is a label. The far pointer to the byte after
  the `:` is stored at DS:5294 plus 4 times the word DS:0CAA, and DS:0CAA is
  raised by one. A byte search of the load image for 0xAA 0x0C finds only the
  read at 1C17:030A, the raise at 1C17:031F and a compare at 1C17:0C87, so the
  count is never reset or bounded; the window pointer DS:5334 lies 40 entries
  after DS:5294. When the byte at DS:0C32 is not 0 and the label equals the
  string at DS:0C32 through 1000:62D2, that string is emptied (copy of DS:0E6B,
  an empty string). A label line is never expanded or run.
- Any other line, when DS:0C32 is empty, is expanded by 1C17:0818 into the
  output buffer with the parameter table at this+0x214, and when the result is
  not empty, passed to 1C17:0504. While DS:0C32 is not empty such lines are
  skipped.
- DS:5338 then takes DS:533C.
- After the loop it frees the script buffer, the output buffer and the
  window (2973:047E, then 1000:0328).

With CR LF line ends, each line takes two passes: one that runs the text up
to the CR, and one that starts at the LF, finds the next line's CR and this
LF, and leaves an empty line. A file with LF alone has one pass per line, and
each pass ends with a NUL the CR of a later line.

Parameters, 1C17:0401 (this), fill nine far pointers at this+0x214, of which
it writes the first seven:

| Parameter | What it points to |
|---|---|
| `%1` | a copy, through 1000:66B1, of the one-character string formed through 1000:6199 with `%c` from the byte at +0x1E6 |
| `%2` | the same from the byte at +0x23E |
| `%3` | the text of the string object at +0x1DE, through 1BA0:0502 |
| `%4` | the text of the string object at +0x194 |
| `%5` | a copy of the string at DS:5557 |
| `%6` | the text of the string object at +0x1AA |
| `%7` | the bytes at +0x1AE |

`%8` and `%9` read the far pointers at +0x230 and +0x234, which 0401 does not
write.

Expansion, 1C17:0818 (line, out, table): bytes other than `%` are copied. At
a `%` it takes the next byte; when that is `1` to `9` it copies the string the
table entry for that digit points to; otherwise it drops that byte and copies
the byte after it. The loop tests for the end only before each step, so a `%`
whose next byte is the line's NUL copies the byte after that NUL and goes on
reading past it. It
ends the output with a NUL. Nothing bounds the output's length.

Commands. The table at DS:0CAE has 10-byte entries of a far name pointer, a
far handler pointer and a word, ended by a null name at DS:0D6C:

| Name | Handler | Word |
|---|---|---|
| `;` | null | 0 |
| `rem` | null | 0 |
| `/*` | null | 0 |
| `//` | null | 0 |
| `echo` | 1C17:0928 | 0 |
| `alert` | 1C17:0CA2 | 0 |
| `copy` | 1C17:19B8 | 1 |
| `space` | 1C17:0CBF | 1 |
| `godir` | 1C17:0E4A | 1 |
| `exists` | 1C17:0F7C | 1 |
| `pause` | 1C17:0B6B | 0 |
| `goto` | 1C17:0C14 | 1 |
| `pick` | 1C17:0D8F | 1 |
| `end` | 1C17:0B55 | 1 |
| `clear` | 1C17:0B35 | 1 |
| `cls` | 1C17:0B35 | 1 |
| `testdir` | 1C17:1AC6 | 1 |
| `del` | 1C17:1C17 | 1 |
| `if` | 1C17:1C63 | 1 |

Dispatch, 1C17:0504 (line), stores the line's far pointer in DS:5342 and
reads its first word through 1000:620F with the format `%s ` (DS:0ECE) into a
44-byte stack buffer, with no width. It walks the table in order and compares
each name with that word through 1000:63B6.

- At the first match with a null handler it returns.
- At the first match with a handler, 1C17:08C1 moves from the start of the
  line to its first space, tab or NUL, one more byte is skipped when that byte
  is not the NUL, and when the entry's word is not 0, 1C17:08F9 skips spaces and
  tabs. The handler is called with the far pointer to what remains.
- With no match it calls 1C17:05D5 with the line.

Program runner, 1C17:05D5 (line), builds an object with the vtable word
DS:1202, a string made from the line, and calls the vtable's first entry with
them. The result picks a message key: 1 `redirOpenErr` (DS:0ED2), 2 `execErr`
(DS:0EED), 3 `redirCloseErr` (DS:0EDF); any other result shows nothing. For a
key it fetches text through 2852:0B71 with the text-dictionary object at
DS:53AC and the key (FND-RES-053), and calls 1C17:0745 with that text and the line. The word at [bp-0x14] of
the object's frame goes to DS:5340 on every path.

Message box, 1C17:0745 (format, ...), formats into a 0xD6-byte stack buffer
through 1000:61BB with no bound, fetches the text for `enterEsc` (DS:0EF5)
the same way, and calls 263F:02BF with both, the bytes at DS:0EFE as the
allowed keys and two colour bytes from the table at DS:0C2E. When the key it
returns is not 0x0D it calls 1000:085B with 0.

`echo`, 1C17:0928 (text):

- With no `>` in the text it writes the text through 2973:092F to the window
  with the format `%s\n` (DS:0F01).
- Otherwise it stores a NUL over the first `>`. When the next byte is `>`
  too, it moves past it and the open mode is 0x4002, with 0x0800 added when
  1000:4113 (not read) returns 0 for the file name with the argument 0, and
  0x0300 added when it returns anything else. A single `>` gives 0x4302. The
  file name is what follows, with spaces and tabs skipped by 1C17:08F9. It
  opens through 1000:53CF with that mode and the permission word 0x0180.
- It writes the text, including any spaces before the `>`, through
  1000:6B2F. When the byte before the text's NUL is not LF it writes `\n`
  (DS:0F19); for an empty text that byte is the one before the text. It then
  closes the file through 1000:4271.
- A negative handle from the open, a write that returns other than the
  text's length, or a failed `\n` write each shows `outputErr` (DS:0F05,
  DS:0F0F, DS:0F1B) through 1C17:0745 with the file name and returns; after
  a failed write the file is not closed.

`clear` and `cls`, 1C17:0B35, call 2973:073E on the window. `end`, 1C17:0B55,
sets DS:0CAC to 1. The search for 0xAC 0x0C also finds 1C17:0282 and
1C17:03A4 in the loop and a store of 1 at 1C17:1C0F, in the `testdir` routine
1C17:1AC6, made when 263F:004D (not read) returned nonzero.

`pause`, 1C17:0B6B (text): with an empty text it passes the text fetched for
`pauseMsg` (DS:0F25), otherwise the text itself, to 2973:092F as its format
string, where `echo` passes `%s\n`; then it calls 2FA4:0005 (not read) and
writes `\n` (DS:0F2E).

`goto`, 1C17:0C14 (text): n is the count of leading bytes of the text that
are neither space nor tab (DS:0F30). For each recorded label from the first,
when the first n bytes of the label equal those of the text through
1000:643A, DS:533C takes the label's address plus its length and the routine
returns. An empty text gives n = 0 and matches the first label recorded. With
no match it copies the whole text to DS:0C32 through 1000:6302, with no
bound.

The shipped `CD:INSTALL.SCR` has 2135 bytes in 93 lines that each end in CR
LF, followed by one byte 0x1A with no line end. Its first words are, by
count: `echo` 32, an empty line 18, a label 12, `end` 5, `clear` 4, `godir`
4, `alert` 4, `rem` 3, `copy` 3, `goto` 3, `space` 2, `pick` 1, `pause` 1, and
a line holding only 0x1A twice. It uses `%1` to `%4` and `%%`. The line before
the first 0x1A line is `end`.

## Interpretation

`install.scr` is a small batch language read from top to bottom. A line
starting with `:` names a label; `goto` jumps back to the first label already
passed whose name starts with the goto's word, keeping case, or else skips
lines until a label that equals the goto's whole text exactly. Before a line
runs, `%1` to `%7` are replaced with values the installer holds; `%%` drops
both bytes and keeps the byte that follows. The first word picks a command
from the table without regard to case; `;`, `rem`, `/*` and `//` make a line
a comment; any other word runs the line as a DOS program. `end` stops the
script. A failed program or redirection shows a message that leaves the
installer unless the player presses Enter.

The shipped script reaches neither 0x1A line along straight-line flow, since
`end` comes first; a forward `goto` to a missing label would skip them too.

## Alternatives

- The script is compiled or tokenized before it runs: ruled out; each line is
  split, expanded and dispatched as the loop reaches it.
- Labels are found by a search of the whole file: ruled out; a backward
  `goto` sees only labels already passed, and a forward one skips lines until
  the label is reached.
- `%%` gives a literal `%` as in DOS batch files: ruled out; both bytes are
  dropped.
- Command names keep case: ruled out; they are compared through 1000:63B6.
- DS:53AC is a string passed with the key: ruled out. This replaces
  FND-RES-046, which said so. A read of 2852:004E and 2852:0B71
  (FND-RES-053) shows DS:53AC is the text-dictionary object that 0B71
  searches for the key.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-066 gives. Disassemble the ranges in
Locations as 16-bit code with the load image at segment 0x1000 and relocation
targets marked. Read the 10-byte entries from DS:0CAE to the null name and the
strings from DS:0E2A to DS:0F32. Search the load image for the byte pairs
0xAA 0x0C and 0xAC 0x0C and decode each hit. Split the shipped `INSTALL.SCR`
at CR LF and count the lowercased first words. Keep listings in ignored local
storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.

Grouped whole-function exclusive bounds follow the extents recorded in FND-RES-063.
