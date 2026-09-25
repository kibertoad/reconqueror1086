meta:
  id: fmt_talk_006
  title: Expression, a record of ALL.TMB
  license: MIT
  endian: le
doc-ref: FMT-TALK-006
seq:
  - id: value_count
    type: s4
  - id: operator_count
    type: s4
  - id: values
    type: s4
    repeat: expr
    repeat-expr: value_count
  - id: operators
    type: s4
    repeat: expr
    repeat-expr: operator_count
