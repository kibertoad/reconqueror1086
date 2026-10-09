---
id: FND-RES-042
title: CONFIG.EXE reads a word at the start of a line followed by a colon as a label, echoes @DISPLAY text to the screen, and stops at any other bare word
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 221F:0003..221F:0335
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 221F:07E2..221F:085F
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B20:0000..1B20:05C3
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:0147..1B80:0182
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:1D90..1B80:1DA6
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:36DA..1B80:3793
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 192C:0003..192C:0022
  - build: BLD-GOG-EN
    file: CD:INSTALL.DAT
    offset: 0x00..0xBDDB
tool: capstone 5.0.7 bounded 16-bit disassembly with relocation targets marked; Python walk of the shipped INSTALL.DAT
environment: null
---

## Observation

Addresses, DS, the class table at DS:6D7B, the character layer, the line
flag at DS:536A, the token reader 221F:0003, the name buffer at DS:7C02, the
keyword table at DS:272F and the stop 213C:00A1 are as FND-RES-036 gives
them; the runner 1B80:184F, the echo routine 35C0:0E1F and the `end of
file` stop are as FND-RES-064 gives them.

Words. 221F:0003 keeps DS:536A in [bp-6] after skipping white space and
before reading the first character. A first character with class bit 2 or
3 (a letter) is pushed back and 221F:07E2 reads characters with comments on
while each is a digit, a letter or `_`, at most 1,500, into DS:7C02; the
first other character is pushed back, and the end of input ends the word.
Then:

- A word of one byte gives token 0x7E (`@BYTE`), with DS:7BFC set to 0x7E.
- A longer word, when the kept DS:536A is nonzero and the next character is
  `:`, takes the `:` and gives token 0x95 (keyword entry 149, which has no
  name), with DS:7BFC set to 0x0C. DS:536A is still set only when every
  byte read on the line so far was a letter or digit, so the word must
  start in the first column.
- Any other word gives token 0x2C (`STRING`), with DS:7BFC set to 6.

Dispatch. 1B20:0000 takes the handle, a context, the token and an echo
flag, and looks the token up in 14 words at 1B20:05CF with their targets
after them: 0 (`INVALID KEYWORD`), 0x01 `@CLS`, 0x1B `@@`, 0x23 `@PAUSE`,
0x2C `STRING`, 0x2D `NUMBER`, 0x3A `@CHDIR`, 0x3B `@CHDRIVE`, 0x66 `@ABORT`,
0x78 `@QSTRING`, 0x7E `@BYTE`, 0x91 `@RMDIR`, 0x95 and 0x97 `@GETCWD`.

- Token 0x95 (1B20:021E) upper-cases the word, forms `LABEL_` and the word,
  and looks that up through 3044:011A. A name already defined returns 1. A
  new one is registered through 3044:01AD with type 6, and its record gets
  the byte at DS:7BA5 and the value 2D9F:01D3 returns (the further stores
  were not read).
- Tokens 0x2C and 0x7E (1B20:0477) return 0 when the echo flag is 0, and
  otherwise write the word through 35C0:004C with `%s`.
- A token not in the table returns 0, unless its keyword entry has class 1
  and the echo flag is set, when it is evaluated and written with `%ld`.

1B80:0147 handles tokens 0x58 to 0x5B only and returns 0 for any other.
192C:0003 returns 0 at once for any token but 0.

In the runner, a token not in its own table goes to 192C:0003, then to
1B20:0000 with echo flag 0, then to 1B80:0147, and stops through 213C:00A1
with the text at DS:7C02 when all three return 0. So a label defines
`LABEL_<word>` and the run goes on, and token 0x2C or 0x7E, a word that is
not a label, stops the run with a syntax error naming it.

`@DISPLAY`. Token 0x73 (`@DISPLAY`, and `@WELCOME`, which FND-RES-036 maps
to 0x73) runs 1B80:36DA and then 35C0:0DF7 (not read). 1B80:36DA reads
characters with comments on and writes each one through 35C0:0E1F until
`@`; the end of input stops the run with `end of file`. The `@` is pushed
back and a token read. Token 0x74 (`@ENDDISPLAY`, and `@ENDWELCOME`)
returns. Token 0x94 (`@GOTO`) is pushed back as a token and the routine
returns, so the runner runs it. Any other token goes to 1B20:0000 with
echo flag 1 and then 1B80:0147, and stops with a syntax error when neither
takes it; the loop then goes on.

Other loops. The routines run for `@GETOUTDRIVE` (1B80:289B), `@GETSUBDIR`
(1B80:3206) and `@GETOPTION` (19A2:049A) each contain a loop that reads
characters through 2D9F:028B and tests for `@`; what they do with the
characters was not read.

Script. Before `@Finish`, the shipped INSTALL.DAT has 24 lines whose first
byte after the leading spaces is neither `@` nor `//` and that do not start
inside a quoted string or a `/*` comment. Six are labels: a word of 3 to 10
letters and digits in the first column followed by `:`, one with a `//`
comment after it. The other 18 lie between a block command and its end
word: 6 between `@Display` and `@EndDisplay`, 5 between `@GetOutDrive` and
`@EndOutDrive`, 4 between `@GetOption` and `@EndOption`, and 3 between
`@GetSubdir` and `@EndSubdir`. No bare word stands outside these.

## Interpretation

Outside quoted strings and comments, the script holds only at-sign
commands, labels and block text. A label is a word in the first column
followed by a colon, stored as `LABEL_<word>` for `@GOTO`. Text inside
`@Display` is written to the screen as it stands, line breaks included,
and the at-sign tokens inside it go to the same handlers with echo on, up
to `@EndDisplay`. A
bare word anywhere else in the main run stops the program with a syntax
error.

## Alternatives

- Bare words are continuations of the previous command: ruled out for the
  main run, where they stop with a syntax error, and for `@Display` text,
  which is echoed byte by byte.
- An indented word with a colon is a label: ruled out; the line flag is
  cleared by the spaces before it.

## How to reproduce

Disassemble `CD:CONFIG.EXE` as 16-bit code with the load image at segment
0x1000 and relocation targets marked, at the ranges in Locations; read the
28 words at 1B20:05CF, keyword entries 0x2C, 0x73, 0x74, 0x7E and 0x95 at
DS:272F, and the strings at DS:16F1 and DS:1761. Walk INSTALL.DAT up to
`@Finish`, tracking quoted strings and both comment forms, and assign each
bare line to the innermost open block among the four pairs named above or
to the label form; report counts only. Keep listings in ignored local
storage.

Whole-function exclusive bounds follow the body extents recorded in FND-RES-062.
