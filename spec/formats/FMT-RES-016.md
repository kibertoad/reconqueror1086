---
id: FMT-RES-016
title: INST.EXE script syntax of CD-root INSTALL.SCR
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["CD:INSTALL.SCR"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-045, FND-RES-046, FND-RES-047, FND-RES-048, FND-RES-049]
conflicting: []
split_with: []
related: []
---

## Layout

`INSTALL.SCR` is a script that the installer `INST.EXE` loads whole and runs
from top to bottom, one line at a time [FND-RES-045, FND-RES-046].

Lines:

- A line ends at CR or LF; the shipped file uses CR LF. A line that is empty
  after expansion does nothing.
- The run stops at the end of the loaded bytes or after `end`.
- A line whose first byte is `:` is a label. The label's name is the rest of
  the line, spaces included. Labels are recorded in the order the run passes
  them, again each time it passes them, with no limit on how many.

Parameters. Before a line runs, `%` followed by a digit `1` to `9` is replaced
with one of nine strings the installer holds. `%` followed by any other byte
drops both bytes and keeps the byte after them, so `%%` keeps neither `%`.
`%1` and `%2` are single characters, `%3`, `%4` and `%6` are strings the
installer keeps, `%5` is the path prefix it reads its own files from, and
`%7` is another field of the installer; `%8` and `%9` are never set. A `%` at
the end of a line reads past the line's end.

Commands. The first word of a line, up to the first space, picks a command,
compared without regard to case. The command's argument is the rest of the
line after one space or tab; for every command except `echo`, `alert` and
`pause`, further spaces and tabs are skipped as well.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| `;`, `rem`, `/*`, `//` | char[] | script_command | Comment: the line does nothing. | supported | FND-RES-046 |
| `echo` | char[] | script_command | Writes its argument and a line break to the installer's window. With `>`, the text before it goes to the named file instead, replacing it; with `>>` it is appended. A line break is added unless the text ends in one. | supported | FND-RES-046 |
| `pause` | char[] | script_command | Writes its argument, used as a format string, or a standard prompt when it has none, then a line break; between the two it calls a routine that is not read. | supported | FND-RES-046 |
| `goto` | char[] | script_command | Jumps to a label (see below). | supported | FND-RES-046 |
| `end` | char[] | script_command | Stops the script. | supported | FND-RES-046 |
| `clear`, `cls` | char[] | script_command | Clear the window. | supported | FND-RES-046 |
| `alert` | char[] | script_command | Shows its argument in a message box. Enter goes on; any other key leaves the installer. | supported | FND-RES-047 |
| `space` | char[] | script_command | Takes a drive letter, a number of kilobytes and a label. Jumps to the label when the drive's free space in kilobytes is less than the number, of which only the low 16 bits count. Also stores the free space as megabytes with one decimal. Stops the installer with an error when RESOURCE.CFG sets `space`. | supported | FND-RES-047 |
| `pick` | char[] | script_command | Takes a list of key letters and four labels. Waits for a key in the list, without regard to case, signalling any other key, and jumps to the label in that key's position. A key whose code has a low byte of 0 selects the position after the last letter. | supported | FND-RES-047 |
| `godir` | char[] | script_command | Takes a path and a label. Makes the path's drive and the path current, creating each missing directory level, and jumps to the label when it cannot. | supported | FND-RES-047 |
| `exists` | char[] | script_command | Takes a file name and a message. Shows the message box again and again until the file exists. | supported | FND-RES-047 |
| `testdir` | char[] | script_command | Takes a directory. When it exists and holds any file, makes it current and asks a yes/no question; the yes answer ends the script. | supported | FND-RES-047 |
| `del` | char[] | script_command | Deletes every file its argument matches, by bare name in the current directory. | supported | FND-RES-047 |
| `if` | char[] | script_command | Takes an optional `not`, then `errorlevel` and a number, true when the last program's result is that number or more (compared signed), or `exist` and a file pattern, true when a file matches. When the test (inverted by `not`) holds, the rest of the line runs as the next line. Any other test word shows an error box and the command is skipped. | supported | FND-RES-048 |
| `copy` | char[] | script_command | Takes a source pattern, an optional destination and the options `/q` (no error when nothing matches) and `/s` (no per-file messages), lower case only. First extracts the members matching the source's name and extension from `drivers.sip` and `sierra.sip` in the source's directory, when they exist, into the destination's directory or the current one. Then copies each matching file, replacing the destination, which defaults to the same name in the current directory; a destination name that is empty or starts with `*`, or an extension of `.*`, takes the source file's. With a `+` anywhere, `copy a+b` appends `b` to `a` instead. | supported | FND-RES-049 |

`if` does not run its command itself. The next pass starts in the script's
own line at the place where the command began in the expanded line, so a
parameter before the command whose value is not exactly two characters long
moves that place [FND-RES-048].

A first word that is none of these passes the whole line to a program
runner, whose reading is open (Q-RES-198). When the runner reports one of its
three errors, a message box offers Enter to go on; any other key leaves the
installer.

`goto` takes its argument's first word. It jumps to just after the first
recorded label whose name starts with that word, comparing case. When none
matches, the run skips every following line until it reaches a label whose
name equals the whole argument exactly, case included, and carries on from
there. An empty argument jumps to the first recorded label.

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

FND-RES-046 counts the shipped file's lines: every first word is a comment,
a label, an empty line or one of `echo`, `end`, `clear`, `godir`, `alert`,
`copy`, `goto`, `space`, `pick` and `pause`, apart from two lines holding only
0x1A, the second without a line end, which follow an `end`. The file uses
`%1` to `%4` and `%%`. FND-RES-046 reads the run loop, the parameter setup,
the expansion, the dispatch and the routines for comments, `echo`, `pause`,
`goto`, `end`, `clear` and `cls`; FND-RES-047 reads `alert`, `space`,
`pick`, `godir`, `exists`, `testdir` and `del`, FND-RES-048 reads `if`,
and FND-RES-049 reads `copy`. The archive routines `copy` calls are not read;
the build has no `.SIP` file for them to open. The program runner behind unknown words, the message lookup and
the window routines are not read, and neither are several run-time routines
the commands call, such as the key read and the find-next.

## Open questions

- How does the program runner behind unknown words split the program from its
  arguments, and which DOS call runs it? (Q-RES-198)
- Which installer fields give `%1` to `%7`, and what values do they hold
  when the script runs? (Q-RES-199)
- When does INST.EXE run the script, and does every path through the shipped
  script reach an `end`? (Q-RES-200)
