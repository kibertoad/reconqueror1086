meta:
  id: fmt_view_008
  title: Scene texture, a TEX resource
  license: MIT
  endian: le
doc-ref: FMT-VIEW-008, FND-VIEW-018
params:
  - id: width
    type: s4
  - id: height
    type: s4
seq:
  - id: pixels
    size: width * height
    doc: Palette index of each pixel; 0 is transparent.
