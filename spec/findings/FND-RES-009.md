---
id: FND-RES-009
title: The bound LE payload maps initialized code and data separately from its data zero-fill
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x00026654..0x00026694
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000290FC..0x00029180
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    kind: file-data
    offset: 0x000291C0..0x000291F0
tool: Independent LE header and object-table inspection
environment: null
---

## Observation

The embedded MZ module begins at file offset `0x00026654`; its new-header pointer
selects the LE header at file offset `0x000290FC`. The page-data offset is relative
to that MZ module and selects file offset `0x0004C254`, not an offset from the LE
header. There are 149 logical pages of 4,096 bytes, with 1,007 bytes in the final
stored page.

The two object-table records occupy file offsets `0x000291C0..0x000291F0`.

Object 1 has base address `0x00010000`, virtual size `0x0007CB9E`, and logical pages
1 through 125. Object 2 has base address `0x00090000`, virtual size `0x00023670`,
and logical pages 126 through 149. Its stored bytes occupy addresses
`0x00090000..0x000A73EF`; its remaining virtual range is
`0x000A73EF..0x000B3670`. Those remaining bytes have no initialized source bytes
in the object page payload. They must not be read from the end of the file.

The header selects object 1 offset `0x00060E64` as the entry, giving address
`0x00070E64`. This describes the file's declared layout. It does not establish
the runtime selector values, initialization of the uninitialized region, or all
instruction and function boundaries.

## Interpretation

The stored pages and virtual object extents are different ranges. A static
mapping must preserve the initialized payload and the uninitialized tail
separately. FND-RES-003 remains the evidence for the resource decoder; this
container observation establishes no decoder behavior or complete code reading.

## Alternatives

Treating the page-data field as relative to the LE header shifts every source
page. Treating the virtual data size as stored bytes reads beyond its payload.
The declared fields and the stored page count distinguish those interpretations.
The observation does not determine how the runtime initializes the virtual tail.

## How to reproduce

Use the executable identified by BLD-GOG-EN. Read little-endian header fields at
file offsets `0x00026654` and `0x000290FC`, follow the LE object-table pointer,
and compare each object's virtual size with its logical-page span. Add the
page-data field to the embedded MZ module's file offset, not the LE header's
file offset. Multiply complete pages by 4,096 and use the declared 1,007-byte
length for the final stored page. Verify the two object bases and the entry's
object-relative offset without assuming that raw fixup operands are relocated.
