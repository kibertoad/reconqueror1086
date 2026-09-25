---
id: FND-RES-002
title: One archive is open at a time; the game opens it, finds an entry by name or index, and reads it by its kind
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049200..0x0004945F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049478..0x00049573
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049574..0x00049579
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004957C..0x000495C1
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000495C4..0x000495E0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000495E4..0x000495F3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000495F4..0x000497AA
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049460..0x00049476
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000733BF..0x000733FF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The game keeps one archive's state in five globals: the open file at `0x0009C9C0`, the end of the
entry data (the directory offset) at `0x0009C9B0`, a changed flag at `0x0009C9B4`, the entry count
at `0x0009C9B8`, and a pointer to the directory records at `0x0009C9BC`.

`0x00049200(path, mode)` appends the extension at `0x00097240`, `.RES`, when `path` has no `.`,
copies the path to `0x000AE4F0` and opens the file with `mode`. It returns -1 when the open fails.
When `mode` starts with `w` or `W` it writes `.RES` and the dword 8, and sets the data end to 8,
the changed flag to 1 and the count to 0. Otherwise it reads four bytes and compares them with
`.RES`, reads the directory offset from bytes 4 to 7, seeks to it, reads the count, allocates
`count * 52` bytes and reads that many 52-byte records, clears the changed flag and returns the
count. Any short read or a failed compare returns -1.

`0x00049478(sort)` does nothing when no file is open. When the changed flag is set or `sort` is not
0, it first sorts the records when `sort` is not 0 (`qsort` at `0x00070A8E` with the compare
`0x00049460`, which compares the first 31 characters ignoring case), writes the data end as the
directory offset at byte 4, and writes the count and the records at the data end. It then closes
the file, clears the file global and frees the records.

`0x00049574()` returns the count. `0x0004957C(name)` walks the records from the first and returns
the first whose name `0x000733BF` finds equal to `name`, or 0. `0x000733BF` compares byte by byte
and turns `A` to `Z` into `a` to `z` on both sides before each compare. `0x000495C4(i)` returns the
address of record `i` without checking `i`.

`0x000495F4(record, out)` returns 0 when either argument is 0, seeks to the record's offset
(`+0x30`) and returns 0 when that fails, and then dispatches on the kind at `+0x20` through the table
at `0x000495E4`: 0 to `0x0004964B`, 1 to `0x00049672`, 2 to `0x000496DA`, 3 to `0x00049743`. A kind
above 3 returns 0. Kind 0 reads the stored size (`+0x28`) straight into `out`. Kinds 1 to 3 allocate
the stored size, read the stored bytes into that buffer, and pass it to `0x00048148(src, out,
stored)` for kind 1, `0x000486B8(src, out, stored)` for kind 2, or `0x000491D4(src, out, stored,
expanded)` for kind 3, then free the buffer. Each successful path returns the expanded size
(`+0x2C`). A short read returns 0; for kinds 1 to 3 it leaves the buffer allocated.

## Interpretation

The record layout is: name `+0x00` (32 bytes), kind `+0x20`, `+0x24`, stored size `+0x28`,
expanded size `+0x2C`, offset `+0x30`. The reader chooses the decoder by the kind alone; the two
sizes are never compared. Name lookup ignores ASCII case and takes the first match. The caller
supplies an output buffer of at least the expanded size, since the decoders do not check it.

## Alternatives

None known.

## How to reproduce

Disassemble the listed ranges and follow the references to the five globals and to the table at
`0x000495E4`.
