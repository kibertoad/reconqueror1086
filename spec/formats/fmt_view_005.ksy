meta:
  id: fmt_view_005
  title: Backdrop descriptor, the Backdrop resource
  license: MIT
  endian: le
doc-ref: FMT-VIEW-005, FND-VIEW-014
seq:
  - id: kind
    type: s4
  - id: unk_04
    size: 4
  - id: width
    type: s4
  - id: height
    type: s4
  - id: horizon
    type: s4
    doc: Image row that meets the view horizon.
  - id: scale
    type: s4
    doc: Image columns per heading unit.
