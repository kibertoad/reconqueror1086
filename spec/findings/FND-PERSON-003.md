---
id: FND-PERSON-003
title: CHARACTR.DAT is parsed by 0x00015920 and 0x00016124, which shift row 0 by up to 8 on each of the first 15 fields
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015920..0x00015A9F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015B80..0x00015D52
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015D54..0x00015EA0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00016008..0x0001629F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00024C4C..0x00024C9C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000682C0..0x00068324
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00015920` allocates the 16-byte table header that the pointer at `0x0009A650` points to: `+0x00`
the list of row records, `+0x04` the list of attribute names, `+0x08` the number of attributes and
`+0x0C` the number of characters. It opens `CHARACTR.DAT` in mode `r`, then:

- `0x00016008` reads a line through `0x00024C4C`, which reads lines with `fgets` into a 256-byte
  buffer and skips any whose first byte is a space, `#`, CR or LF. It ignores that line's content,
  reads the next line with `fgets`, and scans it with `"%d %d"` into the number of characters and
  the number of attributes. Anything else ends the program through `0x00064FF5(1)`.
- It allocates one 25-byte name for each attribute. `0x00016078` requires the next line from
  `0x00024C4C` to start with `>`, then reads lines with `fgets` while each starts with `!`, scanning
  the text after the `!` with `"%s"` into the next name. It returns the count on the first line
  that does not start with `!`, which it has then consumed. The count must equal the header's.
- For each character it allocates a row record with `0x000146B0` and calls
  `0x00016124(file, row, 0)`. That requires the next line from `0x00024C4C` to start with `@`,
  reads the next line with `fgets` and copies up to 80 bytes of it with `strncpy` into the name at
  record offset 0, or stores an empty name when the line starts with `#`, a space, LF or CR. It
  then requires the next line from `0x00024C4C` to start with `>`, reads the next line with
  `fgets` and converts up to the attribute count of its tokens, split on space, tab and comma,
  with `atoi` into the attribute list at record offset `0x50` (`0x000682C0`). The number converted
  must equal the attribute count. The loader stores 0 at record offset `0x54`.

When its third argument is 0 and the row is 0, `0x00016124` then walks fields 0 to 14 of row 0.
For each it draws `0x00024C38(1)` and then `0x00024C38(8)`, negates the second when the first is
not 0, and adds it to the field with a direct store that applies no limit.

`0x00015D54(path)` loads a named file in mode `rt` in the same way but passes 1, so it applies no
shift; the load at `0x0004B0E3` calls it. `0x00015B80(path)` writes the table in mode `wt` in the
same layout: the header line, the two counts, `>ATTRIBUTES`, one `!name` line for each attribute,
then for each character two empty lines, `@NAME`, the name, a header line of attribute
abbreviations and the 30 values separated by spaces. The save at `0x0004AA5D` calls it. The
startup call at `0x0002A010` and the call at `0x00015267` load `CHARACTR.DAT` through
`0x00015920`.

## Interpretation

Every time the game builds a character table from `CHARACTR.DAT`, the player's first 15
attributes move by -8 to 8 each, and may leave 0 to 20. A saved game is reloaded without the shift.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00015920` to `0x00015A9F`, `0x00015B80` to `0x00015EA0`, `0x00016008` to
`0x0001629F`, `0x00024C4C` and `0x000682C0`, and read the object-2 strings the pushes name.
