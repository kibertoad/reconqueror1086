---
id: FND-RES-018
title: The disc and installed C1086.GOB copies are identical and share the documented resource directory
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00000000..0x021B93B2
  - build: BLD-GOG-EN
    file: CD:C1086.GOB
    offset: 0x00000000..0x021B93B2
tool: Python 3.14 complete byte comparison, xxHash3-128 and bounded directory traversal
environment: null
---

## Observation

The disc-root file was selected from the owned ISO root directory and read
through FMT-RES-005's physical-sector payload mapping. The installed copy was
read independently from its manifest path. Each is 35361714 bytes and their
complete bytes compare equal, without sampling. Both have canonical XXH3-128
`741826412e307510f394d5e774519320`, matching BLD-GOG-EN.

Both begin with the .RES marker and a little-endian directory offset of
35336438. The directory's count is 486. A complete traversal of every 52-byte
record gives a final directory end equal to the file length. Every record's
32-byte name region has a zero terminator and zero remainder. Its unused word
at record offset 36 is zero. Observed storage kinds are 0, 1 and 2.

Every stored entry extent is bounded by the directory start, starts where the
preceding extent ends, and the first starts at offset 8. The last ends exactly
at the directory start. No directory or extent record was skipped or capped.
This independently checks the existing FMT-RES-001 / FMT-RES-002 stored layout
against the additional disc manifest path, without decoding any entry content.

## Interpretation

The disc path is another copy of the installed container already described by
FMT-RES-001. The provisional FMT-RES-012 inventory entry is redundant and can be
superseded by that entry. Adding the disc path does not introduce a new content
variant or promote the existing format beyond supported. Full byte identity
and directory traversal establish file structure, not which copy a consumer
opens at runtime or the codecs' behavior.

## Alternatives

- A distinct disc container layout is ruled out by complete byte equality and
  the matching bounded directory traversal.
- Equal length or a shared suffix alone would not establish identity; both
  complete comparison and the canonical hashes were checked.
- Runtime selection of the disc versus installed copy is not settled by these
  identical bytes and remains a separate question.

## How to reproduce

Verify BLD-GOG-EN. Locate CD:C1086.GOB in the ISO root directory, bound its
extent by the logical volume and read exactly its declared length. Read the
installed C1086.GOB independently. Compare every byte and compute canonical
XXH3-128 for both. Decode the .RES header, bound the directory count and all
52-byte records, then inspect every name region, unused word and stored extent
under FMT-RES-001 / FMT-RES-002. Require contiguous data from offset 8 through
the directory start and a directory ending exactly at the file end. Decode no
entry payload, export no original content and run no original executable.
