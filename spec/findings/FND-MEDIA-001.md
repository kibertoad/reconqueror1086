---
id: FND-MEDIA-001
title: CSF sprite files load through 0x00018430, which checks the two bytes 2J and builds a frame table and row tables
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018430..0x0001869B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001869C..0x0001884A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006A360..0x0006A3F0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002A756..0x0002A765
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000644B0
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00018430(name, from_archive)` allocates a 16-byte record. When `from_archive` is 0 it probes the
file `name` through `0x0006A190` (stopping the game with `CSF files does not exist` when that
fails), gets its size through `0x0006A1DC`, allocates that many bytes into the record's `+0x04` and
reads the file into them through `0x0006A210`. Otherwise it looks the name up with `0x0004957C`,
allocates the entry's expanded size (`+0x2C` of the record) and reads it with `0x000495F4`. A missing
entry prints the name and stops the game with `CSF files does not exist in GOB`.

The loaded bytes are then checked: the first two must equal the two bytes at `0x0009E034`, `2J`
(`0x32 0x4A`), or the game stops with `CSF is incorrect version`. The four bytes after them are
copied into the record's `+0x00`. It allocates `4 * count` bytes for a frame pointer array and
fills it walking the `count` `UINT32LE` sizes that follow: frame 0 starts right after the size
table and each next frame starts at the previous one plus its size. It allocates a second array of
`count` pointers (stopping with `Error allocating memory for CS clip offsets` when that fails),
fills it with `0x0006A360(frame)` for every frame, and stores it at `+0x08` and the frame array at
`+0x0C`. Every allocation failure stops the game with a message from `0x000636D0`, which prints
`ERROR: <kind> <text>. Program Aborted` and exits with the code plus 100.

`0x0006A360(frame)` reads the `UINT16LE` at frame `+2` (the height), allocates `4 * height + 4`
bytes and walks the rows from frame `+4`: it records each row's start, reads the row's segment
count byte, and steps over each segment (an operation byte and an `INT16LE` length, followed by
`length` bytes for operation 0, one byte for operation 2 and nothing otherwise). It returns the array
of row starts.

`0x0001869C(index)` does the same from `0x000495C4(index)`, the directory record at that index, with
the same `2J` check and row tables.

The game loads `CONFONT.CSF` at `0x0002A756` through `0x000644B0("CONFONT.CSF", 1)`, a wrapper
that calls `0x00018430`, and keeps the record in the dword at `0x0009E03C`. `0x00018430` has 24
direct callers.

## Interpretation

A CSF is a count, a size table and the frames packed in table order (FMT-MEDIA-001); the first two
bytes are a version tag the game requires. Each frame is a width, a height and run-length rows
(FMT-MEDIA-002). The row tables serve the clipped drawing routine of FND-MEDIA-003.

## Alternatives

The two bytes could be read as a magic number rather than a version; the error text calls them a
version, and the game treats them the same way either way.

## How to reproduce

Disassemble `0x00018430..0x0001869B`, `0x0001869C..0x0001884A` and `0x0006A360` in `CD:CONQUER.EXE`;
read the string at `0x0009E034`.
