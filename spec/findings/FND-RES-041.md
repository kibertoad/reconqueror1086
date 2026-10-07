---
id: FND-RES-041
title: CONFIG.EXE stops its script run at @FINISH and, just before exiting, reopens the script there to echo text and run commands up to @ENDFINISH
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:0007..1B80:0146
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:184F..1B80:198A
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:1F26..1B80:200B
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1B80:532B..1B80:573D
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 1A72:06BE..1A72:06E7
  - build: BLD-GOG-EN
    file: CD:CONFIG.EXE
    address: 35C0:0E1F..35C0:0E7E
  - build: BLD-GOG-EN
    file: CD:INSTALL.DAT
    offset: 0x00..0xBDDB
tool: capstone 5.0.7 bounded 16-bit disassembly with relocation targets marked; Python byte search of the load image; Python walk of the shipped INSTALL.DAT
environment: null
---

## Observation

Addresses, DS, the byte and character layer (2D9F:003D, 2D9F:028B,
2D9F:037B), the token reader 221F:0003, the keyword table at DS:272F and the
stops 213C:0007 and 213C:00A1 are as FND-RES-034 to FND-RES-037 give them.
The script handle and open are FND-RES-035's. Segment 1B80 is longer than
0x5300 bytes: the near call at 1B80:536A reaches 1B80:00A5 only through the
16-bit wrap of its offset.

The runner. 1B80:184F reads a token, dispatches it (16 tokens through the
word table at 1B80:2135 and the 16 targets after it, others through
192C:0003, 1B20:0000 and 1B80:0147, and a syntax error with the name at
DS:7C02 when none takes it), and reads the next. It leaves the loop when
the token read is 0xFFFF, the end of input, or after token 0x28 (`@FINISH`,
keyword entry 40). The `@FINISH` case at 1B80:1F26 sets the word at DS:1768
to 1 and the word at DS:176A to 0, clears the byte at DS:1787, calls
1B80:0007 and sets the handle to 0xFFFF. 1B80:2672 writes the same three
values; what reaches it was not read.

1B80:0007 keeps the value 2D9F:01D3 (not read) returns for the handle at
DS:177F, the 32-bit line count at DS:1783 and the 32-bit count of bytes
left at DS:7B93. When DS:1787 is set it also calls three routines of
segment 35C0 (not read). It then closes the handle through 1000:3D1F and
sets it to 0xFFFF. 1B80:00A5 opens the script again through 2D43:0003,
resets the byte layer through 2D9F:0006, seeks to the kept value from the
start of the file through 1000:0A1F, stops with the message at DS:17AE when
that returns -1, and restores the count of bytes left, the line count and
the 32-bit value at DS:5366 from DS:7B97. 2377:4693 and 2377:46B5 call the
pair around a program the script runs, and 2377:47FF also calls 1B80:00A5.

The finish executor. Main ends at 1A72:06BE by calling 1B80:532B with 0 and
the far pointer at DS:7B7E, then three routines (2375:000F, 2375:000D,
2377:6414) and returns 0. 1B80:532B does nothing unless DS:1768 or DS:176A
is nonzero. Otherwise it calls 35C0:1D06, copies at most 0x3B bytes of the
text its pointer names through 1890:0132, calls 2377:5A92, and reopens the
script through 1B80:00A5. Then it repeats:

1. Characters are read through 2D9F:028B with comments on until `@`. Each
   other character goes to 35C0:0E1F, which reads the cursor through
   interrupt 10h function 3 and goes on to write the character (the rest
   was not read). The end of input before an `@` stops the run through
   213C:00A1 with `end of file`.
2. The `@` is pushed back and a token read through 221F:0003.
3. Token 0x0B (`@ENDFINISH`, keyword entry 11) calls 35C0:0DF7 (not read)
   and returns.
4. Token 0x23 (`@PAUSE`) writes a line feed and `Press any key to continue
   ...` through 35C0:124C, reads a key through 35C0:1BDF, calls 35C0:082A
   with 0 when the key is 0x1B, and writes a line feed.
5. Token 0x34 (`@EXECUTE`) and token 0x94 (`@GOTO`) have their own cases
   (1B80:5418 and 1B80:55A1, read only to their first calls).
6. Any other token goes to 1B20:0000 with the handle, a buffer, the token
   and 1, then to 1B80:0147; when neither takes it, the run stops through
   213C:00A1 with the text at DS:7C02.

Search. A search of the load image for far calls to 1B80:532B (bytes `9A
2B 53`) and for near calls whose 16-bit target in segment 1B80 is 0x532B
found the one far call at 1A72:06C9. The same two searches for 1B80:00A5
found 2377:46B5, 2377:47FF and 1B80:536A, the three calls located by
reading, as a positive control. Calls through pointers were not searched.

Script. The shipped INSTALL.DAT holds `@Finish` once, at offset 47280, and
`@EndFinish` once, as its last ten bytes with no line end after them. The
1,323 bytes from `@Finish` to the end hold 28 CR LF pairs and the names
`@CHDIR`, `@CHDRIVE`, `@ECHO`, `@ELSE`, `@EMMAVAIL`, `@ENDFINISH`,
`@ENDIF`, `@EXECUTE`, `@EXTAVAIL`, `@FINISH`, `@GETCWD`, `@IF`, `@OPTION`,
`@OUTDRIVE`, `@STARTUPDIR`, `@STARTUPDRIVE`, `@SUBDIR` and `@WRITE`, in
upper case.

## Interpretation

`@FINISH` ends the main run and marks where the finish block starts. When
the program exits, the block runs: everything from `@FINISH` to
`@ENDFINISH` that is outside an at-sign command is written to the screen,
line breaks included, and the commands in it run. The final region of the
shipped file is the block's `@ENDFINISH`. The name reader ends a name at
the end of input as at any other byte (FND-RES-036), so the missing line
end changes nothing; a file that ended inside the block without
`@ENDFINISH` would stop with `end of file`.

## Alternatives

- The main runner reads `@ENDFINISH`: ruled out; it closes the script at
  `@FINISH` and returns.
- 1B80:00A5 is the finish block's own reader: ruled out; it only reopens
  the script, for 1B80:532B and for the program runners in 2377.

## How to reproduce

Disassemble `CD:CONFIG.EXE` as 16-bit code with the load image at segment
0x1000 and relocation targets marked, at the ranges in Locations, and
1B80:00A5's callers at 2377:4693..46D8 and 2377:47FF. Read the 32 words at
1B80:2135, keyword entries 11, 35, 40, 52 and 148 at DS:272F, and the
strings at DS:1BFE, DS:2329 and DS:1B75. Search the load image as Search
says. Walk INSTALL.DAT from `@Finish` to the end; report counts and names
only. Keep listings in ignored local storage.
