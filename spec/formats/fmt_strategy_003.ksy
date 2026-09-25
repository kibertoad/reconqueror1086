meta:
  id: fmt_strategy_003
  title: Person record, one character of the strategic map
  license: MIT
  endian: le
doc-ref: FMT-STRATEGY-003
seq:
  - id: name
    type: u4
    doc: Pointer to the name string.
  - id: group
    type: u1
  - id: county
    type: u1
  - id: flags
    type: u1
  - id: assignment
    type: u1
  - id: cell_row
    type: u2
  - id: cell_col
    type: u2
  - id: rating
    type: u1
  - id: next
    type: u1
  - id: unk_0e
    size: 4
