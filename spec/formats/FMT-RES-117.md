---
id: FMT-RES-117
title: AUTORUN.INF, the CD launcher's profile file
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:AUTORUN.INF"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-021, FND-RES-026, FND-RES-027]
conflicting: []
split_with: []
related: []
---

## Layout

A Windows profile (INI) file. The CD launcher `AUTOPLAY.EXE` reads it only
through `GetPrivateProfileStringA` and `GetPrivateProfileSectionA`, under the
name `.\autorun.inf`, so it is the copy in the launcher's current directory.
Sections, keys, comments, white space and letter case are parsed by Windows,
and the launcher adds no rule of its own. The shipped file uses CR LF line
ends (FND-RES-021). Text is 8-bit; values reach the screen through
`MessageBoxA` unchanged, so the bytes above 0x7F, all in the `french` and
`german` sections, are shown in the ANSI code page of the Windows system
running the launcher. A missing key returns the default the launcher passes,
which is empty for every `Sierra` key and one space for the message keys
[FND-RES-026]; Open questions covers what Windows returns for the latter.

Rows follow the order of the shipped file. `<prefix>` is `run` or `install`
and `<n>` is 1 to 9.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| `[autorun]` `OPEN` | `char[]` | `autorun_open` | Not read by AUTOPLAY. | supported | FND-RES-026 |
| `[autorun]` `ICON` | `char[]` | `autorun_icon` | Not read by AUTOPLAY. | supported | FND-RES-026 |
| `[Sierra]` `Title` | `char[]` | `title` | Read into 50 bytes. A directory is taken as the installed game only when its `LANGUAGE.INF` has the same `Title` in section `Ident`, compared with ASCII letter case ignored and every other byte exact. Empty or missing shows an error box, and the launcher goes on. | supported | FND-RES-026, FND-RES-027 |
| `[Sierra]` `DirName` | `char[]` | `dir_name` | Read into 20 bytes. The name of the installed game's directory, searched for under the Sierra directory and compared with ASCII letter case ignored and every other byte exact. Empty or missing shows an error box. | supported | FND-RES-026, FND-RES-027 |
| `[Sierra]` `NecFiles` | `char[]` | `nec_files` | Read into 255 bytes. Comma-separated file names that must all open inside the found directory; spaces are kept as part of each name. Empty or missing skips the check. | supported | FND-RES-026 |
| `[Sierra]` `ExeName` | `char[]` | `exe_name` | Read into 25 bytes. The command run in the found directory, appended to the directory path and passed to `WinExec`. Empty or missing shows an error box. | supported | FND-RES-026 |
| `[Sierra]` `DefaultLang` | `char[]` | `default_lang` | Read into 1,024 bytes. The section that supplies the messages when the user's language has no entry in the launcher's table or no section in this file. | supported | FND-RES-026 |
| `[<language>]` `<prefix>Text<n>` | `char[]` | `message_lines` | Read into 80 bytes each, from `<n>` = 1 until the first read that returns a count of 0, and shown joined by line feeds in an OK and Cancel box captioned `Sierra`. `run` lines are shown when the installed game is found, and OK runs it; `install` lines otherwise, and OK runs `setup.exe` from the current directory. | supported | FND-RES-026 |

`<language>` is the name the launcher's table gives the primary language of
the user's default language: 9 `english`, 12 `french`, 10 `spanish`, 7
`german`, 16 `italian`. The shipped file has `english`, `french` and
`german` sections, and `DefaultLang` is `english`.

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The shipped `CD:AUTORUN.INF` of BLD-GOG-EN holds every key in the table and
no other, and its `Title` and `DirName` equal those in `CD:LANGUAGE.INF`'s
`Ident` section (FND-RES-026).

## Open questions

- Does the one-space default end the message loop at the first missing key?
  The launcher stops on a read that returns 0. Microsoft documents that the
  profile routine strips trailing spaces from a default, which would make a
  missing key return 0; a Windows version that kept the space would return 1
  and add a line holding one space. This depends on the Windows version
  running the launcher, not on the game. (No item:
  operating-system behaviour, not decided by the game)
- Is a `DirName` match made against the long or the short (8.3) name of a
  directory? The names come from a library routine at 0x00411140 over
  `FindFirstFileA` and `FindNextFileA` that FND-RES-026 does not read, and
  the shipped `DirName` fits either form. (Q-RES-173)
- Which program reads the `autorun` section? Windows' AutoRun is the expected
  reader; no source for this build records it yet. (Q-RES-172)
