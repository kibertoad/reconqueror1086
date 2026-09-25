meta:
  id: fmt_talk_007
  title: Value, a record of ALL.TMB
  license: MIT
  endian: le
doc-ref: FMT-TALK-007
seq:
  - id: kind
    type: s4
  - id: value
    type: s4
  - id: flag
    type: s4
  - id: argument_count
    type: s4
  - id: arguments
    type: s4
    repeat: expr
    repeat-expr: argument_count
