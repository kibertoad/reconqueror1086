meta:
  id: fmt_config_002
  title: Setting node, one key and value of the loaded CONQUER.INI
  license: MIT
  endian: le
doc-ref: FMT-CONFIG-002
seq:
  - id: key
    type: u4
    doc: Address of the key string.
  - id: value
    type: u4
    doc: Address of the value string.
  - id: next
    type: u4
    doc: Address of the next node, or 0 in the last.
