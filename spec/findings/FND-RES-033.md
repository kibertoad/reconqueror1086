---
id: FND-RES-033
title: _SETUP.EXE reads SIERRA.INF line by line into five marked sections, each with its own line grammar
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
expands it. The C library routines this reading relies on were each read:
0001:2152 returns a string's length; 0001:2196 compares at most n bytes
exactly, stopping at a NUL; 0001:3D4C compares at most n bytes with only
the letters A to Z folded to lower case; 0001:9812 compares whole strings
the same way; both return 0 for equal. 0001:3CD4 finds a byte and returns a
pointer to it or null; 0001:3F96 finds a string within a string; 0001:3E92
counts the leading bytes that are in a set, and 0001:3DD8 those that are
not; 0001:3EEE splits a string into tokens on a set of delimiter bytes,
keeping its position at DS:0x1394 and writing a NUL after each token.
0001:24B0 reads a line: at most n − 1 bytes, stopping after a line feed,
which it keeps, ending with a NUL, and returning null when it reads nothing.
0001:21D2 and 0001:21D6 both jump to 0001:2238, which skips spaces and tabs,
takes one `-` or `+`, and accumulates decimal digits into a 32-bit value in
DX:AX, stopping at the first other byte. 0002:59E0 appends text to a string
object, and 0002:5C9C cuts a string object at its first byte from a set.

Loader, 0004:2C84 (far, returns 1 in AX or jumps to 0004:2E8F). It appends
`\SIERRA.INF` to the text at offset 0x132 of the object it is given, finds
LANGUAGE.INF beside it (FND-RES-032), copies the path, converts it with
KEYBOARD ordinal 5, and passes it with the mode string `r` to 0001:0856,
whose far result it gives every line read as the file, without testing it.
It then reads lines of at most 255 bytes through 0001:24B0 until that
returns null, and passes the file to 0001:071A. Each line is compared,
over the length of each marker, with `[Archives]`, `[Files]`, `[Dialogs]`,
`[Script]` and `[Billboards]` in that order, through 0001:2196, so a marker
matches only at the start of the line and in exact case, and the rest of
the line is ignored. A line matching none is skipped. A match calls the
marker's handler with the object and the open file, and reading resumes
with the line after the last one the handler read: 0004:2E96 for
`[Archives]` (passing 0) and `[Files]` (passing 1), 0004:32DC for
`[Dialogs]`, 0004:37B8 for `[Script]` and 0004:38FC for `[Billboards]`.

Every handler reads its lines through 0001:24B0 and skips each line whose
first byte is `;`; no other comment form is recognised.

`[Script]`, 0004:37B8. It requests 64,000 (0xFA00) bytes from 0001:1D13
and keeps the far pointer at offset 4 of the object; if that fails, it shows
the message at 0004:74E0 and calls 0001:01E7 with 0. For each line it skips
leading spaces and tabs (the set at 0004:02EA). Reading ends at the end of
the file or at a line that is then a line feed. Otherwise it adds the length
of the rest plus one to a 32-bit total, and while the total is at most
64,000 copies the rest, line feed included, to the end of the buffer,
advancing by the length without the NUL, so the lines follow each other in
one string. A total above 64,000 shows the message at 0004:751E, frees the
buffer, sets the pointer to null and calls 0001:01E7 with 0.

`[Archives]` and `[Files]`, 0004:2E96. Reading ends at the end of the file
or when the first token of a line, split on comma, space and tab (the set at
0004:02EE), is missing or starts with a line feed. Otherwise it makes a
32-byte record: the first token, as a string object at offset 4; then from
the next tokens on the same delimiters, for `[Archives]`, a number at offset
0x0C (16 bits kept); for `[Files]`, a string object at offset 0x0E and, when
that token equals `NOARCHIVE` with letter case ignored or ends in `\`, a
number from the following token at 0x0C. Then a 32-bit number at 0x16 from
the next token on comma, space and tab; a number at 0x1A (16 bits) and a
32-bit number at 0x1C from the next two tokens on comma, space, tab and line
feed (the set at 0004:7468). Unread fields stay 0, and reading stops at the
first missing token. The record starts with 0 at 0x0C, 0x16 and 0x1A and
empty string objects; 0x1C is set only from its token. The record is
appended to the array at offset 0x0A of
the object for `[Archives]` or 0x22 for `[Files]`. Because the first
delimiter set leaves out the line feed, a line that ends after its first or
second token keeps the line feed in that token's string. A last line made
only of commas, spaces and tabs, with no line feed, gives no first token,
and the handler then reads a byte through a null pointer.

`[Dialogs]`, 0004:32DC, reads lines of at most 511 bytes. Reading ends at
the end of the file or at a line whose first byte is a line feed. A line
without `BEGIN` (searched anywhere in the line, exact case) is skipped. After
`BEGIN`, a number on space, tab and comma becomes the dialog's number at
offset 4 of a 0x26-byte dialog record, and the next token, on space, tab,
comma and line feed, its name at offset 6. The next line, after leading
spaces and tabs and cut at its line feed, is the title, stored at offset
0x0E. Each later line, after leading spaces and tabs, is either `END` (the
first three bytes, letter case ignored), which appends the dialog to the
array at offset 0x48 of the object and returns to looking for `BEGIN`, or
an item: a 0x1E-byte record whose number at offset 4 is read from the line,
and whose text, at offset 6, starts after the first comma and leading spaces
and tabs and runs to the first comma, space or line feed. When a comma
follows the text, the rest is read for item number 6 as a number, four bytes
after the spaces and tabs that follow the comma, stored at offset 0x1C, and
for other items by 0004:3142, which is given offset 0x0E and whose return
value is stored at 0x1C; when no comma follows, 0x1C is set to 0xFFFF. An
item line with no comma at all, an empty line among them, takes a null
pointer plus one as the start of its text. Items are appended to the array at offset 0x16 of the
dialog. A title that does not start with `*`, and an item text that does not
start with `*`, is taken as a key of LANGUAGE.INF's `Strings` section and
replaced by its value, read into 0x200 bytes with an empty default (0004:34DF
and 0004:366C, FND-RES-032). An item text starting with `*` is kept as it
is, `*` included; a title starting with `*` is given to 0003:C46A, which this
finding does not read. The end of the file before the first item discards
the dialog; after an item, it keeps it.

`[Billboards]`, 0004:38FC. Reading ends at the end of the file or at a line
whose first byte is a line feed. Each other line makes a 14-byte record: the
number at the start of the line at offset 0x0C (16 bits kept), and at offset
4 the text after the first `=`, cut at the line feed. The record is appended
to the array at offset 0x3A of the object. A line without `=` gives a null
pointer plus one as the start of the text.

Shipped data. The shipped SIERRA.INF read with CR LF as one line end, as a
text-mode read gives it, has 161 lines and none longer than 102 bytes. Its
markers are `[Script]` on the first line, `[Dialogs]` and `[Files]`; it has
no `[Archives]` or `[Billboards]`. Every marker other than the first follows
an empty line, the first lines of `[Setup]`, `[Requirements]` and `[Ident]`
also follow empty lines, and those three sections are not markers. Walking
the file with the procedure above gives eight dialogs, numbered 0 to 7, every
title a key, items of numbers 1, 3 and 10 to 13, and 32 keys requested from
LANGUAGE.INF's `Strings`: `InstallDoneTitle` once, and six keys the shipped
LANGUAGE.INF does not define, which read as empty. 16 keys of that section
are not among them. Its `[Files]` section has seven records, each with
`NOARCHIVE` as the second token and decimal numbers after it: three in six
records, so their offset 0x1C is never set, and four in the last.

## Interpretation

SIERRA.INF is the installer's script file: a sequence of sections, five of
which this loader reads with their own grammars, the rest read only through
the Windows profile routines. The `Script` section is stored as text and
interpreted elsewhere; the command-shaped lines FND-RES-022 found there are
script commands, the comma-bearing lines are `Files` and `Dialogs` records,
and the dialog texts are keys into LANGUAGE.INF.

## Alternatives

- One permissive grammar for every section: ruled out; each handler splits
  lines its own way.
- The loader reads LANGUAGE.INF's `Strings` keys by its own scan: ruled out;
  each key goes through the profile routine (FND-RES-032).

## How to reproduce

Expand `_SETUP.EXE` as FND-RES-032 says. Disassemble as 16-bit code, with
the relocation targets beside each instruction, 0004:2C84 to 0004:2E93,
0004:2E96 to 0004:3140, 0004:32DC to 0004:37B5, 0004:37B8 to 0004:38F9,
0004:38FC to 0004:3A1C, and the library routines named above from their
first addresses; read the strings at the segment 4 offsets named. Walk the
shipped SIERRA.INF and LANGUAGE.INF with the procedure, comparing key names
with letter case ignored, and report counts and key names only. Keep
listings in ignored local storage.
