---
id: FMT-RES-006
title: Installed disc-image cue sheet
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["game.ins"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-RES-011]
conflicting: []
split_with: []
related: []
---

## Layout

The owned cue sheet contains ASCII FILE, TRACK and INDEX records, with CRLF
line endings and a final CRLF. It has no byte-order mark, comment lines or
section delimiters. Command and mode tokens in this file are uppercase. Spaces
separate tokens; the image name is double-quoted. Each TRACK has one following
INDEX. Track and index numbers have two digits, and the timestamp has three
colon-separated two-digit components. TRACK lines have two leading spaces;
INDEX lines have four [FND-RES-011]. These describe the shipped file, not the
full language accepted by its wrapper reader. Alternative casing, whitespace,
comments, missing records and unknown records have not been established.

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| FILE image token | `char[]` | `cue_file_name` | Double-quoted raw image filename; the shipped value names game.gog. | supported | FND-RES-011 |
| FILE storage token | `char[]` | `cue_file_storage` | The shipped token is BINARY. | supported | FND-RES-011 |
| TRACK number token | `char[2]` | `cue_track_number` | Two ASCII decimal digits; the shipped sequence is 01 through 06. | supported | FND-RES-011 |
| TRACK mode token | `char[]` | `cue_track_mode` | MODE1/2352 on track 1 and AUDIO on tracks 2 through 6. | supported | FND-RES-011 |
| INDEX number token | `char[2]` | `cue_index_number` | Two ASCII decimal digits; every shipped record is 01. | supported | FND-RES-011 |
| INDEX timestamp token | `char[8]` | `cue_index_time` | Three two-digit components separated by colons; the minute/second/75-subdivision interpretation agrees with the shipped spans. | supported | FND-RES-011 |

## Enumerations and flags

None. The observed storage and mode tokens are given above; no additional
accepted tokens or mode values are established.

## Differences between builds

None known.

## Coverage

The installed cue sheet identified by BLD-GOG-EN was inspected in full. Its
track-span arithmetic agrees with the identified raw image and track sizes
[FND-RES-011]. This is file-data evidence only; wrapper parsing has not been
completely read or exercised. Native executable access to this file is not
asserted by this entry.

## Open questions

- Does the shipped wrapper convert INDEX timestamps with 75 subdivisions per
  second? The arithmetic fits the file, but a reading of its consumer is still
  needed. (Q-RES-118)
- Does the shipped wrapper reject an INDEX timestamp whose seconds component
  is 60 or greater? The file contains no such case. (Q-RES-119)
- Other cue grammar and malformed-record behavior are outside this entry's
  observed-file coverage. (No item: no claim about that wider wrapper language
  is made here.)
