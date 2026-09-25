meta:
  id: fmt_talk_003
  title: Action index, ALL.TMI
  license: MIT
  endian: le
doc-ref: FMT-TALK-003
seq:
  - id: unk_00
    type: s4
  - id: records
    type: record
    repeat: eos
types:
  record:
    seq:
      - id: action
        type: s4
      - id: offset
        type: s4
