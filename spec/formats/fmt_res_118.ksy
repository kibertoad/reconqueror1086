meta:
  id: fmt_res_118
  title: SETUP.SOL directory record
  license: MIT
  endian: le
doc-ref: FMT-RES-118, FND-RES-030
seq:
  - id: tag
    type: str
    size: 3
    encoding: ASCII
    doc: DH9; any other value ends the directory reading.
  - id: kind
    type: str
    size: 1
    encoding: ASCII
    doc: Kinds 1 and 8 carry the two words at 0x16.
  - id: name
    type: strz
    size: 14
    encoding: ASCII
    doc: Member name; THE_END marks the end of the directory.
  - id: stored_size
    type: u4
  - id: unk_16
    type: u2
    if: kind == "1" or kind == "8"
  - id: unk_18
    type: u2
    if: kind == "1" or kind == "8"
