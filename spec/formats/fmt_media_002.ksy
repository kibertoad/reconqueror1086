meta:
  id: fmt_media_002
  title: CSF frame
  license: MIT
  endian: le
doc-ref: FMT-MEDIA-002, FND-MEDIA-001, FND-MEDIA-002, FND-MEDIA-003
seq:
  - id: width
    type: u2
  - id: height
    type: u2
  - id: rows
    type: row
    repeat: expr
    repeat-expr: height
types:
  row:
    seq:
      - id: segment_count
        type: u1
      - id: segments
        type: segment
        repeat: expr
        repeat-expr: segment_count
  segment:
    seq:
      - id: op
        type: u1
        enum: segment_op
      - id: length
        type: s2
      - id: pixels
        size: length
        if: op == segment_op::literal
      - id: colour
        type: u1
        if: op == segment_op::fill
enums:
  segment_op:
    0: literal
    1: skip
    2: fill
