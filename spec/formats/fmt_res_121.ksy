meta:
  id: fmt_res_121
  title: Pointer hot spots, an MVG entry
  license: MIT
  endian: le
doc-ref: FMT-RES-121, FND-RES-059
seq:
  - id: magic
    contents: [0x34, 0x12, 0xad, 0xde]
  - id: count
    type: s4
  - id: buffer_size
    type: s4
  - id: records
    type: record
    repeat: expr
    repeat-expr: count
types:
  record:
    seq:
      - id: unk_00
        type: s4
      - id: unk_04
        type: s4
      - id: hot_x
        type: s4
      - id: hot_y
        type: s4
