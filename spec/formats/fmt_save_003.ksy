meta:
  id: fmt_save_003
  title: Properties, persons, items and variables, PROPERTY.SAV
  license: MIT
  endian: le
doc-ref: FMT-SAVE-003
seq:
  - id: globals
    type: s4
    repeat: expr
    repeat-expr: 9
  - id: item_counts
    type: s4
    repeat: expr
    repeat-expr: 70
  - id: properties
    size: 15
    repeat: expr
    repeat-expr: 14
  - id: person_bytes
    size: 12
    repeat: expr
    repeat-expr: 176
