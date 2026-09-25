meta:
  id: fmt_strategy_004
  title: Route file, a list of route points
  license: MIT
  endian: le
doc-ref: FMT-STRATEGY-004
seq:
  - id: point_count
    type: s4
  - id: points
    type: s4
    repeat: expr
    repeat-expr: point_count * 2
    doc: x and y of each point in route units.
