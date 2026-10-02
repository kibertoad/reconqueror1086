---
id: FND-RES-019
title: INSTALL.HLP preserves marked text regions, whitespace and a control byte followed by CRLF
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:INSTALL.HLP
    offset: 0x00000000..0x000054F3
tool: Python 3.14 complete byte classification and bounded line/marker traversal
environment: null
---

## Observation

The file was selected from the owned ISO root directory and read completely
through FMT-RES-005's physical-sector payload mapping. Its 21747-byte length
and canonical XXH3-128 `7dd660f1c45378e01d06ae7357b95375` agree with BLD-GOG-EN.

Every byte is printable ASCII, tab, CR, LF or a single 0x1A byte. There are
752 CRLF pairs, no bare CR or bare LF, 40 tab bytes and no NUL, byte-order mark
or non-ASCII byte. Splitting only at CRLF yields 753 regions, including an empty
region after the final delimiter. The longest region is 58 stored bytes.

Complete region classification gives 157 regions starting with two backslashes,
471 other nonempty text regions, 124 empty regions and one region containing
only 0x1A. No region begins with exactly one backslash. The backslash-prefixed
regions have 147 distinct complete remainders, so some repeat. These numbers
come from retaining the original bytes, without trimming or case conversion.

Among marker remainders, 18 have no dot, 69 have a final drv suffix, 60 have a
final hlp suffix, nine have a final inf suffix, and one has an hlp suffix followed
by a space. All of these suffix spellings are lowercase. This records stored
shapes only: it does not establish sections, file lookup, links or dispatch.
The last case shows why trimming a marker changes the stored token.

There are 152 nonempty regions ending in a space. Whitespace is retained
whether it occurs on marked or other lines. The lone 0x1A occurs at file offset
0x54F0, followed by CRLF through file end. It is the sole byte of its own region,
not the final byte of the file. No interpretation of it as end-of-input is made.

A complete traversal retaining every region and delimiter reconstructs the file
byte-for-byte. The help prose and complete marker contents are not reproduced
here. No reader or original program was read or run.

## Interpretation

The provisional binary grouping is replaced by observed text framing with
backslash-prefixed region shapes. Tabs, trailing spaces, repeated markers and
the CRLF after 0x1A must remain separate from consumer semantics. A reader that
trims markers or drops stored bytes after the control byte would make a choice
not settled by file inspection.

## Alternatives

- Binary record framing has no supporting evidence; complete inspection instead
  partitions the file into the described text regions.
- Unique marker remainders are ruled out by complete duplicate counting.
- A control byte occupying the physical final byte is ruled out by the two
  following delimiter bytes.
- Exact marker matching and normalized whitespace matching both fit the file;
  the reader must settle which it uses.
- Backslash prefixes may introduce links, section labels or file references;
  their spelling alone does not establish any of those roles.

## How to reproduce

Verify BLD-GOG-EN and the canonical fingerprint. Bound and read the complete
ISO file. Classify every byte, retain CRLF delimiters and count regions by their
literal prefixes without trimming. Count complete marker remainders and suffix
shapes separately, including whitespace. Locate the lone 0x1A and inspect all
following bytes. Reconstruct the entire file from retained regions and delimiters
and compare exactly. Export no help prose and run no reader.
