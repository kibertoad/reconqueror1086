---
id: FND-RES-008
title: Kind-1 blocks are neither the LSB-first LZW of other Dynamix files nor classic LH1
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
tool: Container census written for this project
environment: null
---

## Observation

Reading kind-1 blocks as the LSB-first LZW with 9 to 12 bit codes that other Dynamix games use
inside their resources gives undefined dictionary codes at the start of the stream and no valid
output. Reading them as classic LH1 (LZHUF) with a 4 KiB window, reset every 16,384 output bytes,
under either bit order, gives no valid PCX header for any of the 187 kind-1 `.PCX` and `.PCC`
entries of `C1086.GOB`.

## Interpretation

Neither codec is kind 1. The executable's own decoder (FND-RES-003) gives the format.

## Alternatives

None known.

## How to reproduce

Run both decoders over the kind-1 blocks of the `.PCX` and `.PCC` entries of `C1086.GOB`.
