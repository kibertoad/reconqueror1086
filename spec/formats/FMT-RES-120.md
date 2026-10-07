---
id: FMT-RES-120
title: SIERRA.INF, the installer's script file
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:SIERRA.INF"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-021, FND-RES-029, FND-RES-032, FND-RES-033]
conflicting: []
split_with: []
related: []
---

## Layout

A text file of lines, read in two ways. The first-stage installer `SETUP.EXE`
and the second-stage installer `_SETUP.EXE` read some keys through the
Windows profile routines, which parse the file as an INI file (FND-RES-029,
FND-RES-032). `_SETUP.EXE` also reads the whole file once, line by line in
text mode, at most 255 bytes per line, and divides it into sections by five
markers (FND-RES-033). The shipped file is printable ASCII with CR LF line
ends and no line end after its last line (FND-RES-021).

A line starting with a marker, in exact case, starts that section; the rest
of the marker line is ignored, and every other line outside a section is
skipped. The markers are, in the order they are tested, `[Archives]`,
`[Files]`, `[Dialogs]`, `[Script]` and `[Billboards]`. A section ends at the
end of the file or at the line its grammar treats as empty, and the line
after that is tested for markers again; a marker line reached before that
is read as part of the section. Inside a section, a line starting with `;`
is skipped. A section met twice is read twice.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| `[Script]` | `char[]` | `script` | Leading spaces and tabs are removed and the line, line feed included, is added to one script text of at most 64,000 bytes counting a NUL per line; a larger script stops the installer with a message. Ends at a line that is a line feed after the spaces and tabs. The commands in the text are read elsewhere. | supported | FND-RES-033 |
| `[Archives]` | `char[]` | `archives` | Tokens split on comma, space and tab: a name, then a number. Then a 32-bit number, a number and a 32-bit number, the last two split on line feed as well. Ends at a line whose first token is missing or starts with a line feed. | supported | FND-RES-033 |
| `[Files]` | `char[]` | `files_section` | As `[Archives]`, but the second token is a name, and a number follows it only when that name is `NOARCHIVE` (letter case ignored) or ends in `\`. | supported | FND-RES-033 |
| `[Dialogs]` | `char[]` | `dialogs` | Blocks. A line containing `BEGIN` starts one, followed on that line by the dialog's number and name; the next line is its title; each later line is an item, a number, a comma and a text up to the next comma, space or line feed, with an optional comma and more; a line starting with `END` (letter case ignored) ends the block. Lines outside blocks without `BEGIN` are skipped, and the section ends at a line starting with a line feed. A title or item text not starting with `*` is a key of LANGUAGE.INF's `Strings` section (FMT-RES-119). | supported | FND-RES-033 |
| `[Billboards]` | `char[]` | `billboards` | `<number>=<text>`, the text running to the line feed. Ends at a line starting with a line feed. | supported | FND-RES-033 |
| `[Setup]` `SetupSize` | `UINT16` | `setup_size` | Read by `SETUP.EXE` through the profile routines as an integer. | supported | FND-RES-029 |
| `[Setup]` `ForceLanguage` | `char[]` | `force_language` | Read by `SETUP.EXE` into 10 bytes and compared with five language names. Not in the shipped file. | supported | FND-RES-029 |

Numbers are decimal, with optional leading spaces and tabs and one sign,
read to the first other byte; a number kept in 16 bits keeps the low 16 bits
of the 32-bit value.

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The shipped `CD:SIERRA.INF` of BLD-GOG-EN has `[Script]`, `[Dialogs]` and
`[Files]` as markers, each section closed by an empty line before the next
section header, and the sections `Setup`, `Requirements` and `Ident`, which
the loader skips. Its eight dialogs request 32 `Strings` keys, and its seven
`[Files]` records all use `NOARCHIVE` (FND-RES-033).

## Open questions

- How are the `[Script]` lines interpreted? The text is kept whole and
  read by a command interpreter whose names follow the string `RUN` in the
  installer; the shapes FND-RES-022 records are its input. (Q-RES-179)
- What do the dialog item's trailing fields and a `*` title mean? Item
  fields after the text are read by 0004:3142 and a `*` title by 0003:C46A,
  neither read yet. (Q-RES-180)
- Which keys of `Setup`, `Requirements` and `Ident` does `_SETUP.EXE` read
  from this file? Its profile reads of those sections take the file name
  from a variable. (Q-RES-181)
- What do the numeric fields of `[Archives]`, `[Files]` and `[Billboards]`
  records mean? Their consumers are not read. (Q-RES-182)
