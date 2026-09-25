meta:
  id: fmt_view_004
  title: Scene settings, the Scenario resource
  license: MIT
  endian: le
doc-ref: FMT-VIEW-004, FND-VIEW-013
seq:
  - id: unk_00
    size: 20
  - id: texture_count
    type: s4
  - id: block_count
    type: s4
  - id: effect_count
    type: s4
  - id: unk_20
    size: 8
  - id: color_maps_on
    type: s4
  - id: color_map_count
    type: s4
  - id: distance_shift
    type: s4
  - id: fade_color
    type: s4
    doc: Palette index the maps fade towards.
  - id: unk_38
    size: 512
