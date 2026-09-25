---
id: FND-VIEW-015
title: The backdrop blitter copies BackImage rows from column heading * scale, wrapping, and aligns the backdrop horizon with the view horizon
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043B8C..0x00043CD4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00046AF4
tool: Ghidra 12.1.3
environment: null
---

## Observation

Routine `0x00043B8C`..`0x00043CD4` returns at once when the backdrop image pointer is 0. It
takes a heading and a view horizon row as arguments. It sets the source column to
`heading * scale`, subtracts the backdrop width once when that is above the width, and takes
`first = min(width - column, view_width)`. It sets `shift = view_horizon - backdrop_horizon`;
when that is negative it starts `-shift` rows into the image and at view row 0, otherwise at
image row 0 and view row `shift`. It copies `min(view_height, image_rows - skipped_rows)` rows.
For each it copies `first` bytes from the source column, and when `first` is below the view
width it copies the remaining `view_width - first` bytes from the start of the same image row.
Rows of the view it does not reach are left as they were. World renderer `0x00046AF4` calls it
before drawing any surface.

## Interpretation

The panorama turns with the view, three image columns to one heading unit, wraps around its
width, and is placed so that its own horizon row meets the view's horizon row. For a view
117 rows high (horizon 58) the image rows 141 to 199 fill view rows 0 to 58, and the rows below
are left for the surfaces drawn next.

## Alternatives

An earlier reading took the backdrop as four overlapping 320-pixel screens cropped to the view,
and another scaled a 640-pixel strip. Both are ruled out by the copy loop, which never scales
and moves one image byte to one view byte.

## How to reproduce

Open `0x00043B8C`; the multiply by the scale, the subtraction of the backdrop horizon from the
argument and the two `rep movsd` copies per row are in the first 80 instructions.
