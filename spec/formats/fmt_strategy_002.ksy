meta:
  id: fmt_strategy_002
  title: Property record, one castle of the strategic map
  license: MIT
  endian: le
doc-ref: FMT-STRATEGY-002
seq:
  - id: state
    type: u1
  - id: cell_row
    type: u2
  - id: cell_col
    type: u2
  - id: map_x
    type: u2
  - id: map_y
    type: u2
  - id: lord
    type: u1
  - id: next
    type: u2
  - id: garrison
    type: u1
  - id: alerted
    type: u1
  - id: approached
    type: u1
