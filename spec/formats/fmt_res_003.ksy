meta:
  id: fmt_res_003
  title: Kind-1 stream, the stored bytes of a kind-1 entry
  license: MIT
  endian: le
doc-ref: FMT-RES-003, FND-RES-003
seq:
  - id: blocks
    type: block
    repeat: eos
types:
  block:
    seq:
      - id: length
        type: u2
      - id: marker
        type: u1
        doc: 0x80 for a stored block; the game decodes any other value as compressed.
      - id: body
        size: length - 1
        type:
          switch-on: marker
          cases:
            0x80: stored_body
            _: compressed_body
  stored_body:
    seq:
      - id: output
        size-eos: true
  compressed_body:
    seq:
      - id: unk_00
        type: u1
        doc: Never read.
      - id: control
        type: u2be
        doc: Flags of the first 16 tokens, bit 15 first.
      - id: tokens
        size-eos: true
        doc: Literal bytes, 2-byte copies and 4-byte runs, with a big-endian control word before each further group of 16.
