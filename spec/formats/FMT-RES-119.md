---
id: FMT-RES-119
title: LANGUAGE.INF, the installer's profile file of titles and strings
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:LANGUAGE.INF"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-021, FND-RES-026, FND-RES-032, FND-RES-033, FND-RES-038, FND-RES-040]
conflicting: []
split_with: []
related: []
---

## Layout

A Windows profile (INI) file. Both programs known to read a file of this
name read it only through the Windows profile routines, one key at a time:
the CD launcher `AUTOPLAY.EXE` reads the copy in the installed game's
directory (FND-RES-026), and the second-stage installer `_SETUP.EXE` reads
the copy beside the SIERRA.INF it loads, or failing that one in a directory
it holds, and the copy in the product directory (FND-RES-032). Sections,
keys, comments, white space and letter case are parsed by Windows, and
neither program adds a rule of its own. The shipped file is printable ASCII
with CR LF line ends and no line end after its last line (FND-RES-021).

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| `[Ident]` `Title` | `char[]` | `title` | Read by the launcher into 50 bytes from an installed copy and compared with `AUTORUN.INF`'s `Title` (FMT-RES-117). | supported | FND-RES-026 |
| `[Ident]` `DirName` | `char[]` | `dir_name` | Read by `_SETUP.EXE` into 13 bytes, default empty, and added after `\` to the destination directory it proposes. | supported | FND-RES-040 |
| `[Ident]` `ShortTitle` | `char[]` | `short_title` | Read by `_SETUP.EXE` into 0x50 bytes, default empty. | supported | FND-RES-032 |
| `[Strings]` `<key>` | `char[]` | `strings` | Read by `_SETUP.EXE` under a key name its caller passes. Dialog titles and item texts in SIERRA.INF's `[Dialogs]` section (FMT-RES-120) that do not start with `*` are keys, read into 0x200 bytes with an empty default, so a missing key gives empty text. The `[Script]` command `ADDPROGMANGROUP`'s argument is a key read into 0x50 bytes with default `Sierra`, and `ADDPROGMANITEM`'s second field one read into 0x50 bytes with an empty default. | supported | FND-RES-032, FND-RES-033, FND-RES-038 |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The shipped `CD:LANGUAGE.INF` of BLD-GOG-EN has the sections `Ident` and
`Strings`. Its `Ident` `DirName` is `CONQUER` (FND-RES-040). Its `Strings` section has the keys of the three
`ADDPROGMANITEM` lines and lacks the `ADDPROGMANGROUP` key, so the group
name is `Sierra` (FND-RES-038). It repeats the key `InstallDoneTitle`
(FND-RES-022), and 12 of its lines begin with `;` (FND-RES-021).

## Open questions

- Which occurrence of the repeated `InstallDoneTitle` does a read return?
  The profile routine decides, not the game; Windows versions are expected
  to return the first, which no source for this build records. (No item:
  operating-system behaviour, not decided by the game)
- Does the installer copy this file into the installed directory? The
  launcher's reading of an installed copy rests on equal values only.
  (Q-RES-178)
