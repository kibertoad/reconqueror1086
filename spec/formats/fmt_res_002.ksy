meta:
  id: fmt_res_002
  title: Container directory record
  license: MIT
  endian: le
doc-ref: FMT-RES-002, FND-RES-001, FND-RES-002
seq:
  - id: name
    type: strz
    size: 32
    encoding: ASCII
  - id: kind
    type: u4
    enum: storage
  - id: unk_24
    type: u4
    doc: Never read; 0 in every shipped record.
  - id: stored_size
    type: u4
  - id: expanded_size
    type: u4
  - id: offset
    type: u4
instances:
  stored:
    io: _root._io
    pos: offset
    size: stored_size
  kind1_output:
    io: _root._io
    pos: offset
    size: stored_size
    process: rule_res_002
    if: kind == storage::blocks
    doc: The entry's bytes, expanded_size long, decoded as RULE-RES-002 gives.
  kind2_output:
    io: _root._io
    pos: offset
    size: stored_size
    process: rule_res_003
    if: kind == storage::lzw
    doc: The entry's bytes, expanded_size long, decoded as RULE-RES-003 gives.
enums:
  storage:
    0: plain
    1: blocks
    2: lzw
    3: kind_3
