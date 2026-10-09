---
id: FND-RES-037
title: CONFIG.EXE reads a quoted script string with nine backslash escapes and at-sign substitution, and stops on any other escape
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 221F:0B5C..221F:1124
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 213C:0007..213C:0059
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 213C:00A1..213C:0164
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1000:0668..1000:0677
  - build: BLD-GOG-EN
    file: CD:INSTALL.DAT
    offset: 0x00..0xBDDB
tool: capstone 5.0.7 bounded 16-bit disassembly with relocation targets marked; Python walk of the shipped INSTALL.DAT
environment: null
---

## Observation

Addresses, the character layer and the token reader are as FND-RES-036
gives them. 213C:0007 formats its arguments, writes the text to the stream
at DS:954F and calls 1000:0668 with 1. 1000:0668 passes its argument to
1000:0611, which was not read and is taken here only as the end of the
program. 213C:00A1 writes `SCRIPT FILE: %s LINE: %lu SYNTAX ERROR:`, using
the value of `@SCRIPTFILE`, then its text in quotes and `was not
expected`, and also reaches 1000:0668. Both are called "stop" below.

221F:0B5C takes the handle, the context and an evaluate byte (at [bp+0xC]).
On its first call it allocates a 1,500-byte buffer at the far pointer
DS:2B15. It notes the line count, skips space and `/*` comments through
2D9F:046D, and calls 29BC:0008 with the handle and `"` (not read). Then,
with the output index at 0, it repeats until the index reaches 1,500:

1. A counter at [bp-0xC] is lowered by one if it is above 0.
2. One character is read with comments off, so `//` and `/*` are ordinary
   text here, and stored at the index.
3. `"` or the end of input leaves the loop. `\` is an escape (below). `@`
   is a substitution (below). Any other byte stays, and the index moves on.

Escapes. The byte after `\` is read with comments off and looked up in the
ten-entry table at 221F:10FD:

- `\"` gives `"`, `\@` gives `@`, `\a` 0x07, `\b` 0x08, `\n` 0x0A,
  `\r` 0x0D, `\t` 0x09.
- `\x` and `\X` read further characters while each is a digit or a hex
  letter. Each shifts the byte value left by 4 within 8 bits and adds the
  digit's value, so only the last two digits count. With no digit the
  value is 0. The first other character is pushed back.
- `\\` gives `\`. The exception is when the counter is above 0, the index is
  at least 1 and the byte before is `\`: then the new `\` is overwritten by
  the next character.
- Any other byte stops with the text `character following \`.

Substitution. The `@` is pushed back and 221F:0003 reads a token, with
comments on.

- Token 0 (an unrecognised name) ends the buffer with a 0 at the index and
  copies the buffer into a recursion area at the far pointer DS:2B0D. That
  area is set from DS:8208 on first use; going past DS:8208 plus 1,500
  stops with `Exceeded maximum recursion space for argument processing`.
  The name is looked up through 3044:011A, and the buffer is copied back.
  - When the evaluate byte is 0, nothing more is done.
  - When it is set and the name is unknown, the run stops with the name as
    the text.
  - When it is set and the name is known, the value is inserted by the type
    word at offset 0 of the name's record. Type 2: the 32-bit value at
    offsets 6 and 8, formatted with `%ld`. Type 3: the string at the far
    pointer at offset 0x13. Type 4: the same string, written over a `\`
    just before it. When that string is longer than one byte, its last byte
    is then removed. The counter is set to 2. Type 5: the byte at offset
    0x0A.
  - Other types insert nothing.
- Token 0x1B (`@@`) gives `@`.
- Token 0x97 (`@GETCWD`) reads its argument through 221F:1125. A drive
  letter that is not a letter, or a drive that 2DEF:0009 does not mark as
  present (bit 3 of byte 0x12), stops the run with one of two messages
  about `@GetCwd()`. Otherwise the drive's current directory, taken through
  2E86:0C89, is inserted with the same overwrite of a `\` before it.
- Any other keyword whose entry at DS:272F has class byte 1 is pushed back
  through 221F:0526. It is evaluated through 2163:06D4, and the 32-bit result
  is inserted with `%ld`.
- Any other keyword inserts its own name from that table.

At the end, an index of 1,500 stops with `String starting on line %lu
exceeds maximum length of %d bytes`. The end of input stops with
`Unterminated quoted string starting on line %lu`. Otherwise a 0 ends the
buffer, which is copied through 17EF:06BE into the string held by the far
pointer at DS:7BF8, and that pointer is returned.

Script. Walking the shipped INSTALL.DAT with `//` and `/*` comments removed
outside quotes, 534 quoted strings open and close, and the only escapes in
them are `\\` (259 times) and `\n` (9).

## Interpretation

A quoted string runs to the next unescaped `"` on any line; a line break
inside it is kept. Comments are not recognised inside it. Backslash escapes
are limited to the nine forms above, and any other one ends the program
with a syntax error, so a single backslash in a path has to be written
`\\`. At-sign names inside strings are replaced by their values when the
caller asks for evaluation.

## Alternatives

- An unknown escape keeps both bytes: ruled out; the program stops.
- A string ends at the end of its line: ruled out; only `"` or the end of
  input ends it, and the end of input stops the program.

## How to reproduce

Disassemble `CD:CONFIG.EXE` as 16-bit code with the load image at segment
0x1000 and relocation targets marked, at the ranges in Locations. Read the
twenty words at 221F:10FD (ten characters, then their targets), the four
words at 221F:10F5, and the strings at DS:2FD7, DS:308E, DS:30D9 to DS:3142,
DS:3170, DS:31AF and DS:25E0 to DS:262B. Walk INSTALL.DAT outside comments,
pair the quotes, and count the escapes; report counts only. Keep listings
in ignored local storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.
