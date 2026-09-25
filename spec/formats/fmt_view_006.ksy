meta:
  id: fmt_view_006
  title: Backdrop pixels, the BackImage resource
  license: MIT
  endian: le
doc-ref: FMT-VIEW-006, FND-VIEW-014
params:
  - id: width
    type: s4
  - id: height
    type: s4
seq:
  - id: pixels
    size: width * height
    doc: Palette index of each pixel, row by row.
