meta:
  id: fmt_res_001
  title: Resource container, a GOB, RES or LOW file
  license: MIT
  endian: le
  imports:
    - fmt_res_002
doc-ref: FMT-RES-001, FND-RES-001, FND-RES-002
seq:
  - id: magic
    contents: '.RES'
  - id: directory_offset
    type: u4
instances:
  entry_count:
    pos: directory_offset
    type: u4
  directory:
    pos: directory_offset + 4
    type: fmt_res_002
    repeat: expr
    repeat-expr: entry_count
