meta:
  id: fmt_view_003
  title: Scene start position, the Viewer resource
  license: MIT
  endian: le
doc-ref: FMT-VIEW-003, FND-VIEW-012
seq:
  - id: x
    type: s4
    doc: Start x in 8.8 map units.
  - id: y
    type: s4
    doc: Start y in 8.8 map units.
  - id: elevation
    type: s4
  - id: heading
    type: s4
    doc: One turn is 0x10000.
  - id: unk_10
    size: 92
