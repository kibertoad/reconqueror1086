---
id: FND-RES-004
title: Kind 2 is one LZW bit stream with codes of 9 to 14 bits, read most significant bit first
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000486B8..0x000488EF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004819C..0x00048213
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00048658..0x000486B7
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B1
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect) and a container census written for this project
environment: null
---

## Observation

`0x000486B8(src, out, stored)` allocates a 36,082-byte prefix table, a 18,041-byte suffix table and
a 18,041-byte stack, sets the code width at `0x000AE5D2` to 9 and the mask at `0x000AE5D4` to
`0x1FF`, and reads codes through `0x0004819C`. That routine keeps a 32-bit bit buffer at `0x000AE5A8`,
fills it a byte at a time from the high end, and returns its top `width` bits, so codes are read
most significant bit first. The loop ends at code `0x101` or when the input pointer passes
`src + stored`. The first code after the start or after a clear is written as a byte. Code `0x100`
resets the width to 9, the mask to `0x1FF` and the next code to `0x102`. Any other code below the
next code is expanded through the tables by `0x00048658`; a code at or above the next code is
expanded as the previous string followed by the previous string's first byte. When the next code is not above
the mask, the routine stores the previous code and the first byte of the current string as the next
code and increments it, and when the new next code equals the mask and the width is below `0x0E` it
increments the width and sets the mask to `(1 << width) - 1`. It frees the three tables and returns.

The five kind-2 entries of `C1086.GOB` decode this way to their expanded sizes. All reach 14-bit
codes. `configit.666`, `prog.pcx`, `champion.pcx`, `crowning.pcx` and `death.pcx` contain 1, 5, 3, 3
and 1 clear codes, and 4 or 5 bytes follow the end code in each.

An earlier reading of the executable placed this decoder at `0x00045C6E`, its bit reader at
`0x000456F4` and its dictionary helper at `0x0004576C`. Those addresses came from a page mapping
that added the LE data-pages offset to the LE header's file offset, `0x2AA8` bytes too far; the
data-pages offset counts from the start of the embedded MZ module at file offset `0x26654`, so
the pages begin at file offset `0x4C254`. With that mapping `0x00045C6E` is inside the renderer.

## Interpretation

Kind 2 is LZW with clear code 256, end code 257, first free code 258, and a width that grows when
the next code reaches the current mask, one code earlier than the usual rule, up to 14 bits. At 14
bits the dictionary stops growing at code 16,383.

## Alternatives

Reading the stream with a 12-bit ceiling gives the first bytes of each image right and then loses
alignment; resetting at byte, word or dword boundaries also fails. Those readings are wrong.

## How to reproduce

Disassemble the three ranges, and decode the five kind-2 entries with the growth rule described.
