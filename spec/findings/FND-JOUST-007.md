---
id: FND-JOUST-007
title: The lance1.csf colours match the practice movie palette and not the DRJSTWIN.PCX palette
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER/JOUSPRAC.SMK
    offset: 0x00..0xC16F4
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00..0x21B93B2
tool: Resource decoders written for this project
environment: null
---

## Observation

Decoding `CD:CONQUER/JOUSPRAC.SMK` gives 88 frames of 640 by 300 pixels at 71 ms, with a
palette only in frame 0. The 25 frames of `lance1.csf` in `C1086.GOB` use 22 palette indices in 176,061 opaque
pixels. Weighting each used index by how often it occurs, the mean absolute difference per RGB
channel from the movie's palette is 2.673, and from the palette of `DRJSTWIN.PCX` it is 29.382.

## Interpretation

The practice lance is drawn with the practice movie's palette.

## Alternatives

The lance could be drawn with the palette of `DRJSTWIN.PCX`, as the dragon run does. The
colour distances rule that out for practice.

## How to reproduce

Decode the three resources and average
`count[i] * abs(movie_rgb[i][c] - other_rgb[i][c])` over the used indices and the three
channels, divided by the total count.
