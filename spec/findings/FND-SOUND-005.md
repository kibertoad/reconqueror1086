---
id: FND-SOUND-005
title: MIDI music plays one HMP song at a time through 0x0005B0D4; the release holds 12 songs
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B0D4..0x0005B2BF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B2C0..0x0005B2F3
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005B2F4..0x0005B36F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0009AE70..0x0009AE7F
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1AFDDF7..0x1AFFC8A
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1AFFC8B..0x1B03A10
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B03A11..0x1B03DDA
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B03DDB..0x1B04194
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B04195..0x1B0454E
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B0454F..0x1B04F61
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B04F62..0x1B08BE5
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B08BE6..0x1B0A2ED
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B0A2EE..0x1B0AF19
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1BCB224..0x1BCB9B3
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1BCB9B4..0x1BCC32A
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1BCC32B..0x1BCCC08
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B0AF1A..0x1B0BBD0
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1B0BBD1..0x1B0C31A
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0005B0D4(index, from_open)` returns unless `0x0009ADCC` (`MIDIMUSIC`) and `0x0009DBB4` (MIDI
started) are both set. It takes the directory record of `index` from the open archive, or with
`from_open` not 1 switches to the path at `0x000A9D90` first. A record whose expanded size is above
`0x8000` makes it exit with 1. It reads the entry into the 32 KB buffer at `0x000AFF20`, prepares it
with `0x0006EABC` into the song handle at `0x000AFF24`, starts it with `0x0006F2E1`, and reopens
`C1086.GOB` when it had switched. `0x0005B2C0` stops the song with `0x0006F369` and `0x0006F29E`;
`0x0005B2F4` pauses it through `0x0007ACB1(handle, 1)` and `0x0005B330` resumes it through
`0x0007AE08`, both keeping the state at `0x0009DBB8`.

A byte scan finds 28 calls to `0x0005B0D4`, all with `from_open` 1, and 38 to `0x0005B2C0`. The
constant indices are `0x177` (`church`), `0x178` (`morals`), `0x179` (`fiefmgmt`), `0x17B`
(`iconwrld`), `0x17C` (`ocast`), `0x18B` (`lady`), `0x18C` (`tavern`) and `0x18D` (`village`). Two
calls take the index from the four dwords at `0x0009AE70`, `0x176`, `0x17A`, `0x175`, `0x176`
(`castle3`, `icast`, `castle2`, `castle3`), and six replay the index kept at `0x0009AAC4` unless it
is -1, four of them playing `0x178` when it is. No call names `0x17D` (`party`).

The 12 `.hmp` entries of `C1086.GOB`, at indices `0x175` to `0x17D` and `0x18B` to `0x18D`, all
start with the 14 bytes `HMIMIDIP013195`, which the executable also holds, and are 1,948 to 28,705
bytes. `morals.hmp` and `fiefmgmt.hmp` are the same 2,173 bytes.

## Interpretation

The game keeps one song playing through the HMI MIDI driver; screens start their own song, and
`0x0009AAC4` lets a screen restore the song that was playing. Every shipped song fits the 32 KB
buffer.

## Alternatives

The HMP layout past the tag is the HMI library's; the game never reads it. The castle table may be
chosen at random; the index into it at the two calls was not traced.

## How to reproduce

Disassemble the listed ranges in `CD:CONQUER.EXE`; scan object 1 for calls to `0x0005B0D4`; read the
first 14 bytes of each `.hmp` entry.
