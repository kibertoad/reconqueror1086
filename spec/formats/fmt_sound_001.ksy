meta:
  id: fmt_sound_001
  title: Sound bank, a .666 entry
  license: MIT
  endian: le
doc-ref: FMT-SOUND-001, FND-SOUND-001
seq:
  - id: tag
    contents: [0x31, 0x50, 0x4a, 0x00]
  - id: samples
    type: sample
    repeat: eos
types:
  sample:
    seq:
      - id: length
        type: u4
      - id: rate
        type: u4
      - id: data
        size: length
