---
id: FND-SAVE-002
title: A save writes ten temporary files and packs them with a title and version into one resource container
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004A9C0..0x0004ADD2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049AB8..0x00049BA5
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0004A9C0(name, title)` first deletes `TROOPS.SAV`, `PROPERTY.SAV`, `~~1.SAV`, `~~2.SAV`,
`~~3.SAV`, `~~4.SAV`, `vtsave.vtb` and `~~5.SAV` through `0x000681D1`. It then calls, in order,
`0x000109AC()` and `0x00042AF0()`, stopping the game with `Error in saving game data. Aborting.`
(code 0) when either returns 0, then `0x00015B80("~~1.SAV")`, `0x000385B4("~~2.SAV")`,
`0x0002B3E0("~~3.SAV")`, `0x00029C5C("~~4.SAV")` and `0x0005C3FC("~~5.SAV")` (FND-SAVE-004).

It builds `SAVEGAME\` + `name` and opens it as a new archive with `0x00049200(path, "w+b")`,
stopping the game on failure, and adds entries with kind 2 and field 24 set to 0 (FND-RES-005):
`TITLE`, 40 bytes from `title`, and `VERSION`, the 4 bytes `2.1` and NUL, through `0x000497BC`;
then the files `PROPERTY.SAV`, `vtsave.vtb`, `TROOPS.SAV`, `~~1.SAV`, `~~2.SAV`, `~~3.SAV`,
`~~4.SAV` and `~~5.SAV` through `0x00049AB8`, stopping the game when one returns 0; then
`ictempmc.jp`, `ictempmf.jp`, `ictempmm.jp`, `ictempmt.jp` and `temp.jap`, each only when
`0x0006A190` finds it, stopping the game when adding it fails. It closes the archive with
`0x00049478(0)` and deletes the eight files it deleted at the start; the four `.jp` files and
`temp.jap` stay.

`0x00049AB8(path, kind, field24)` splits `path` with `0x0006A447` and names the entry after its
file name and extension, takes the size from `0x00073413`, allocates size + 256 bytes, reads the
whole file in one `0x00063ABA` call and adds it through `0x000497BC`; it returns 0 when the
allocation or read fails or the entry exists.

## Interpretation

A saved game is a `.RES` container like `C1086.GOB` (FMT-RES-001), with every entry compressed with
the kind-2 coder. The component writers each write a loose file in the current directory; the save
routine gathers them and deletes them again. The version string `2.1` is the save format's
revision.

## Alternatives

What writes the four `ictemp*.jp` files was not found; no other code names them apart from the
startup cleanup (FND-SAVE-005).

## How to reproduce

Disassemble the listed ranges and read the strings at object-2 offsets `0xCABC` to `0xCB54` and
`0x7434` to `0x7464`.
