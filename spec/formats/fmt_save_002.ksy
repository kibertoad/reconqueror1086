meta:
  id: fmt_save_002
  title: Strategic map state, TROOPS.SAV
  license: MIT
  endian: le
doc-ref: FMT-SAVE-002
seq:
  - id: planting_notice
    type: s4
  - id: king_order
    size: 32
  - id: globals
    type: s4
    repeat: expr
    repeat-expr: 17
  - id: player_forces
    size: 280
    repeat: expr
    repeat-expr: 6
  - id: brigands
    type: brigand
    repeat: expr
    repeat-expr: 3
  - id: hostiles
    type: hostile
    repeat: expr
    repeat-expr: 5
  - id: unk_ab170
    size: 4028
types:
  route_block:
    params:
      - id: count
        type: s4
    seq:
      - id: marker
        type: u4
      - id: points
        type: s4
        repeat: expr
        repeat-expr: count * 2
        if: marker == 0x1111
  brigand:
    seq:
      - id: order
        size: 32
      - id: record
        size: 280
      - id: route
        type: route_block(record_count)
    instances:
      record_count:
        value: record[0x18] + (record[0x19] << 8) + (record[0x1A] << 16) + (record[0x1B] << 24)
  hostile:
    seq:
      - id: record
        size: 280
      - id: route
        type: route_block(record_count)
    instances:
      record_count:
        value: record[0x18] + (record[0x19] << 8) + (record[0x1A] << 16) + (record[0x1B] << 24)
