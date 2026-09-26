---
id: FND-SAVE-003
title: A load checks VERSION 2.1, unpacks the entries and runs the component loaders; temp.jap must be present
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004ADD4..0x0004B196
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00049BA8..0x00049C78
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0004ADD4(name)` deletes the same eight files as the save, builds `SAVEGAME\` + `name` and opens
it with `0x00049200(path, "r+b")`; on failure it returns 0. When `0x0004957C("VERSION")` finds no
entry, or the entry's first bytes are not `2.1`, it closes the archive, reopens `C1086.GOB` (a
failure stops the game) and shows `This save game will not work with the current revision.` through
`0x00025004`, and returns 0. When reading the entry fails it returns 0 with the archive still open.

It then writes entries back to loose files through `0x00049BA8`: `TROOPS.SAV`, `vtsave.vtb`,
`PROPERTY.SAV` and `~~1.SAV` to `~~5.SAV`, stopping the game with `Error in Restoring game data.
Aborting.` (code 0) when one returns 0; then `ictempmt.jp`, `ictempmm.jp`, `ictempmf.jp` and
`ictempmc.jp` without a check; then `temp.jap`, stopping the game when it returns 0. It closes the
archive, calls `0x00010CB4()` and `0x00042C90()`, stopping the game when either returns 0, then
`0x00015D54("~~1.SAV")`, `0x00038600("~~2.SAV")`, `0x0002B5DC("~~3.SAV")`, `0x00029CAC("~~4.SAV")`
and `0x0005C47C("~~5.SAV")`, deletes the eight files, calls `0x00024CA0` and returns 1.

`0x00049BA8(path)` finds the entry named by the file name and extension of `path`, allocates its
expanded size, reads it with `0x000495F4`, and when that returns a length writes that many bytes to
`path` with mode `wb`. It returns the length, or 0 when the entry is missing or the read fails.

## Interpretation

The version check rejects saves from other revisions with a message and leaves the player on the
load screen. Since the save adds `temp.jap` only when the file exists, a game saved before anything
wrote `temp.jap` in that session cannot be loaded: the load stops the program (BUG-SAVE-001).

## Alternatives

When `temp.jap` is written was only partly traced: `0x0003E6F1` writes it through `0x0006E980` and
`0x0003E952` reads it through `0x0006E320`, in the strategic map code, and startup deletes it. The
load may never meet a save without it in normal play.

## How to reproduce

Disassemble the listed ranges and read the strings at object-2 offsets `0x769C` to `0x7904`.
