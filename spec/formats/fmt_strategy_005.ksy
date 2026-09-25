meta:
  id: fmt_strategy_005
  title: Strategic terrain grid, icon.jp
  license: MIT
  endian: le
doc-ref: FMT-STRATEGY-005
seq:
  - id: cell_width
    type: s4
  - id: cell_height
    type: s4
  - id: rows
    type: s4
  - id: cols
    type: s4
  - id: cells
    type: u4
    repeat: expr
    repeat-expr: rows * cols
    doc: Column by column. Bits 0-15 tile, bits 16-23 person, bits 24-31 unknown.
