---
id: FND-RES-011
title: The installed cue sheet names one raw image and six track starts
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: game.ins
    offset: 0x00000000..0x00000113
tool: ASCII token inspection and independent track-span arithmetic
environment: null
---

## Observation

The installed cue sheet is 275 bytes, with SHA-256
`9992bb6a41043f54c2cb4345eabdfe02b40a7d3ebe63d7858eb3e47817e2a90c`.
Every byte is ASCII. Its 13 lines end in CRLF, including the last line, and
there is no byte-order mark. The first record names `game.gog` in double quotes
and has a BINARY storage token. Six TRACK records each have one INDEX record.
Commands and mode tokens are uppercase; TRACK records have two leading spaces
and INDEX records have four. Track numbers and index numbers are two decimal
digits, and each index timestamp has three two-digit components separated by
colons. Every index number in this file is 01.

Interpreting the timestamp components as minutes, seconds and 75 subdivisions
per second yields these raw-sector starts and lengths. Lengths use the following
track's start, or the image's end for the final track.

| Track | Recorded mode | Start sector | Length in sectors |
|---|---|---|---|
| 1 | MODE1/2352 | 0 | 244445 |
| 2 | AUDIO | 244445 | 1942 |
| 3 | AUDIO | 246387 | 4212 |
| 4 | AUDIO | 250599 | 9797 |
| 5 | AUDIO | 260396 | 17067 |
| 6 | AUDIO | 277463 | 6260 |

The image's size is an exact multiple of 2,352 bytes, giving 283,723 sectors.
The first audio start matches the data-track extent in FND-RES-010. Each audio
length multiplied by 2,352 matches that track's manifest size. This is an
independent arithmetic consistency check, not an observation of wrapper code.

## Interpretation

The installed .INS file is text with cue records, not a binary structure inferred
from its extension. Its recorded timestamps are consistent with the identified
raw data/audio spans. This does not establish how a wrapper parses other cue
records, handles invalid timestamps or resolves image paths.

## Alternatives

A binary-layout interpretation is ruled out by the complete ASCII record
structure. Interpreting the last timestamp component as hundredths of a second
does not reproduce the recorded track spans. The 75-subdivision reading agrees
with all observed spans; a wrapper's actual conversion and validation remain
unread. The file's fixed casing and indentation do not demonstrate which other
spellings or whitespace a reader accepts.

## How to reproduce

Read the cue sheet identified by BLD-GOG-EN, verify its size and SHA-256, and
inspect its bytes as ASCII without normalizing line endings. Identify the FILE,
TRACK and INDEX tokens, retaining their widths, quoting and indentation. For
each timestamp, calculate `(minutes * 60 + seconds) * 75 + subdivisions`.
Subtract successive starts; for the last track subtract its start from the raw
image size divided by 2,352. Compare the resulting data extent and audio sizes
with FND-RES-010 and the build manifest. No original program needs to run.
