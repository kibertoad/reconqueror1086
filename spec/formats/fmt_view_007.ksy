meta:
  id: fmt_view_007
  title: Colour map, one of the Pal0 to Pal127 resources
  license: MIT
  endian: le
doc-ref: FMT-VIEW-007, FND-VIEW-016
seq:
  - id: index
    type: u1
    repeat: expr
    repeat-expr: 256
    doc: Palette index drawn for each source palette index.
