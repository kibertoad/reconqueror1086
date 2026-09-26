meta:
  id: fmt_save_005
  title: Home fief, ~~3.SAV
  license: MIT
  endian: le
doc-ref: FMT-SAVE-005
seq:
  - id: unk_abe8
    type: s4
  - id: estate_block
    size: 12
  - id: home_fief
    size: 56
  - id: list_28
    size: 628
  - id: list_2c
    size: 688
  - id: list_34
    size: 416
  - id: list_30
    size: 296
  - id: chains
    size-eos: true
    doc: 12-byte nodes; their number comes from the length fields of the list rows.
