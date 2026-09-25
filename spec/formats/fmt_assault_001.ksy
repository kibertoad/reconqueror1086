meta:
  id: fmt_assault_001
  title: Combatant record, one of the actors of a first-person assault
  license: MIT
  endian: le
doc-ref: FMT-ASSAULT-001
seq:
  - id: unk_00
    size: 8
  - id: effect
    type: s4
    doc: The live effect the combatant is running, or -1.
  - id: x
    type: s4
    doc: Position along x in 8.8 map units.
  - id: y
    type: s4
    doc: Position along y in 8.8 map units.
  - id: unk_14
    size: 4
  - id: mode
    type: s4
    enum: mode
  - id: next_mode
    type: s4
    enum: mode
  - id: fallback_mode
    type: s4
    enum: mode
  - id: target
    type: s4
    doc: Index in the combatant list of the combatant it pursues, strikes or follows.
  - id: dest_x
    type: s4
  - id: dest_y
    type: s4
  - id: side
    type: s4
    doc: 0 for the player's side.
  - id: skill
    type: s4
  - id: unk_38
    size: 4
  - id: armor
    type: s4
  - id: health
    type: s4
enums:
  mode:
    1: mode_formation
    2: mode_defend
    3: mode_seek_friend
    4: mode_seek_enemy
    5: mode_retreat
    6: mode_attack
    7: mode_regroup
    8: mode_pursue
    9: mode_flee_friend
    10: mode_flee
    11: mode_strike
    12: mode_go_to
    13: mode_rally
    14: mode_hit
    15: mode_dying
    16: mode_follow
    17: mode_follow_search
