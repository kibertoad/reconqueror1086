meta:
  id: fmt_battle_001
  title: Field battle unit, one troop in an interactive field battle
  license: MIT
  endian: le
doc-ref: FMT-BATTLE-001
seq:
  - id: category
    type: s4
  - id: x
    type: s4
  - id: y
    type: s4
  - id: dest_x
    type: s4
  - id: dest_y
    type: s4
  - id: heading
    type: s4
  - id: phase
    type: s4
  - id: state
    type: s4
    enum: unit_state
  - id: strength
    type: s4
  - id: value
    type: s4
  - id: control
    type: s4
  - id: lane
    type: s4
  - id: target
    type: s4
enums:
  unit_state:
    0x00: unit_idle
    0x28: unit_fighting
    0x50: unit_dying
