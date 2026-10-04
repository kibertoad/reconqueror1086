meta:
  id: fmt_res_014
  title: Owned disc-root indexed icons
  license: MIT
  endian: le
doc-ref: FMT-RES-014, FND-RES-020
seq:
  - id: ico_reserved
    type: u2
    valid: 0
  - id: ico_type
    type: u2
    valid: 1
  - id: ico_count
    type: u2
    valid:
      min: 1
      max: 2
  - id: ico_directory
    type: directory_record
    repeat: expr
    repeat-expr: ico_count
  - id: ico_payload_storage
    size-eos: true
types:
  directory_record:
    seq:
      - id: width
        type: u1
        valid:
          any-of: [16, 32]
      - id: height
        type: u1
        valid:
          any-of: [16, 32]
      - id: color_count
        type: u1
        valid: 16
      - id: reserved
        type: u1
        valid: 0
      - id: planes_hint
        type: u2
        valid: 0
      - id: bit_count_hint
        type: u2
        valid: 0
      - id: image_length
        type: u4
        valid:
          any-of: [296, 744]
      - id: image_offset
        type: u4
        valid:
          min: 6 + 16 * _root.ico_count
          max: _root._io.size - image_length
    instances:
      image:
        io: _root._io
        pos: image_offset
        size: image_length
        type: image_payload
  image_payload:
    seq:
      - id: header_size
        type: u4
        valid: 40
      - id: width
        type: s4
        valid:
          any-of: [16, 32]
      - id: combined_height
        type: s4
        valid: width * 2
      - id: planes
        type: u2
        valid: 1
      - id: bit_count
        type: u2
        valid: 4
      - id: compression
        type: u4
        valid: 0
      - id: image_size
        type: u4
        valid:
          any-of: [128, 512, 640]
      - id: x_resolution
        type: s4
        valid: 0
      - id: y_resolution
        type: s4
        valid: 0
      - id: colors_used
        type: u4
        valid: 0
      - id: colors_important
        type: u4
        valid: 0
      - id: palette
        size: 64
      - id: xor_plane
        size: ((width * 4 + 31) / 32) * 4 * (combined_height / 2)
      - id: and_plane
        size: ((width + 31) / 32) * 4 * (combined_height / 2)
