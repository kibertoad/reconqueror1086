---
id: FND-RES-015
title: AUTOPLAY.BMP contains an 8-bit indexed bitmap with exact palette and row boundaries
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.BMP
    offset: 0x00000000..0x0004B436
tool: Python 3.14 struct header decoding and complete bounded palette and row traversal
environment: null
---

## Observation

The file was selected from the owned ISO root directory and read in full through
FMT-RES-005's physical-sector payload mapping. Its length is 308278 bytes,
agreeing with BLD-GOG-EN. Complete SHA-256 is
`15997d6b098a718141be3c4dd76e3fb1bfd07a79dd15d855f4770ad05599caaa`.

The 14-byte file header starts with BM. Its little-endian size is 308278,
both reserved words are zero, and the pixel offset is 1078. The following
40-byte information header declares width 640, positive height 480, one plane,
eight bits per pixel, compression value zero, image byte count 307200, both
resolution fields zero, palette count 256 and important-color count zero.
SRC-BMP-REFERENCE supplies the documented field roles; the stored values and
extent arithmetic were independently checked in the owned file.

A 1024-byte region occupies offsets 54 through 1077: exactly 256 four-byte
palette entries. Every entry's fourth byte is zero. The pixel region occupies
1078 through 308277: exactly 480 rows of 640 bytes each. The documented stride
expression, width times bit depth rounded up to a multiple of 32 bits and divided
by eight, yields 640 here. There is no extra row padding for this width, gap
before the pixel region or trailing region after it.

Every palette entry and every complete row was traversed without sampling or a
cap. Concatenating the original header fields, retained palette entries and
stored rows reconstructs the whole file byte-for-byte. No palette colors, pixel
samples or art are retained here. No original executable or viewer ran.

## Interpretation

The complete file matches the documented uncompressed indexed bitmap framing.
This establishes the narrow layout of the owned file, not a general decoder or
the shipped consumer's checks. Positive height encodes bottom-up rows under
SRC-BMP-REFERENCE; that convention is not a claim about how a particular viewer
places the image on screen.

## Alternatives

- A different outer container is ruled out for this file by the matching file
  and information headers and exact palette/pixel extents.
- Compressed payload framing has no supporting declaration: compression is zero
  and the complete payload fits the documented uncompressed row size exactly.
- A header-only signature match is insufficient by itself; complete boundary
  traversal and reconstruction are the additional checks recorded here.
- Runtime use, malformed-input policy and visible placement remain unresolved;
  filenames and a recognized bitmap do not establish those behaviors.

## How to reproduce

Verify BLD-GOG-EN and the complete fingerprint. Read exactly the ISO-declared
file length. Decode the 14-byte header and subsequent 40-byte information header
in little-endian order, preserving signed width, height and resolution fields.
Check header size, palette count, pixel offset, stride and image length against
SRC-BMP-REFERENCE. Bound every palette entry and row by the file length and
require exact final consumption. Reconstruct the stored bytes from parsed
fields, palette and row regions and compare to the complete source. Do not
render or export the artwork, and do not run the consumer.
