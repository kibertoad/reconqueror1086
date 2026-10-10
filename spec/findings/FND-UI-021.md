---
id: FND-UI-021
title: Youth Continue and Reroll bind file-order indices rather than stored region IDs
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000147AC..0x000147CE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059C4C..0x00059C6E
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00059D34..0x00059D4D
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0005A2A4..0x0005A414
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x013BC6B2..0x013BC73F
tool: Capstone 5.0.7, bounded x86-32 decoding of the fingerprinted relocated LE image and bounded archive-record decoding
environment: null
---

## Observation

The youth setup passes callback `0x00015054`, slot 2 and index 5 to
`0x00059C4C`; it then passes callback `0x000151F8`, slot 2 and index 6.
FND-PERSON-004 identifies the former as Continue and FND-UI-013 identifies
the latter as Reroll.

The binding helper reads the current object's pointer array at offset 108
and selects its first argument's array position. It writes the callback at
region offset 64 plus four times the slot. It never reads the region's ID.
The enable helper `0x00059D34` uses the same array-position selection and
writes the region's enabled field at offset 8. The loader stores newly
created regions successively in that array in file order, as FND-UI-002
records; there is no ID-based reordering in this loop.

Independent decoding of CHARGEN.HAT's kind-1 archive entry yields 208 bytes:
a 40-byte header followed by seven 24-byte records. Zero-based record 5 has
stored ID 6, rectangle (473, 132, 97, 40), and enabled value 1. Record 6 has
stored ID 5, rectangle (470, 48, 110, 30), and enabled value 1.

## Interpretation

Continue is array index 5, stored ID 6, at (473, 132, 97, 40). Reroll is
array index 6, stored ID 5, at (470, 48, 110, 30). Enabling index 5 after
an answer targets Continue. The earlier screen table associated the effects
with stored IDs instead of callback indices and consequently reversed them.

## Alternatives

An ID-based callback binding would reverse these effects. The helper's
direct array selection and the loader's file-order stores rule it out.
This finding does not establish physical input dispatch or button artwork.

## How to reproduce

Verify the BLD-GOG-EN executable fingerprint, relocate its LE objects and
decode the listed bounded instruction ranges. Read the CHARGEN.HAT directory
entry, bound its stored extent, decode its kind-1 block to the declared size,
and compare records 5 and 6 with the two setup bindings. Compare callback
effects with FND-PERSON-004 and FND-UI-013.
