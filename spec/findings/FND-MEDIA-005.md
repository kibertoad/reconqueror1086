---
id: FND-MEDIA-005
title: Pictures are drawn by 0x00040ED0, which decodes PCX rows straight to the screen and can load the trailing palette
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00040ED0..0x00041105
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0007331C..0x00073354
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006DF1C..0x0006DF8F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00040CE4..0x00040E70
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00041120..0x00041151
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00040ED0(x, y, index, name, set_palette)` takes the directory record `0x000495C4(index)`, or
`0x0004957C(name)` when `index` is -1, allocates its expanded size and reads it with `0x000495F4`,
stopping the game through `0x000636D0` with `Unable to find pcx data in resource file`, `Unable to
alloc enough memory for pcx data` or `Unable to Get Pcx data from resource file!`. It then requires
byte 0 to be `0x0A`, byte 1 to be 5 and byte 3 to be 8; otherwise it stops the game with error code 4
and the name. It reads no other header field but the four `UINT16LE` bounds at `0x04` to `0x0A`:
the width is `xmax - xmin + 1`, raised by one when odd, and the height `ymax - ymin + 1`.

From byte `0x80` it decodes `height` rows with `0x0007331C(dest, source, width)`, one row at
`0x000B0840 + (y + row) * 0x000B0854 + x` each: a byte whose two top bits are set is a count in its
low six bits followed by the byte to repeat; any other byte is one pixel. The routine stops once
the row holds `width` pixels or more and returns where it stopped, so a run can write past the row.
When `set_palette` is not 0 and the byte 769 before the end of the entry is `0x0C`, it copies the
last 768 bytes as 256 three-byte colours into the buffer pointed to by `0x000B0844`, four bytes
apart, through `0x0006DF1C`. It passes the drawn rectangle to `0x0006DF90` and frees the entry.

`0x00040CE4` reads a PCX from a disk file instead: it opens it with mode `rb`, reads the 128-byte
header, checks `0x0A`, bits 8 and version 5, seeks to 769 bytes before the end, requires `0x0C`,
reads the 768 palette bytes and decodes the rows through `0x00040C60` into a buffer it allocates. It
is called from `0x00025C06`, `0x0004C506` and `0x00052FD4`.

`0x00040ED0` has 11 direct callers. `0x00041120(n)` formats `lance%1d.csf` and loads it with
`0x00018430(name, 1)` into the dword at `0x000AC170`.

## Interpretation

The game draws only 8-bit single-plane PCX pictures with version 5 and RLE rows, and takes their
palette only on request (FMT-MEDIA-003). The encoding byte, plane count and bytes per line are
ignored, so the stored rows must be exactly the even-rounded width.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00040ED0`, `0x0007331C`, `0x0006DF1C` and `0x00040CE4` in `CD:CONQUER.EXE`; read
the strings at `0x00096114` to `0x00096198`.
