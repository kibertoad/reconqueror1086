---
id: FND-RES-020
title: Disc-root icon candidates share counted directories but differ in a payload size value
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:AUTOPLAY.ICO
    offset: 0x00000000..0x00000436
  - build: BLD-GOG-EN
    file: CD:CONQCFG.ICO
    offset: 0x00000000..0x000002FE
  - build: BLD-GOG-EN
    file: CD:CONQUER.ICO
    offset: 0x00000000..0x000002FE
tool: Python bounded ISO root traversal and complete-file header inspection
environment: null
---

## Observation

Each complete file was read from the owned carrier using FMT-RES-005's physical
record payload mapping. Length and canonical XXH3-128 match BLD-GOG-EN:

| File | Length | XXH3-128 |
|---|---|---|
| CD:AUTOPLAY.ICO | 1078 | 211573abe5fae0d4fa88a8d805f4e9ba |
| CD:CONQCFG.ICO | 766 | 293e66def58eac7dea9321ee4bfd53e5 |
| CD:CONQUER.ICO | 766 | c609490572ed38680343c3a3a4a518d9 |

The first three little-endian words are 0, 1 and a count. AUTOPLAY has count 2;
the others have count 1. Beginning at offset 6, each counted 16-byte row
partitions as four bytes, two words and two doublewords. In row order:

| File / row | Four bytes | Two words | Length candidate | Offset candidate |
|---|---|---|---|---|
| AUTOPLAY / 0 | 32, 32, 16, 0 | 0, 0 | 744 | 38 |
| AUTOPLAY / 1 | 16, 16, 16, 0 | 0, 0 | 296 | 782 |
| CONQCFG / 0 | 32, 32, 16, 0 | 0, 0 | 744 | 22 |
| CONQUER / 0 | 32, 32, 16, 0 | 0, 0 | 744 | 22 |

Using the final pair as length and file offset gives adjacent, nonoverlapping
payloads beginning immediately after the complete directory and ending exactly
at file end. No unaccounted prefix, gap or trailer remains under this partition.

Each payload begins with a 40-byte region. Its little-endian fields at offsets
0, 4, 8, 12, 14, 16 and 20 have widths 4, 4, 4, 2, 2, 4 and 4:

| Payload | Values at those offsets |
|---|---|
| AUTOPLAY / 0 | 40, 32, 64, 1, 4, 0, 640 |
| AUTOPLAY / 1 | 40, 16, 32, 1, 4, 0, 128 |
| CONQCFG / 0 | 40, 32, 64, 1, 4, 0, 512 |
| CONQUER / 0 | 40, 32, 64, 1, 4, 0, 512 |

The four remaining doublewords through offset 39 are zero in every payload.
A possible partition after that region is 64 bytes, then 512 and 128 bytes for
the larger payloads, or 64 bytes, then 128 and 64 bytes for the smaller one.
These sums account for the stored payload lengths. Their interpretation as
color entries, a four-bit image and a one-bit mask is a hypothesis, not a
reader observation. Neither pixel bytes nor color values are reproduced here.

The value at payload offset 20 is not uniformly the same inferred span:
AUTOPLAY / 0 gives 512 + 128, whereas the other larger payloads give 512.
AUTOPLAY / 1 gives 128 rather than 128 + 64. No consumer was read or run.

Additional inspection (2026-10-05), using SRC-ICO-REFERENCE and
SRC-BMP-REFERENCE: partitioning each payload into the header, 64-byte palette
and the two aligned planes consumes it exactly. Every palette fourth byte is
zero. Unpacking each color byte high nibble first and repacking all indices,
and unpacking each mask byte most-significant-bit first and repacking every bit,
reconstructs the complete planes. Repacking headers and joining every directory
and payload reconstructs each entire file exactly and preserves its canonical
fingerprint. No bytes are omitted. No consumer or rendered pixel was observed.
This supplies the complete storage witness; reference semantics do not establish
a shipped reader's behavior.

## Interpretation

The counted-directory interpretation fits every listed file and its complete
extent. The signatures and header shapes are compatible with ICO files carrying
bitmap payloads. Header size, doubled dimension and proposed plane sizes provide
leads, but do not settle field semantics, row order, transparency, image selection
or the meaning of the differing size value.

## Alternatives

- Unrelated arbitrary binary layouts remain possible until readers establish
  field roles, although the shared counted partition favors a common container.
- A universal offset-20 value equal to the complete image-plus-mask length is
  contradicted by the stored values and the proposed partition.
- A universal value equal to the four-bit plane length is contradicted by
  AUTOPLAY / 0 under that same partition. A reader may ignore, reinterpret or
  accept variants of the value; the files alone do not decide which.

## How to reproduce

Verify the owned build. Read the bounded ISO root directory, locate all three
manifest paths, read their complete extents and compare canonical fingerprints.
Read the counted 16-byte rows, check each candidate extent before accessing it,
and verify adjacency and exact file-end coverage. Inspect each complete 40-byte
payload header at the stated widths. Compare the size candidate separately from
the proposed plane sums. Do not export pixels, palettes or renderings, and do not
execute a reader. The temporary inspection had a 16 MiB logical-read bound; no
bound was reached for these files.
