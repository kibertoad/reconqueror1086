meta:
  id: fmt_res_009
  title: Owned autoplay indexed bitmap
  license: MIT
  endian: le
doc-ref: FMT-RES-009, FND-RES-015
seq:
  - id: bmp_signature
    contents: [0x42, 0x4d]
  - id: bmp_file_size
    type: u4
    valid: 308278
  - id: bmp_reserved_1
    type: u2
    valid: 0
  - id: bmp_reserved_2
    type: u2
    valid: 0
  - id: bmp_pixel_offset
    type: u4
    valid: 1078
  - id: bmp_info_size
    type: u4
    valid: 40
  - id: bmp_width
    type: s4
    valid: 640
  - id: bmp_height
    type: s4
    valid: 480
  - id: bmp_planes
    type: u2
    valid: 1
  - id: bmp_bit_count
    type: u2
    valid: 8
  - id: bmp_compression
    type: u4
    valid: 0
  - id: bmp_image_size
    type: u4
    valid: 307200
  - id: bmp_x_resolution
    type: s4
    valid: 0
  - id: bmp_y_resolution
    type: s4
    valid: 0
  - id: bmp_palette_count
    type: u4
    valid: 256
  - id: bmp_important_count
    type: u4
    valid: 0
  - id: bmp_palette
    size: 1024
  - id: bmp_pixels
    size: 307200
