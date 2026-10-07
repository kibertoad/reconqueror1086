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
evidence: [FND-RES-045, FND-RES-046]
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

The words `alert`, `copy`, `space`, `godir`, `exists`, `pick`, `testdir`,
`del` and `if` are commands too; what they do is in Open questions.

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
`goto`, `end`, `clear` and `cls`; the other nine command routines, the
program runner behind unknown words, the message lookup and the window
routines are not read.

## Open questions

- What does `alert` do with its argument? (Q-RES-193)
- What do `copy`, `del` and `exists` do with their arguments, and what does
  each do when the file operation fails? (Q-RES-194)
- What do `space`, `godir` and `testdir` test or change, and when does
  `testdir` stop the script? (Q-RES-195)
- What does `pick` offer and where does the choice go? (Q-RES-196)
- What syntax does `if` take, and what does it run? (Q-RES-197)
- How does the program runner behind unknown words split the program from its
  arguments, and which DOS call runs it? (Q-RES-198)
- Which installer fields give `%1` to `%7`, and what values do they hold
  when the script runs? (Q-RES-199)
- When does INST.EXE run the script, and does every path through the shipped
  script reach an `end`? (Q-RES-200)
