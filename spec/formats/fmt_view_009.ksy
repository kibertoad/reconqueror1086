meta:
  id: fmt_view_009
  title: Combat palette, 256 colours of three bytes
  license: MIT
  endian: le
doc-ref: FMT-VIEW-009, FND-VIEW-017
seq:
  - id: rgb
    type: u1
    repeat: expr
    repeat-expr: 768
    doc: Red, green and blue of each colour, colour c at 3 * c.
