---
id: FMT-RES-013
title: INST.EXE text dictionary syntax of CD-root INSTALL.TXT and INSTALL.HLP
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:INSTALL.TXT", "CD:INSTALL.HLP"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-019, FND-RES-053]
conflicting: []
split_with: []
related: []
---

## Layout

Both files are CRLF text read in text mode, so each line reaches the reader
ending in LF. The reader takes them in pieces of at most 99 bytes; a line of
99 bytes or more arrives as several pieces, and the rules below apply to each
piece [FND-RES-053]. No shipped line is cut where that changes the result
[FND-RES-053].

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| Before the first marker line | BYTE[] | (skipped) | Ignored. Both shipped files start with a marker line. | supported | FND-RES-053 |
| Line starting with two backslashes | BYTE[2] | text_dict_marker | Starts an entry and ends the previous one's text. | supported | FND-RES-053 |
| Rest of the marker line | BYTE[] | text_dict_key | The entry's key: leading and trailing space, tab, CR and LF removed; matched without case. | supported | FND-RES-053 |
| Lines up to the next marker line or the end | BYTE[] | text_dict_text | The entry's text: the lines joined with their LF, with one final LF dropped. | supported | FND-RES-053 |
| Last two bytes of a text line | BYTE[2] | text_dict_continuation | `\` followed by LF: both removed, so the next line continues this one. | supported | FND-RES-053 |
| Line 358 of INSTALL.TXT, line 752 of INSTALL.HLP | BYTE[1] | text_dict_end_byte | A lone 0x1A, inside the last entry's text unless the text-mode read stops at it. | supported | FND-RES-019, FND-RES-053 |

A key loaded again replaces the earlier entry's text. The installer loads
`install.txt`, then `install.hlp`, then every other `*.txt` and `*.hlp` file,
all into one dictionary, so a later file overrides an earlier one, and it
looks keys up without case [FND-RES-053]. With the environment variable
`LANGUAGE` set, `install.txt` and `install.hlp` are read from the directory
it names [FND-RES-053]. A file that does not open is looked for in
`install.sip`, which this build does not ship [FND-RES-053, FND-RES-049].

Shipped content: INSTALL.TXT has 138 entries with distinct keys, holding every
message the installer's code read so far names, and 21 continued lines.
INSTALL.HLP has 157 entries with 147 distinct keys, 40 tabs and no continued
lines; one key ends in a space, which the reader removes. The two files share
no key [FND-RES-053, FND-RES-019]. No original text is kept here.

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Every byte of both BLD-GOG-EN files was classified [FND-RES-019,
FND-RES-053]. The loader, parser, store and lookup of INST.EXE were read
[FND-RES-053]; the run-time `fgets` path, the entry constructor 2852:0C71 and
the archive reader were not. Files of the same names in the demo and other
product directories of the disc are not covered.

## Open questions

- Does the text-mode read stop at the 0x1A byte, leaving it and the empty
  line after it out of each file's last entry? Reading the run-time library's
  text-mode read would settle it. (Q-RES-162)
- How does the installer show tabs in a text? The dictionary keeps them; the
  display routines were not read. (Q-RES-163)
- Which code looks up the keys of INSTALL.HLP? None of the keys the code read
  so far uses is in it. (Q-RES-204)
- What happens when a key is missing: does each caller of 2852:0B71 pass a
  flag that ends the run with a fatal error, or get an empty text? (Q-RES-205)
