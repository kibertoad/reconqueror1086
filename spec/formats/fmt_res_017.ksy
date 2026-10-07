meta:
  id: fmt_res_017
  title: SETUP.SOL, the first-stage installer's member archive
  license: MIT
  endian: le
  imports:
    - fmt_res_118
doc-ref: FMT-RES-017, FND-RES-030
seq:
  - id: header
    size: 6
    doc: Read and never tested; ASCII DISK01 in the shipped file.
  - id: directory
    type: fmt_res_118
    repeat: until
    repeat-until: _.name == "THE_END" or _.tag != "DH9"
    doc: Records up to and including THE_END.
  - id: members
    size-eos: true
    doc: Stored streams packed in directory order, expanded as RULE-RES-005 gives.
