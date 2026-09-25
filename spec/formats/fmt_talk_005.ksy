meta:
  id: fmt_talk_005
  title: Action, a record of ALL.TMB
  license: MIT
  endian: le
doc-ref: FMT-TALK-005
seq:
  - id: kind
    type: s4
  - id: expression
    type: s4
  - id: branch_count
    type: s4
  - id: branches
    type: s4
    repeat: expr
    repeat-expr: branch_count
