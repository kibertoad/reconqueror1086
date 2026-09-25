meta:
  id: fmt_talk_004
  title: Action group, a record of ALL.TMB
  license: MIT
  endian: le
doc-ref: FMT-TALK-004
seq:
  - id: count
    type: s4
  - id: actions
    type: s4
    repeat: expr
    repeat-expr: count
