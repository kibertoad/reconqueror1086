---
id: FND-RES-012
title: The raw carrier has a cue-delimited data span with duplicate and zero padding records
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: game.gog
    offset: 0x00000000..0x27C67110
tool: Complete leading-record scan and bounded ISO directory traversal
environment: null
---

## Observation

The image is 667,316,496 bytes, exactly 283,723 records of 2,352 bytes.
FMT-RES-006 / FND-RES-011 place the first audio track at record 244445, raw
file offset `0x2244CE70`. The records before it and the following audio span
are distinguished by that cue boundary, not by an image-wide header.

Every one of the 244,445 leading records was inspected without a cap or
sampling. Records 0 through 244303 have a twelve-byte prefix consisting of a
zero byte, ten FF bytes, and a zero byte. Their byte 15 is 1. Bytes 12 through
14 contain valid packed-decimal minute, second and subdivision components;
seconds are below 60 and subdivisions below 75. A 2,048-byte region starts at
byte 16, followed by 288 stored bytes at byte 2064. Bytes 2068 through 2075
are zero in every inspected leading record. The functions of the remaining
trailer bytes were not determined.

For records 0 through 244287, converting the three stored address components
with 75 subdivisions per second gives physical record index plus 150.
Records 244288 through 244303 have stored addresses sixteen records earlier
than that expression predicts. Their complete bytes duplicate records 244272
through 244287. Both sixteen-record blocks have SHA-256
`958e1870cd3df92ac26d9f061484568a31a1e25272e53d68a1abfbf0e39cb0fa`.
The source block occupies `0x223E9900..0x223F2C00` and its duplicate occupies
`0x223F2C00..0x223FBF00`.

Records 244304 through 244444 are entirely zero, including their prefix,
mode byte and trailer. This run occupies `0x223FBF00..0x2244CE70`. No other
leading record has a non-matching prefix or a mode byte other than 1; the
complete scan of every record is the absence check. Record 16's primary volume
descriptor is the positive control for a recognized payload.

That descriptor starts at raw offset `0x00009310`, has identifier CD001 and
version 1, and declares 244,070 logical blocks of 2,048 bytes. Its duplicated
little/big-endian volume count and block-size values agree. The physical extent
of those logical blocks ends at `0x22375920`. Every record from 244070 through
244303 has an all-zero 2,048-byte payload region, although its prefix/trailer
remain nonzero. Thus the duplicate and wholly-zero runs are beyond the declared
volume, as are the other extra leading records after its end.

A recursive directory traversal checked every file and directory extent
against the declared logical-volume size, with no sampling or item cap. All
2,830 file extents and 30 directory extents fit. It excluded the current/parent
directory records, rejected cycles, and bounded directory depth to 32 and each
directory to 16 MiB; none of those bounds was reached. The positive control was
CONQUER.EXE with its manifest size of 919,107 bytes. Logical reads selected the
2,048-byte region by physical record index, not by the stored address bytes.

## Interpretation

The cue-delimited leading span is larger than the ISO volume's declared span.
It includes a nonzero framed padding area, a duplicate block and entirely-zero
records. A requirement that every leading record have a Mode 1 prefix, or that
every stored address equal physical index plus 150, rejects this owned carrier.
Stored addresses and physical position must be kept distinct. This records
file structure, not which validations a wrapper performs.

## Alternatives

Treating the carrier as consecutive 2,048-byte records does not retain the
observed prefixes, payload alignment and cue/audio boundary. Treating all
244,445 leading records as ordinary Mode 1 sectors conflicts with the complete
zero records. Treating the stored address as a unique physical locator conflicts
with the duplicate block. Padding or mastering artifacts are plausible reasons
for the extra records, but their production history is not established.
The cue labels the trailing span AUDIO; this reading does not establish its
sample interpretation or the behavior of a reader presented with corrupt data.

## How to reproduce

Use the carrier identified by BLD-GOG-EN and the cue boundaries of FND-RES-011.
Scan every leading 2,352-byte record. Check prefix, address components and mode
separately; retain complete-zero records rather than discarding them. Compare
each decoded address with physical index plus 150 and compare the two cited
sixteen-record blocks byte-for-byte. Check the post-volume payload regions
independently of their surrounding headers and trailers.

Read the primary volume descriptor from record 16's byte-16 payload, compare
its paired endian fields, and recursively traverse all directories. Bound each
logical extent by the declared volume size; keep the CONQUER.EXE positive
control. No original executable or wrapper needs to run.
