---
id: FND-MEDIA-006
title: All 192 PCX and PCC entries of C1086.GOB are version 5 8-bit pictures with a palette trailer
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B1
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1C7C310..0x1C82853
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1CA9D9D..0x1CC978E
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x1C42856..0x1C48EB0
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x16B7915..0x16C2514
tool: Container census written for this project
environment: null
---

## Observation

`C1086.GOB` holds 143 `.PCX` and 49 `.PCC` entries; every one decodes to a PCX picture with byte 0
`0x0A`, version 5, encoding 1, 8 bits, one plane, `xmin` and `ymin` 0, and a `0x0C` byte followed by
768 palette bytes at the very end. In all 192 the RLE rows of the even-rounded width end exactly at
that `0x0C`, and no run crosses a row end or has a count of 0.

The `.PCX` entries are 640x480 (134), 195x203 (5, `MONEYLE1` to `MONEYLE4` and `MONEYLEN`), and one
each of 1024x728 (`battle.pcx`), 1024x41, 800x41 and 640x40 (`warbt102`, `warbt800`, `warbt640`).
Four are kind 2 (`prog.pcx`, `champion.pcx`, `crowning.pcx`, `death.pcx`), the rest kind 1;
`engmap1.pcx` is a kind-1 640x480 picture. The `.PCC` entries are portraits: 37 at 195x203, 7 at
135x160 (the `j*.pcc` tournament faces), 4 at 194x202 (`fakey`, `monylend`, `nellie`, `serf`) and
`note.pcc` at 196x204. Only `richard.pcc` is kind 0.

The 49 pictures with an odd width store `width + 1` bytes per row (`0x42` of the header), and
the decoded extra column is index 255 in most of them: `richard.pcc` and 40 others hold 255 in
every row, `otto.pcc` and `jearlwil.pcc` 254, and five (`monylen1`, `monylen2`, `monylen3`,
`monylen5`, `hughbeat`) hold picture pixels. The header's palette-type field is 0 in 132 `.PCX` and
32 `.PCC` entries and 1 in the rest; the resolution fields are 640x480 in most.

`ttents.hat` in `C1086.GOB` also begins with `0x0A`, version 0, and is not a picture.

## Interpretation

The pictures are the PCX layout of FMT-MEDIA-003. Because FND-MEDIA-005 draws the even-rounded
width, an odd-width portrait is drawn one column wider than its header says, the extra column
coming from the stored padding.

## Alternatives

None known.

## How to reproduce

Decode each `.PCX` and `.PCC` entry of `C1086.GOB` (RULE-RES-001) and read its header, rows and
trailer.
