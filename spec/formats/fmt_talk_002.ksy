meta:
  id: fmt_talk_002
  title: Conversation node, one record of ALL.CBF
  license: MIT
  endian: le
doc-ref: FMT-TALK-002
seq:
  - id: portrait_length
    type: s4
  - id: speaker_length
    type: s4
  - id: prompt_count
    type: u1
  - id: unk_009
    size: 3
  - id: prompt_lengths
    type: s4
    repeat: expr
    repeat-expr: 10
  - id: response_lengths
    type: s4
    repeat: expr
    repeat-expr: 5
  - id: response_count
    type: u1
  - id: unk_049
    size: 3
  - id: targets
    type: s4
    repeat: expr
    repeat-expr: 5
  - id: response_actions
    type: s4
    repeat: expr
    repeat-expr: 150
  - id: node_actions
    type: s4
    repeat: expr
    repeat-expr: 30
  - id: unk_330
    type: s4
    repeat: expr
    repeat-expr: 5
  - id: unk_344
    type: s4
  - id: strings
    size-eos: true
    doc: The strings, each length + 1 bytes, in the order the Layout section gives.
