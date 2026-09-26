meta:
  id: fmt_media_003
  title: PCX picture as the game reads it
  license: MIT
  endian: le
doc-ref: FMT-MEDIA-003, FND-MEDIA-005, FND-MEDIA-006
seq:
  - id: manufacturer
    contents: [0x0a]
  - id: version
    type: u1
  - id: encoding
    type: u1
  - id: bits
    type: u1
  - id: xmin
    type: u2
  - id: ymin
    type: u2
  - id: xmax
    type: u2
  - id: ymax
    type: u2
  - id: resolution
    type: u2
    repeat: expr
    repeat-expr: 2
  - id: ega_palette
    size: 48
  - id: reserved
    type: u1
  - id: planes
    type: u1
  - id: bytes_per_line
    type: u2
  - id: palette_type
    type: u2
  - id: filler
    size: 58
  - id: rows
    size: _io.size - 128 - 769
    doc: RLE rows; see FMT-MEDIA-003.
  - id: palette_marker
    type: u1
  - id: palette
    size: 768
