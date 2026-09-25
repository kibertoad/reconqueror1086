meta:
  id: fmt_res_004
  title: Kind-2 stream, the stored bytes of a kind-2 entry
  license: MIT
  endian: be
doc-ref: FMT-RES-004, FND-RES-004
seq:
  - id: codes
    size-eos: true
    doc: LZW codes of 9 to 14 bits, most significant bit first, ending with code 257.
