meta:
  id: fmt_media_006
  title: Smacker 2 movie header
  license: MIT
  endian: le
doc-ref: FMT-MEDIA-006, FND-MEDIA-008
seq:
  - id: signature
    contents: "SMK2"
  - id: width
    type: u4
  - id: height
    type: u4
  - id: frame_count
    type: u4
  - id: frame_rate
    type: s4
  - id: flags
    type: u4
  - id: audio_size
    type: u4
    repeat: expr
    repeat-expr: 7
  - id: trees_size
    type: u4
  - id: tree_sizes
    type: u4
    repeat: expr
    repeat-expr: 4
  - id: audio_rate
    type: u4
    repeat: expr
    repeat-expr: 7
  - id: reserved
    type: u4
  - id: frame_sizes
    type: u4
    repeat: expr
    repeat-expr: frame_count
  - id: frame_types
    type: u1
    repeat: expr
    repeat-expr: frame_count
  - id: trees
    size: trees_size
