meta:
  id: fmt_media_004
  title: Stored palette, a PAL entry
  license: MIT
  endian: le
doc-ref: FMT-MEDIA-004, FND-MEDIA-007
seq:
  - id: colours
    type: colour
    repeat: expr
    repeat-expr: 256
types:
  colour:
    seq:
      - id: red
        type: u1
      - id: green
        type: u1
      - id: blue
        type: u1
