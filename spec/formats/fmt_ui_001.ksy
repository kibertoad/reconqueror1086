meta:
  id: fmt_ui_001
  title: Screen layout, a HAT file
  license: MIT
  endian: le
  imports:
    - fmt_ui_002
doc-ref: FMT-UI-001, FND-UI-002
seq:
  - id: screen
    type: s4
    doc: The screen number the file is registered under.
  - id: origin_x
    type: s4
  - id: origin_y
    type: s4
  - id: width
    type: s4
  - id: height
    type: s4
  - id: region_count
    type: s4
    doc: Sizes the pointer array; the loader counts the records from the file size.
  - id: background
    type: strz
    size: 13
    encoding: ASCII
  - id: unk_25
    size: 3
    doc: Never read.
  - id: regions
    type: fmt_ui_002
    repeat: expr
    repeat-expr: region_count
