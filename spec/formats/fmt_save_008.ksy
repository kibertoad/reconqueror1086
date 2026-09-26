meta:
  id: fmt_save_008
  title: Starting persons and properties, default.dat
  license: MIT
  endian: le
doc-ref: FMT-SAVE-008
seq:
  - id: person_bytes
    size: 8
    repeat: expr
    repeat-expr: 176
  - id: properties
    size: 15
    repeat: expr
    repeat-expr: 14
