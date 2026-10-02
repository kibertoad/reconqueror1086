meta:
  id: fmt_res_007
  title: Counted CD-root driver containers
  license: MIT
  endian: le
doc-ref: FMT-RES-007, FND-RES-013
seq:
  - id: driver_unk_00
    type: u4
  - id: driver_reserved
    size: 28
  - id: driver_record_count
    type: u4
    valid:
      max: (_io.size - 44) / 48
  - id: driver_records_offset
    type: u4
    valid: 44
  - id: driver_unk_28
    type: u4
  - id: records
    type: driver_record
    repeat: expr
    repeat-expr: driver_record_count
types:
  driver_record:
    seq:
      - id: driver_name_region
        size: 32
      - id: driver_unk_record_20
        type: u4
      - id: driver_payload_length
        type: u4
        valid:
          expr: _ + 92 == driver_unk_record_20 and _ <= _io.size - _io.pos - 8
      - id: driver_unk_record_28
        type: u4
      - id: driver_unk_record_2c
        type: u4
      - id: driver_payload
        size: driver_payload_length
