meta:
  id: fmt_talk_008
  title: Conversation variables, ALL.VTB
  license: MIT
  endian: le
doc-ref: FMT-TALK-008
seq:
  - id: count
    type: s4
  - id: element_size
    type: s4
  - id: element_kind
    type: s4
  - id: values
    size: count * element_size
