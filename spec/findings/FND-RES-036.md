---
id: FND-RES-036
title: CONFIG.EXE reads its script through a comment-removing character layer and upper-cases at-sign names before matching them
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2D9F:003D..2D9F:01D2
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2D9F:0205..2D9F:03C4
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 2D9F:046D..2D9F:04BA
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 221F:0003..221F:035E
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 221F:0509..221F:0548
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 3044:0000..3044:016C
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 3044:01AD..3044:0263
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1000:1839..1000:1864
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1000:5FF6..1000:60D4
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1890:00A7..1890:00F1
  - build: BLD-GOG-EN
    file: CD:INSTALL.DAT
    offset: 0x00..0xBDDB
tool: capstone 5.0.7 bounded 16-bit disassembly with relocation targets marked; Python walk of the shipped INSTALL.DAT
environment: null
---

## Observation

Addresses are as FND-RES-034 gives them for `CD:CONFIG.EXE`; DS is the data
segment. The script handle is the one FND-RES-035's search returns. The
character class table is at DS:6D7B, indexed by byte: bit 0 marks 0x09 to
0x0D and 0x20, bit 1 the digits, bit 2 `A` to `Z`, bit 3 `a` to `z`.
1000:1839 returns a byte with bit 3 less 0x20 and any other byte unchanged.
1890:00A7 passes each byte of a string with bit 3 through 1000:1839 in place.
1000:5FF6 compares two strings byte for byte. 1000:6094 compares them with
`a` to `z` (0x61 to 0x7A) lowered by 0x20 on both sides. The routines
1000:083A, 1000:087B and 1000:07A3, and 2FEA:00B8, are named by address and
arguments; their bodies were not read.

Bytes. 2D9F:003D returns the last word pushed back, when the push-back
stack at the far pointer DS:536C holds any (its count is at DS:5362).
Otherwise it returns the next byte of a 1,024-byte buffer at the far pointer
DS:5370, refilled through 2FEA:00B8 with the handle, the buffer and the
smaller of 0x400 and the 32-bit count left at DS:535E, which each byte
returned lowers by one. It returns 0xFFFF when that count is 0, or when the
byte just taken was the last of the buffer and is 0x1A. Where DS:535E is set
was not read. 2D9F:037B pushes a word back; at 3,000 words it reports an
internal error through 213C:0007.

Characters. 2D9F:028B takes the handle and a comment switch. A byte of 0x0D
or 0x0A sets the word at DS:536A to 1, and 0x0A adds one to the 32-bit line
count at DS:8232. While DS:536A is set, a digit or letter leaves it set and
any other byte, a space or `@` among them, clears it. With the switch nonzero, `/` is looked at
with the byte after it:

- `/*`: 2D9F:0205 reads past bytes until a `*` immediately followed by
  `/`, counting lines, and 2D9F:028B returns a space for the comment. Comments do not nest. If the input ends first, it writes
  a message naming the line where the comment began and calls 35C0:082A
  with 1.
- `//`: the bytes up to and including the next 0x0A, or to the end, are
  read and dropped without regard to quotes; DS:536A is set, the line
  counted, and 0x0A returned.
- `/` and any other byte: that byte is pushed back and `/` returned.

Every 0x0A that 2D9F:028B returns is followed by a pushed-back 0x0D, so the
next read gives 0x0D. 2D9F:03AD reads one character with comments on and
pushes it back. 2D9F:046D reads with comments on past every byte with bit 0
and every `/*` comment, and pushes back the first other byte.

Tokens. 221F:0003 takes the handle. When the byte at DS:272E is set, it
clears it and returns the token at DS:272C, which 221F:0526 stores there.
Otherwise it clears the text buffer at DS:7C02, skips through 2D9F:046D,
keeps DS:536A as it was at that point, and reads one character with comments
on through 221F:0509. 0xFFFF is returned as 0xFFFF. A first byte of `@`
starts a name:

- `@` is stored, then characters are read with comments on while each is a
  digit or letter. The loop also takes `:` when the kept DS:536A is
  nonzero, but 2D9F:046D has already read the `@` once through 2D9F:028B,
  which cleared DS:536A, so that arm is never taken and `:` ends a name. Each is stored after 1000:1839, so letters are upper case in the
  buffer. A name that reaches 1,500 bytes reports `String too long`
  through 213C:0007. `_` and every other byte end the name.
- The ending byte: `@` right after the first `@` gives token 0x1B; `!` right
  after the first `@` gives whatever a fresh call of 221F:0003 returns,
  so `@!` is passed over. `@` after a longer name is read with the byte
  after it: `@!` is dropped, and otherwise both are pushed back. Any other
  ending byte is pushed back.
- The buffer is ended with a 0 and compared with the 165 six-byte entries at
  DS:272F (a far pointer to a name, a length byte, a class byte): an
  entry matches when its length byte equals the name's length and
  1000:5FF6 reports the texts equal. Entry i gives token i, except that
  221F:033B maps 12 (`@ENDWELCOME`) to 0x74, 43 (`@WELCOME`) to 0x73, 131
  (`@F`) to 13, 132 (`@S`) to 0x2E, 133 (`@O`) to 0x15, 134 (`@A`) to 0x31,
  135 (`@GETQSTRING`) to 0x7F, 136 (`@GETDIR`) to 0x10, and 163 to 0x8E.
  Entries 0 (`INVALID KEYWORD`), 44 (`STRING`) and 45 (`NUMBER`) lack the
  `@` that every name starts with, so no name matches them. Entry 136's length byte is 10 for the 7-byte `@GETDIR`,
  and entry 64's is 10 for the 12-byte `@UNUSEDTOKEN`, so neither ever
  matches. Entries with a null name have length 0 and never match.
- No match sets DS:7BFC to 9 and returns 0, with the upper-case name left in
  DS:7C02.

Other first bytes go to a number reader (a digit, or `-` then a digit), an
identifier reader at 221F:07E2 (a letter; with the kept DS:536A nonzero and a
following `:` it gives a label), the string reader at 221F:0B5C (`"`, which reads with
comments off at 221F:0BDF, 221F:0C14 and 221F:0D06), or a table of single
characters at 221F:035F. Those readers are not covered here.

Names. Callers such as 221F:1247 pass DS:7C02 to 3044:011A, which calls
3044:00B2. 3044:0000 hashes a name: it skips a leading `@` when a second
byte follows; for each byte it shifts the 32-bit value left by 4 through
1000:083A and exclusive-ors the byte in; when the top four bits are set it
exclusive-ors in those bits shifted right by 24 (1000:087B) and clears them;
the result goes to 1000:07A3 with 211. 3044:00B2 walks the chain of records
from the far pointer at DS:8AA4 plus four times that result, linked at
offset 0x1C, and returns the first whose name 1000:6094 reports equal.
3044:01AD, which registers a name, hashes it as given and stores an
upper-cased copy through 1890:00A7.

Script. The shipped INSTALL.DAT has no `@` name followed directly by `_`,
holds `@!` and holds `@@` twice. Its last byte is not 0x1A and it holds no
0x1A.

## Interpretation

At-sign words are matched without regard to the case of `a` to `z`: the
reader upper-cases them before the keyword table and before the name table,
whose lookup also folds case. `//` starts a comment to the end of the line
and `/*` one to `*/`, both outside quoted strings only. A name ends at the
first byte that is not a letter or digit, so a stored `@` name never
contains `_` or `:`.

## Alternatives

- Case variants of a name dispatch to different handlers: ruled out for
  `a` to `z`, which are upper-cased before every comparison.
- Quotes inside a `//` comment start a string: the comment is dropped byte
  by byte up to the line feed without looking at quotes.

## How to reproduce

Disassemble `CD:CONFIG.EXE` as 16-bit code with the load image at segment
0x1000 and relocation targets marked, at the ranges in Locations; read the
165 entries at DS:272F, the nine-word table at 221F:033B with its targets,
and the class table at DS:6D7B. Walk INSTALL.DAT for `@` followed by letters
and digits and report counts only. Keep listings in ignored local storage.
