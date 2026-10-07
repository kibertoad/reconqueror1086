meta:
  id: fmt_res_123
  title: MIDI song, an HMP entry
  license: MIT
  endian: le
doc-ref: FMT-RES-123, FND-SOUND-005
seq:
  - id: tag
    type: str
    size: 14
    encoding: ASCII
  - id: body
    size-eos: true
