---
id: FND-MEDIA-008
title: The CD holds 2,131 Smacker movies, all SMK2, most 196x204 at 100 ms with one 22,050 Hz track
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER/TITLE.SMK
    offset: 0x00..0x13B7FF
  - build: BLD-GOG-EN
    file: CD:CONQUER/CREDITZZ.SMK
    offset: 0x00..0x197457
  - build: BLD-GOG-EN
    file: CD:CONQUER/WH_640.SMK
    offset: 0x00..0x16DE03
  - build: BLD-GOG-EN
    file: CD:CONQUER/1101.SMK
    offset: 0x00..0xF867
  - build: BLD-GOG-EN
    file: CD:CONQUER/TRANDRAG.SMK
    offset: 0x00..0x18313
  - build: BLD-GOG-EN
    file: CD:CONQUER/JOUSPRAC.SMK
    offset: 0x00..0xC16F3
tool: Smacker census written for this project
environment: null
---

## Observation

The disc image holds 2,131 files named `.SMK` under `CONQUER/`, 288,867,980 bytes and 182,360
frames in all. Every one begins `SMK2`. In each, the header, frame size table, frame flag table,
tree data and frames end exactly at the end of the file.

Sizes: 196x204 (2,068 files), 200x424 (26), 640x300 (13), 640x480 (9), 192x268 (6), 320x180 (4),
640x360 (2), 320x184 (2) and 320x200 (1). Frame durations: 100 ms in 2,069 files, 71 ms in 45, and
other values in 17. 2,129 files change the palette once, one twice and one four times, 2,135 palette
changes in all. 2,093 files have one packed 8-bit mono audio track at 22,050 Hz, two at 11,025 Hz,
and 36 have none. The 161,884 audio packets decode to 398,368,812 bytes of unsigned 8-bit samples,
and all 182,360 frames decode within their packets. The longest file has 1,128 frames.

`TITLE.SMK`, `CREDITZZ.SMK` and `WH_640.SMK` are 640x480; `JOUSPRAC.SMK` is 640x300;
`TRANDRAG.SMK` is 192x268 and silent. The 196x204 files are named by number, `1101.SMK` onwards,
and are the conversation movies of FND-TALK-003.

## Interpretation

The movies are standard Smacker 2 files (FMT-MEDIA-006); the release has no Smacker 4 file.

## Alternatives

None known.

## How to reproduce

Parse every `CONQUER/*.SMK` of the disc image with a Smacker 2 parser and decode its palette,
audio and video packets.
