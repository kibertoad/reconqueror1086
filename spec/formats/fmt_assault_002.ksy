meta:
  id: fmt_assault_002
  title: Combat row, one of the 25 weapon records in the executable
  license: MIT
  endian: le
doc-ref: FMT-ASSAULT-002, FND-ASSAULT-032
seq:
  - id: dice
    type: s4
  - id: sides
    type: s4
  - id: penetration
    type: s4
  - id: swing_divisor
    type: s4
  - id: reach
    type: s4
    doc: Contact distance in 8.8 map units.
  - id: unk_14
    size: 8
