---
id: FND-RES-060
title: FIEF0.DAT is a text file of six sections, each a column line and a line of integers, read when a new game sets up the home fief
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000297A0..0x000298B7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029968..0x00029A3F
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0xC29B3F..0xC29E47
tool: Python 3.14 LE object, page and fixup mapping of CONQUER.EXE with capstone 5.0.7 bounded 32-bit disassembly; Python container reader per FMT-RES-001 to FMT-RES-003
environment: null
---

## Observation

`0x000297A0` allocates a 56-byte fief record, writes the archive entry
`FIEF0.DAT` out to a loose file through `0x00049BA8` (FND-SAVE-003 describes
that routine), opens it with mode `r` through `0x00063612`, and passes the
stream in turn to `0x00029968`, `0x00013520`, `0x0005F480`, `0x0001C410` and
`0x000309D0`, storing the last four results at the record's `+0x28`, `+0x2C`,
`+0x34` and `+0x30`. It closes the stream, stops the game through
`0x00064FF5` when the close fails, and deletes the loose file through
`0x000681D1`.

`0x000299E0` reads lines of up to 256 bytes through `0x00065025` and skips
each line whose first byte is a space, `#`, CR or LF; it returns at the first
other line. `0x00029968` calls it, reads the next line, and parses it with
`%d %d %d` into the record's `+0x00`, `+0x10` and `+0x14`, stopping the game
when fewer than three values are read or a line is missing. `0x00013520`
parses with `%d %d %d %d %d %d %d` among others, and `0x000309D0` with nine
`%d`; `0x0005F480` uses a format of sixteen `%d`. The formats of the other
calls in `0x00013520` and `0x0001C410` are formed in registers and were not
traced.

The decoded entry is 1,752 bytes of CRLF text in 53 lines: `#` comment lines,
blank lines, and six sections, each a line starting with `@` that names the
columns followed by one line of integers. The six data lines hold 3, 12, 7,
13, 10 and 9 values.

## Interpretation

FIEF0.DAT is the starting state of the home fief, written as text. The `@`
line of each section is the line the skipper stops at and is never parsed;
the line after it is. The first section is the fief record's three values;
the others belong to the four readers in order, with `0x00013520` reading two
sections (12 and 7 values).

## Alternatives

- The `@` lines are parsed: ruled out for the first section, where the line
  after the skipped lines is consumed by the skipper's return and the next
  line is parsed.
- Each reader reads one section: contradicted by the counts, six sections for
  five readers; which reader reads two is inferred from the value counts
  only.

## How to reproduce

Map CONQUER.EXE as FND-RES-067 gives and disassemble the ranges above; read
the format strings each `0x00065108` call pushes. Decode `fief0.dat` from
C1086.GOB and count its lines and each data line's values.
