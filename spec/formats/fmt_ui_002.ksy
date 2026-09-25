meta:
  id: fmt_ui_002
  title: Screen region, one record of a HAT file
  license: MIT
  endian: le
doc-ref: FMT-UI-002, FND-UI-002
seq:
  - id: id
    type: s4
    doc: Kept with the region; callbacks are bound by position.
  - id: x
    type: s4
  - id: y
    type: s4
  - id: width
    type: s4
  - id: height
    type: s4
  - id: enabled
    type: s4
    enum: enabled
enums:
  enabled:
    0: off
    1: on
