---
id: FND-SOUND-002
title: Sound banks load through 0x0005B584, only when SOUND_EFFECTS is on, from the open archive by index
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B554..0x0005B583
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B584..0x0005B716
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053EC5..0x00053EDF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0005B584(index, from_open)` returns 0 at once when the dword at `0x0009ADBC` (`SOUND_EFFECTS`)
is 0. With `from_open` equal to 1 it takes the directory record `0x000495C4(index)` of the open
archive; otherwise it closes the archive with `0x00049478(0)`, opens the path at `0x000A9D90` with
`0x00049200` and takes the record there. A missing record prints `Sound File not found in Resource
file. Aborting.` and exits with 1. It then allocates an 8-byte record, asks `0x0007AB20` for
`(size + 15) / 16` paragraphs of DOS memory for the entry's expanded size (`+0x2C` of the directory
record) and stores the result in the record's `+0x00`; when that fails it allocates ordinary memory
with `0x00063D20` instead and stores 0 in `+0x04`, otherwise 1. It reads the entry with
`0x000495F4` (`Unable to read MouseSoundOpen sounds` and exit 1 on failure), reopens the path at
`0x000A9CF0` when it had switched archives, and returns the record.

`0x0005B554(record)` frees the data with `0x0007B3D0` when `+0x04` is 1 and with `0x00063D78`
otherwise, then frees the record. It has 31 callers.

A byte scan of object 1 finds 33 calls to `0x0005B584`. Every one passes `from_open` 1. 32 pass a
constant index: `0x15F` (`iconmap`) once, `0x161` (`fopts`) once, `0x162` (`topts`) twice,
`0x163`, `0x164`, `0x165`, `0x166`, `0x167`, `0x169`, `0x16B`, `0x16C`, `0x16D` once each, `0x168`
(`chargen`) twice, `0x16A` (`fiefmgmt`) six times, `0x16E` (`tents`) twice, `0x170`
(`utility`) once, `0x1D8` (`joust`) three times, `0x1D9`, `0x1DA`, `0x1DB`, `0x1DC` once each and
`0x1E5` (`intro`) once. The call at `0x00053ED0` loads index 0 when the dword at `0x0009D54C` is 0
and keeps the result there. No call names `0x160` (`monylndr`), `0x16F` (the GOB `skirmsnd`) or
`0x171` (`configit`).

## Interpretation

Every bank is read from whichever archive is open: `C1086.GOB` for the 22 constant indices, and
`SKIRMISH.RES` for index 0 at `0x00053ED0`, whose sample offsets fit `SKirmsnd.666`
(FND-SOUND-003). The switch to `C1086ad.GOB` is never taken. With `SOUND_EFFECTS` off no bank is
loaded and the callers keep a null record.

## Alternatives

`monylndr.666`, the GOB `skirmsnd.666` and `configit.666` could still be loaded through a computed
index; no such call was found. That the open archive is `SKIRMISH.RES` at `0x00053ED0` is inferred
from the offsets.

## How to reproduce

Disassemble `0x0005B554..0x0005B716` in `CD:CONQUER.EXE`; scan object 1 for `E8` calls whose target
is `0x0005B584` and read the two pushes before each.
