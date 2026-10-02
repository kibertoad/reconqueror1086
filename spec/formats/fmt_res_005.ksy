meta:
  id: fmt_res_005
  title: Raw disc carrier with cue-delimited spans
  license: MIT
  endian: le
  imports:
    - fmt_res_116
doc-ref: FMT-RES-005, FND-RES-011, FND-RES-012
params:
  - id: num_disc_data_records
    type: u4
    doc: First audio start from the paired cue, expressed in 2352-byte records.
seq:
  - id: disc_data_records
    type: fmt_res_116
    repeat: expr
    repeat-expr: num_disc_data_records
    doc: Includes the observed duplicate and complete-zero padding records.
  - id: disc_audio_bytes
    size-eos: true
    doc: Cue-labelled audio span; sample interpretation is outside this definition.
