---
id: FND-RES-013
title: The three CD-root .386 candidates share counted variable-length record framing
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:HMIDET.386
    offset: 0x00000000..0x00014024
  - build: BLD-GOG-EN
    file: CD:HMIDRV.386
    offset: 0x00000000..0x0003FCB9
  - build: BLD-GOG-EN
    file: CD:HMIMDRV.386
    offset: 0x00000000..0x0001B538
tool: Python 3.14 struct-based complete record traversal of bounded ISO extents
environment: null
---

## Observation

The three files were read from their root-directory extents in the owned image,
using physical raw-sector indexing and the 2048-byte payload region. Their sizes
agree with BLD-GOG-EN. Complete source fingerprints are:

| File | Bytes | SHA-256 |
|---|---:|---|
| CD:HMIDET.386 | 81956 | 330d0f993423b9e8eea5325a907f9c6becaa24256981d976e22fef6df7cf93bf |
| CD:HMIDRV.386 | 261305 | ce1d5a73195340366bda040b60e71c0f859abacb73cab012278fccb097c6d03d |
| CD:HMIMDRV.386 | 111928 | dd2730e2b9f1fd3f2b1931707065e2296e367822c3abf29448c3c4da32e9cfad |

Every file begins with a 44-byte prefix. The little-endian word at offset 0 is
51; bytes 4 through 31 are zero; the word at 36 is 44. The word at 32 gives
76, 96 and 8 respectively. The word at 40 is respectively 0x00004024,
0xFFFFFCB9 and 0xFFFFB538; no runtime interpretation is established.

Starting at offset 44, each record has a 32-byte stored name region, four
little-endian 32-bit words at record offsets 32, 36, 40 and 44, then a payload
at record offset 48. Taking the word at 36 as the payload byte count and
advancing by 48 plus that count reaches the exact file end after the prefix's
count, in every file. The word at record offset 32 equals the payload count
plus 92 in every record. It is not the absolute next-record offset: interpreting
it that way repeats the second record of HMIDET.386 rather than advancing.

All names have a zero terminator within the stored 32 bytes; all bytes preceding
that terminator are printable ASCII. Bytes after the first terminator are not
always zero: this occurs in 13, 23 and 2 records respectively. Treating those
bytes as required zero padding would reject the owned files.

| File | Records inspected | Minimum payload size | Maximum payload size | Last record start | Last payload size | Values at record offset 44 |
|---|---:|---:|---:|---:|---:|---|
| CD:HMIDET.386 | 76 | 393 | 3320 | 79947 | 1961 | 16384, 32768, 49152 |
| CD:HMIDRV.386 | 96 | 371 | 9762 | 259577 | 1680 | 16384, 32768, 49152 |
| CD:HMIMDRV.386 | 8 | 503 | 47988 | 111108 | 772 | 0 |

Each record header and complete payload was bounded by the file length. There
was no sampling or record cap. No trailing bytes remained. This traversal did
not decode the payloads or read a loader; it establishes stored framing only.

## Interpretation

A single counted container layout describes these three complete files. Their
suffix grouping can therefore be replaced by this narrow framing description.
The name region must retain bytes after its terminator. The roles of payloads,
unknown words, selection keys and the shipped loader's acceptance rules remain
unread. File framing does not establish executable mapping or code ranges.

## Alternatives

- An absolute next-record offset at record offset 32 is ruled out by the second
  record and the complete size-based traversal.
- Required zero padding after each name is ruled out by complete inspection of
  all name regions.
- Unrelated outer layouts are ruled out for these files by the shared bounded
  traversal ending exactly at each file boundary. Different payload layouts
  remain possible and are not resolved by this finding.

## How to reproduce

Verify BLD-GOG-EN and the fingerprints above. Read the three ISO root files
using FMT-RES-005's physical-sector mapping. Inspect the 44-byte prefix, then
repeat the record count at prefix offset 32: bound a 48-byte record header,
retain its 32-byte name region and four little-endian words, bound and consume
the payload count at record offset 36, and advance by 48 plus that count.
Check the stored word at offset 32 against payload count plus 92, inspect every
name terminator and its remaining bytes, and require the final cursor to equal
the file size. Do not infer loader behavior from this file-data check.
