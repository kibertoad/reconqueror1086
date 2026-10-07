---
id: FND-RES-047
title: INSTALL.SCR's alert, space, pick, godir, exists, testdir and del commands in INST.EXE
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:0CA2..1C17:0FDC
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:1AC6..1C17:1C62
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1C17:1F1B..1C17:1F3A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1E1A:0073..1E1A:0133
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 263F:004D..263F:01ED
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2CBD:00B6..2CBD:0142
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2CB5:0050..2CB5:0063
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2FA4:0005..2FA4:0025
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 2FA4:0118..2FA4:0136
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:027A..1000:0290
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:02C2..1000:02D9
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:043D..1000:0474
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:0C53..1000:0C6A
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:1A62..1000:1A79
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 1000:4113..1000:4149
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:0F33..3583:1016
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:115A..3583:1169
  - build: BLD-GOG-EN
    file: CD:INST.EXE
    address: 3583:2BFB..3583:2C15
tool: capstone 5.0.7 bounded 16-bit disassembly of the LZEXE-unpacked file with relocation targets marked; Python byte searches of the CD-root text files
environment: null
---

## Observation

The unpacked file, its notation, the command table, the dispatch and its
argument, the message lookup 2852:0B71 and the message box 1C17:0745 are as
FND-RES-055 gives them. Every routine here reads its argument with 1000:620F
and a `%s` format, into stack buffers with no width given.

Helpers read here:

- 2CBD:00B6 lower-cases one byte: `A` to `Z` add 0x20, 0x80 to 0xA5 go
  through the byte table at DS:389E, anything else is kept. 2CBD:010A
  applies it to each byte of a string in place.
- 2FA4:0005 reads a key through 1000:3290 (not read); when that returns 0 it
  reads again and returns that value shifted up 8 bits. 2FA4:0118 lower-cases
  a value of 0xFF or less through 2CBD:00B6 and returns larger values as they
  are.
- 1C17:1F1B returns the low word of 1000:414A (not read) on its string.
- 1000:043D is DOS function 0x36 for the drive number it is given. It
  stores DX, BX, AX and CX at offsets 0, 2, 4 and 6 of its record and returns
  0, or, when AX is 0xFFFF, stores 0x13 at DS:007F and returns without
  writing the record.
- 1000:02C2 is DOS function 0x3B (change directory), 1000:0C53 is 0x39 (make
  directory) and 1000:1A62 is 0x41 (delete file); each returns 0 or passes
  the DOS error to 1000:0B2C. 1000:0900 passes its argument plus 1 to
  1000:0686 (not read). 1000:4113 (name, mode) calls 1000:4254 (not read) and
  returns -1 when that does, and with mode 0 returns 0 otherwise.
- 1E1A:0073 (bytes, out) writes the bytes as megabytes with one decimal into
  `out`: the whole part is bytes >> 20; the remainder below 2^20 is
  multiplied by 100 and shifted right 10 twice, divided by 1000, and printed
  as one digit with `%-*.*u` and the width and precision 1; `out` is `%u.%s`
  of the two.
- 263F:004D (text, colours) fetches the texts for `yn` (DS:2BFB) and
  `ynPrompt` (DS:2BFE), calls 263F:02BF with the text, the prompt and the
  `yn` text as the allowed keys, and returns 1 when the key it returns
  equals the first byte of the `yn` text through 2012:0DB0 (not read), and
  0 otherwise.
- 263F:0133 (format, ...) formats into a 0x138-byte buffer, shows it with
  the text for `pressKeyToExit` (DS:2C07) through 263F:02BF, and calls
  1000:027A, which writes `Abnormal program termination` (DS:003D, 0x1E
  bytes) through 1000:0272 and calls 1000:086A with 3. Neither returns to
  its caller.

`alert`, 1C17:0CA2 (text), calls 1C17:0745 with the text as the format and
no further arguments, and returns.

`space`, 1C17:0CBF (text):

- When the doubleword at +0x19E of the object at the far pointer DS:1A04 is
  not 0, it calls 263F:0133 with the text at DS:0F33, which says that the
  command cannot be used together with a `space=` line in `RESOURCE.CFG`.
- It reads three words with `%s %s %s` (DS:0FF4) into 40-byte buffers and
  lower-cases the first.
- It calls 1000:043D with the first word's first byte minus 0x60, so `a` is
  drive 1, and multiplies the words at record offsets 2, 6 and 4 (available
  clusters, bytes per sector, sectors per cluster) as 32-bit unsigned values,
  keeping the low 32 bits. The free kilobytes are that product >> 10.
- It writes the product through 1E1A:0073 into the bytes at +0x1AE of the
  object at DS:1A04.
- When the low word of the second word's number, taken unsigned, is greater
  than the free kilobytes, it calls `goto` (1C17:0C14) with the third word.

`pick`, 1C17:0D8F (text):

- It reads five words with `%s %s %s %s %s` (DS:0FFD): a key list into a
  buffer at [bp-8] and four labels into 40-byte buffers at [bp-0xAA],
  [bp-0x82], [bp-0x5A] and [bp-0x32]. The key list is lower-cased.
- It loops: it reads a key through 2FA4:0005 and lower-cases it through
  2FA4:0118. When 1000:6295 finds the key's low byte in the key list, it
  forms the one-character string of the key with `%c` (DS:100C) in the two
  bytes at [bp-2], takes n as the count of leading key-list bytes not in it,
  calls `goto` with the label buffer at [bp-0xAA] plus 0x28 times n, and
  returns. Otherwise it calls 1000:558D (not read) with the byte 0x07
  (DS:100F) and reads another key.
- A key whose low byte is 0 matches the key list's terminating NUL; n is
  then the length of the key list.

`godir`, 1C17:0E4A (text):

- It reads a path and a label with `%s %s` (DS:1011) and lower-cases the
  path. When the path has a `:`, it calls 1000:0900 with the byte before it
  minus `a`, and the scan pointer p starts after the `:`; otherwise p starts
  at the path.
- A try is: change to the path; when that fails, make it; when that fails,
  change to it again.
- When the try fails, it looks for the next `\` from p. With none, it calls
  `goto` with the label and returns. Otherwise it ends the path at that
  `\`, makes the directory named so far, puts the `\` back, moves p past it,
  and tries again.
- When a try succeeds, it tries once more and calls `goto` with the label
  only when that last try fails in all three steps.

`exists`, 1C17:0F7C (text), reads one word with ` %s` (DS:1017) into a
60-byte buffer. While 1000:4113 with mode 0 returns other than 0 for it, it
calls 2CB5:0050, which calls 2CB5:0064 (not read) with 0x514 and 0xDC, and
then `alert` with the text after the first word (1C17:08C1, then 1C17:08F9
skipping spaces and tabs). When the file is found it returns.

`testdir`, 1C17:1AC6 (text):

- It reads one word with `%s` (DS:115A) and lower-cases it; with a `:` it
  calls 1000:0900 as `godir` does.
- When changing to the directory fails, or when 1000:45DF finds nothing for
  `*.*` (DS:115D) with attribute 0 there, it returns.
- Otherwise it fetches the text for `dirFound` (DS:1161), combines it with
  the directory through 2FD0:0647 (not read), and calls 263F:004D with the
  result and two colour bytes from the table at DS:0C2E. When that returns
  nonzero it sets DS:0CAC to 1, which ends the script.

`del`, 1C17:1C17 (text), calls 1000:45DF with the text, a 43-byte record at
[bp-0x2C] and attribute 0. While the result is 0 it deletes the name at
[bp-0xE] through 1000:1A62 and calls 1000:4612 (not read) with the record.

A byte search of `INSTALL.HLP`, `LANGUAGE.INF`, `SIERRA.INF` and
`INSTALL.DAT` finds none of the keys `dirFound`, `ynPrompt`, `pauseMsg` and
`enterEsc`; the unpacked `INST.EXE` holds each.

## Interpretation

- `alert` shows its text in a box; Enter goes on and any other key leaves
  the installer, as for every 1C17:0745 box.
- `space drive kilobytes label` jumps to the label when the drive has less
  free space than the number given, and leaves the free space in megabytes,
  with one decimal, in the string `%7` points to when that is the same
  object. It stops the installer when `RESOURCE.CFG` sets `space`.
- `pick keys label1 label2 label3 label4` waits for one of the keys, without
  regard to case, beeping at others, and jumps to the label in the key's
  position.
- `godir path label` makes the path the current drive and directory,
  creating each missing level, and jumps to the label when it cannot.
- `exists file message` repeats the message box until the file exists.
- `testdir directory` asks a yes/no question when the directory exists and
  holds any file, and ends the script on the first answer letter.
- `del pattern` deletes the files the pattern finds, by bare name, so in the
  current directory whatever directory the pattern names.

## Alternatives

- `space` measures the whole disk: ruled out; the record offsets multiplied
  are those DOS function 0x36 fills from BX, CX and AX, the free clusters,
  bytes per sector and sectors per cluster.
- `pick` shows its own menu: ruled out; it reads keys and writes nothing but
  the beep call.
- The message texts are read from `INSTALL.HLP`: not supported for these
  keys, which no CD-root text file holds.

## How to reproduce

Unpack `CD:INST.EXE` as FND-RES-054 gives. Disassemble the ranges in
Locations as 16-bit code with the load image at segment 0x1000 and relocation
targets marked, and read the strings at DS:003D, DS:0F33 to DS:1016, DS:115A
to DS:1169 and DS:2BFB to DS:2C15. Search the CD-root text files named above
for the four key strings. Keep listings in ignored local storage.
