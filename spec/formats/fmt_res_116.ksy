meta:
  id: fmt_res_116
  title: Leading raw-track record
  license: MIT
  endian: le
doc-ref: FMT-RES-116, FND-RES-012
seq:
  - id: raw_sync
    size: 12
    doc: Stored prefix, including the entirely-zero padding case.
  - id: raw_position
    size: 3
    doc: Stored address bytes; do not infer physical position from them.
  - id: raw_mode
    type: u1
    enum: raw_modes
  - id: raw_payload
    size: 2048
    doc: Physically indexed ISO payload region in the leading span.
  - id: raw_trailer
    size: 288
    doc: Stored trailer bytes; validation roles remain unread.
enums:
  raw_modes:
    0: raw_blank
    1: raw_mode_1
