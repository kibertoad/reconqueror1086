meta:
  id: fmt_view_002
  title: Scene map, the Map resource
  license: MIT
  endian: le
doc-ref: FMT-VIEW-002, FND-VIEW-002
seq:
  - id: cells
    type: u2
    repeat: expr
    repeat-expr: 16384
    doc: Block number of each cell, cell (x, y) at index x * 128 + y.
