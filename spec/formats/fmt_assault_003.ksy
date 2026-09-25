meta:
  id: fmt_assault_003
  title: Scene effect descriptor, one record of the SFXDEFS resource
  license: MIT
  endian: le
  bit-endian: le
doc-ref: FMT-ASSAULT-003, FND-ASSAULT-042
seq:
  - id: unk_00
    size: 12
  - id: tick_count
    type: s4
  - id: unk_10
    size: 4
  - id: interval
    type: s4
    doc: Milliseconds between ticks.
  - id: unk_flags_0
    type: b4
  - id: stop_when_blocked
    type: b1
  - id: unk_flags_5
    type: b1
  - id: turn_when_blocked
    type: b1
  - id: unk_flags_7
    type: b25
  - id: step_x
    type: s4
  - id: step_y
    type: s4
  - id: unk_24
    size: 4
  - id: block_selector
    type: s4
  - id: block_step
    type: s4
  - id: loop_block
    type: s4
  - id: surface_step
    type: s4
  - id: last_surface
    type: s4
  - id: heading_step
    type: s4
