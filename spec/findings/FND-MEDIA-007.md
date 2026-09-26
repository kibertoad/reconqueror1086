---
id: FND-MEDIA-007
title: SKIRMISH.RES keeps its screens as raw 320x200 pictures that the game builds from PCX files when they are missing
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004C414..0x0004C6DD
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004E253
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000521D2
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00053E45
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00054A09
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00054AF6
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00056859
  - build: BLD-GOG-EN
    file: CD:CONQUER/SKIRMISH.RES
    offset: 0x00..0xAFBEF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0004C414(name, a, b, buffer, palette)` frees `*buffer`, joins the path at `0x000A9D40` and
`SKIRMISH.RES` through `0x000641A0`, and opens that archive with `0x00049200(path, "rb")`,
returning when that fails. It looks `name` up, allocates the entry's expanded size plus 2 into
`*buffer` and reads it. When that works and `palette` is not 0, it cuts `name` at the first `.`,
appends `.PAL`, and reads that entry into `palette`; a missing one formats `Can't find %s in RES
file.!` and stops the game. When the read fails it closes the archive, frees `*buffer` and calls
`0x00040CE4(name, a, b, buffer, palette)` to decode the PCX file `name` from disk. If that gives
nothing it formats `Can't read file %s! (PCX)` and stops the game. Otherwise it reopens the archive
with mode `r+b` and adds the decoded pixels as kind 1 under `name`, and, with a palette, the 768
palette bytes as kind 1 under the `.PAL` name, through `0x000497BC`. It closes the archive at the
end.

Its callers pass `EXIT.PCX` (`0x0004E253`), `STAIRS.PCX` (`0x000521D2`), `BRAWL.PCX`
(`0x00053E45`), `SKIRMCUR.PCX` (`0x00054A09`), `SKIRMISH.PCX` (`0x00054AF6`) and
`HELP.PCX` (`0x00056859`).

`SKIRMISH.RES` holds 18 entries. The eight `.PCX` entries `MELEE2`, `SK_START`, `BRAWL`,
`SKIRMCUR`, `SKIRMISH`, `HELP`, `EXIT` and `STAIRS` are kind 1 and decode to exactly 64,000 bytes
with no PCX header. The five `.PAL` entries (`MELEE2`, `SK_START`, `BRAWL`, `SKIRMISH`, `HELP`) are
kind 0 and exactly 768 bytes, with largest component values of 252 to 255. `FNT6.PCX` decodes to
9,216 bytes, 8,248 of them 0 and 808 of them 255. The other entries are `SKirmsnd.666`,
`skirmish.csf` (FND-MEDIA-002), `skirmish.svg` (125,702 bytes, the same size as `skirmish.csf`, with
a different start) and `skirmish.sfg` (150 bytes).

Drawn as 320 by 200 with `SKIRMISH.PAL`, `SKIRMISH.PCX` is a stone frame with a black view at x 26
to 192 and y 24 to 140, a message panel below it, four buttons labelled Attack, Defend, Follow and
Retreat, a Health bar along the bottom, two panels at the upper right and a black box at the lower
right.

## Interpretation

The skirmish screens were drawn as PCX files and cached in `SKIRMISH.RES` as plain row-major
pixels, 320 per row, with their palette split off (FMT-MEDIA-004, FMT-MEDIA-005). The shipped
archive already holds every cached screen, so the PCX path runs only for a missing entry.

## Alternatives

`a` and `b` go only to `0x00040CE4`; they may be the expected size of the picture. `FNT6.PCX` is
9,216 bytes, which fits a 96x96 or 288x32 font sheet; which was not established.

## How to reproduce

Disassemble `0x0004C414..0x0004C6DD` and its callers in `CD:CONQUER.EXE`; decode the entries of
`CD:CONQUER/SKIRMISH.RES` and draw `SKIRMISH.PCX` with `SKIRMISH.PAL`.
