meta:
  id: fmt_media_001
  title: CSF sprite file
  license: MIT
  endian: le
doc-ref: FMT-MEDIA-001, FND-MEDIA-001, FND-MEDIA-002
seq:
  - id: version
    contents: "2J"
  - id: count
    type: u4
  - id: sizes
    type: u4
    repeat: expr
    repeat-expr: count
  - id: frames
    size: sizes[_index]
    type: fmt_media_002
    repeat: expr
    repeat-expr: count
types:
  fmt_media_002:
    seq:
      - id: width
        type: u2
      - id: height
        type: u2
      - id: rows
        type: row
        repeat: expr
        repeat-expr: height
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
