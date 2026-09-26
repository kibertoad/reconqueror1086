meta:
  id: fmt_view_001
  title: Scene block definition, one record of the Blocks resource
  license: MIT
  endian: le
  bit-endian: le
doc-ref: FMT-VIEW-001, FND-VIEW-001
seq:
  - id: kind
    type: s4
    enum: kind
    doc: Shape of the block.
  - id: see_through
    type: b1
    doc: Set, a ray that hits the block goes on.
  - id: blocks_movement
    type: b1
    doc: Set, movement cannot enter the cell.
  - id: mirrored
    type: b1
    doc: Set on a kind-4 block, the far half of its angles reuse the near half mirrored.
  - id: chained
    type: b1
    doc: Set with see_through, a ray follows state_target in the same cell.
  - id: actionable
    type: b1
    doc: Set, the player can act on the block.
  - id: weapon_contact
    type: b1
    doc: Set, the player's weapon replaces the block by its state_target when it reaches it.
  - id: opens_on_contact
    type: b1
    doc: Set, the block is used when the player walks into its cell.
  - id: actor
    type: b1
    doc: Set, with interaction 1, the block is a combatant.
  - id: selected
    type: b1
    doc: Set, the combatant is selected for orders.
  - id: unk_05_1
    type: b7
    doc: Purpose unknown.
  - id: unk_06
    size: 2
    doc: Purpose unknown.
  - id: color_family
    type: s2
    doc: First of the 32 colour maps used for the block.
  - id: unk_0a
    size: 2
    doc: Purpose unknown.
  - id: color_offset
    type: s4
    doc: Subtracted from the distance colour map index.
  - id: texture_width
    type: s4
  - id: width_shift
    type: s4
  - id: texture_height
    type: s4
  - id: lower
    type: s4
    doc: Elevation of the bottom of the block.
  - id: upper
    type: s4
    doc: Elevation of the top of the block.
  - id: offset_x
    type: s4
    doc: Signed 8.8 offset within the cell along x.
  - id: offset_y
    type: s4
    doc: Signed 8.8 offset within the cell along y.
  - id: surface0
    type: s2
  - id: effect
    type: s2
    doc: Effect descriptor started when the block's state begins (FMT-ASSAULT-003).
  - id: surface1
    type: s4
  - id: surface2
    type: s4
  - id: surface3
    type: s4
  - id: unk_3c
    size: 4
  - id: state_target
    type: s4
  - id: unk_44
    size: 2
  - id: movement
    type: s2
  - id: interaction
    type: s2
  - id: argument1
    type: s2
  - id: argument2
    type: s2
  - id: label
    type: strz
    size: 16
    encoding: ASCII
  - id: end
    contents: [0xcc, 0xcc]
enums:
  kind:
    0: block_empty
    1: block_solid
    2: block_wall_x
    3: block_wall_y
    4: block_sprite
    5: block_diagonal_a
    6: block_diagonal_b
