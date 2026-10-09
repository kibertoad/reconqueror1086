---
id: FND-RES-043
title: CONFIG.EXE's @GetOutDrive, @GetSubdir and @GetOption echo their block text to the screen like @Display
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:289B..1B80:2A3A
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:3206..1B80:33AA
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 19A2:049A..19A2:054F
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:1DEC..1B80:1E2D
tool: capstone 5.0.7 bounded 16-bit disassembly with relocation targets marked
environment: null
---

## Observation

Addresses, DS, the class table at DS:6D7B, the character layer, the token
reader 221F:0003, the keyword table at DS:272F, the echo routine 35C0:0E1F
and the stop 213C:00A1 are as FND-RES-036, FND-RES-064 and FND-RES-042 give
them. The runner's table sends token 0x0F (`@GETOUTDRIVE`) to 1B80:289B,
0x10 (`@GETSUBDIR`) to 1B80:3206 and 0x3C (`@GETOPTION`) to 19A2:049A.

Each of the three routines has a loop of the same shape: read a character
through 2D9F:028B with comments on; when it is neither `@` nor the end of
input, write it through 35C0:0E1F and read the next. The end of input stops
the run through 213C:00A1 with `end of file` (the copies at DS:1BFE and
DS:11DB). The `@` is pushed back through 2D9F:037B and a
token read through 221F:0003. The loops are at 1B80:29C7..1B80:29E9,
1B80:3338..1B80:335A and 19A2:04EF..19A2:0511.

- 1B80:289B leaves for its end at token 0x08 (`@ENDOUTDRIVE`) and has cases
  for 0x24 (`@PROMPT`), 0x7C (`@SUPPRESS`) and 0x94 (`@GOTO`), and others.
- 1B80:3206 leaves for its end at token 0x0A (`@ENDSUBDIR`) and has cases
  for 0x24 (`@PROMPT`), 0x7B (`@DEFAULT`) and 0x94 (`@GOTO`), and others.
- 19A2:049A leaves for its end at token 0x3E (`@ENDOPTION`) and sends other
  tokens to a four-entry table.

Before that loop, 1B80:289B and 1B80:3206 each first echo the white space
(class bit 0) that follows the command, through 35C0:0E1F, and test the
next character for `@`. What the cases and the code after each loop do was
not read.

## Interpretation

Text in the blocks of `@GetOutDrive`, `@GetSubdir` and `@GetOption` is
written to the screen as it stands, line breaks included, as `@Display`
text is (FND-RES-042). Together with FND-RES-064 and FND-RES-042 this accounts for every
bare line of the shipped INSTALL.DAT.

## Alternatives

- The block text is kept as a prompt string: ruled out for the text before
  each `@`, which the loops write out byte by byte and keep nowhere.

## How to reproduce

Disassemble `CD:CONFIG.EXE` as 16-bit code with the load image at segment
0x1000 and relocation targets marked, at the ranges in Locations, and read
keyword entries 0x08, 0x0A, 0x24, 0x3E, 0x7B and 0x7C at DS:272F and the
strings at DS:1BFE and DS:11DB. Keep listings in ignored local storage.
