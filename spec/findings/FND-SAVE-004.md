---
id: FND-SAVE-004
title: What the saved files hold: forces, properties, persons, items, conversation variables, fief 0, armies, calendar and tournament
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000109AC..0x00010CB2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00010CB4..0x00010FEC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003E978..0x0003E9AD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00042AF0..0x00042C8E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00042C90..0x00042E4A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000623EC..0x0006260E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000385B4..0x0003866B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002B3E0..0x0002B5D8
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002B5DC..0x0002BC00
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00029C5C..0x00029D20
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005C3FC..0x0005C520
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Every component file is written with `fwrite` (`0x00063748`) and read back with `fread`
(`0x00063ABA`) in the same order.

`0x000109AC` writes `troops.sav` (mode `wb`; `Unable to Save troops.sav`): 4 bytes from `0x0009A560`,
32 from `0x000AA498`, then 4 bytes each from `0x0009AEEC`, `0x0009AE50`, `0x0009AE4C`, `0x000AB0C8`,
`0x000AB0C0`, `0x0009B610`, `0x0009AE5C`, `0x0009AE60`, `0x0009AE64`, `0x0009AE68`, `0x0009AE58`,
`0x000AB0C4`, `0x000AB0CC`, `0x000AB0D4`, `0x000AB0D0`, `0x0009AE6C` and `0x000A7540`; the six
280-byte records at `0x000AA4B8`; for each of the three records at `0x000AA150`, the 32-byte order
at `0x000AA0F0 + 32 * i`, the record, and a 4-byte marker; for each of the five records at
`0x000AAB48`, the record and a marker; then, through `0x0003E978`, 4,028 bytes from `0x000AB170`.
The marker is `0x1111` followed by `8 * count` bytes from the record's `route` when `complete` is 0,
`route` is not null and `count` is not 0, and `0xFFFF` otherwise. `0x00010CB4` first calls
`0x00010FF0`, reads the same fields, allocates `8 * count` bytes for `route` after each `0x1111`
(`Unable to Allocate for Route in LoadTroops` on failure), and at the end frees the conversation
variables at `0x0009A928` through `0x000623B0` and stores 0 there.

`0x00042AF0` writes `property.sav` (`Unable to Write Property.sav`): 4 bytes each from
`0x000AB0C4`, `0x000AB0D4`, `0x000AB0D0`, `0x0009B718`, `0x0009B71C`, `0x0009C9D0`, `0x000AC180`,
`0x0009B8C0` and `0x0009B8C4`; 4 bytes from `0x0009C6C8 + 8 * i` for `i` from 0 to 69; 14 records
of 15 bytes from `0x0009B8EC`; and for each of the 176 18-byte records at `0x0009BA50`, 4 bytes from
`+0x06`, 4 from `+0x07` and 4 from `+0x0D`. When `0x0009A928` is 0 it loads the variables through
`0x00022564`, then writes them with `0x000623EC(0x0009A928, "vtsave")` and returns 1.
`0x000623EC(table, name)` writes `name.vtb` with mode `w+b`: the three dwords of FMT-TALK-008 and
the values, deleting the file when a write fails. `0x00042C90` reads `property.sav` in the same
order, frees the variables, and loads `vtsave.vtb` through `0x00062610("vtsave")` into
`0x0009A928`, stopping the game with `Unable to open vt save` when that fails.

`0x000385B4` writes the 64 bytes the pointer at `0x0009AE00` points to, doing nothing while it is
null; `0x00038600` calls `0x00038598`, allocates 64 bytes there and reads them.

`0x0002B3E0` does nothing while the pointer at `0x0009ABEC` is null. It writes 4 bytes from
`0x0009ABE8`, the 12-byte block, the 56 bytes of the first fief record its list at `+0x08` points
to, then that fief's lists: 628 bytes at `+0x28`, 688 at `+0x2C`, 416 at `+0x34` and 296 at `+0x30`.
It then walks 19 rows of 32 bytes of the first list, 15 rows of 44 of the second, 10 rows of 40 of
the `+0x34` list and 9 rows of 32 of the `+0x30` list, and for each row writes the 12-byte nodes of
the chain whose head and length the row holds at `+0x14` and `+0x1C`, `+0x1C` and `+0x24`, `+0x10`
and `+0x18`, and `+0x08` and `+0x10`, following `+0x00` of each node. `0x0002B5DC` calls
`0x0002B374`, allocates the block, a pointer list of `4 * block[0]` bytes, one 56-byte fief and the
four lists, reads them, rebuilds each chain by inserting every node it reads at the head, and
stores fixed string addresses (`0x00093C78` to `0x00093D2C`) in the `+0x18` field of the first
list's rows.

`0x00029C5C` writes 640 bytes from each of the five records the pointers at `0x000A9CD0` point to;
`0x00029CAC` allocates and reads them. `0x0005C3FC` writes, when the pointer at `0x0009DC30` is not
null, the 12-byte block it points to and 4 bytes each from `0x0009DC38`, `0x0009DC3C` and
`0x0009DC40`; `0x0005C47C` frees the block through `0x0005C3E0`, allocates and reads it
(FND-TOURNEY-001).

No component writes `0x0009E044`, `0x0009DC34` or `0x0009DED4`.

## Interpretation

The saved state is: the strategic map (the player's six forces, the three brigand forces with their
orders and the five hostile forces with their routes, the king's order, the pacing values and the
home position), the 14 properties and the changeable bytes of the 176 persons, the items the player
holds, the conversation variables, the character table (`~~1.SAV`, FND-PERSON-003), the calendar,
the home fief with its lists, the five armies and the tournament. The random number generator's
state is not saved, so a loaded game continues with whatever state the session had. The home fief
is the only fief saved; each load reverses the order of every chain in its lists. The tournament's
previous site and the flag for a tournament at the place being visited are not saved either.

## Alternatives

What the 4,028 bytes at `0x000AB170`, the dwords at `0x000A7540`, `0x000AB0C0`, `0x000AB0C8`,
`0x0009B718` and `0x0009B71C`, and the 64-byte calendar block hold beyond the fields other areas
name was not traced. Writing a person's `+0x06` and `+0x07` as two overlapping dwords restores bytes
6 to 10 and 13 to 16; whether the gap was meant is not known.

## How to reproduce

Disassemble the listed ranges and follow the `fwrite` and `fread` calls; the strings are at
object-2 offsets `0x0278` to `0x02F0` and `0x7168` to `0x71C4`.
