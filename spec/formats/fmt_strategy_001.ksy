meta:
  id: fmt_strategy_001
  title: Strategic movement record, one force on the strategic map
  license: MIT
  endian: le
doc-ref: FMT-STRATEGY-001
seq:
  - id: active
    type: s4
  - id: selected
    type: s4
  - id: unk_08
    type: s4
  - id: complete
    type: s4
  - id: reversed
    type: s4
  - id: target
    type: s4
  - id: count
    type: s4
  - id: swordsmen
    type: s4
  - id: halberdiers
    type: s4
  - id: knights
    type: s4
  - id: origin
    type: s4
  - id: lord
    type: s4
  - id: cursor
    type: s4
  - id: mode
    type: s4
    enum: mode
  - id: frame
    type: s4
  - id: dest_x
    type: s4
  - id: dest_y
    type: s4
  - id: cell_row
    type: s4
  - id: cell_col
    type: s4
  - id: unk_4c
    size: 8
  - id: cooldown
    type: s4
  - id: terrain_kind
    type: s4
  - id: x
    type: f4
  - id: y
    type: f4
  - id: dir_x
    type: f4
  - id: dir_y
    type: f4
  - id: route
    type: u4
    doc: Pointer to the route points, or 0.
  - id: points
    type: s4
    repeat: expr
    repeat-expr: 40
  - id: image
    type: u4
  - id: unk_114
    size: 4
enums:
  mode:
    1: move_direct
    2: move_routed
    3: move_pursuit
