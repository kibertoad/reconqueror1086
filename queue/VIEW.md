# VIEW

Next ID: Q-VIEW-027

## Static

- Q-VIEW-001. FMT-VIEW-001: What is the purpose of `unk_05_1`, `unk_06`, `unk_0A`, `unk_3C` and
  `unk_44`? Settles it: trace the named record or file through its loader and every relevant
  consumer, using the cited findings as entry points; record field widths and unresolved
  aliases. Blocks: none.

- Q-VIEW-002. FMT-VIEW-001: Which diagonal `BLOCK_DIAGONAL_A` and `BLOCK_DIAGONAL_B` run along?
  Settles it: trace the named record or file through its loader and every relevant consumer,
  using the cited findings as entry points; record field widths and unresolved aliases. Blocks:
  none.

- Q-VIEW-003. FMT-VIEW-001: What do the remaining interaction values and their dispatcher cases
  mean? Settles it: trace the named record or file through its loader and every relevant
  consumer, using the cited findings as entry points; record field widths and unresolved
  aliases. Blocks: none.

- Q-VIEW-004. FMT-VIEW-003: How the 16-bit `heading` becomes the byte heading the view uses?
  Settles it: trace the named record or file through its loader and every relevant consumer,
  using the cited findings as entry points; record field widths and unresolved aliases. Blocks:
  none.

- Q-VIEW-005. FMT-VIEW-003: What is the purpose of `unk_10`? Settles it: trace the named record
  or file through its loader and every relevant consumer, using the cited findings as entry
  points; record field widths and unresolved aliases. Blocks: none.

- Q-VIEW-006. FMT-VIEW-004: What is the purpose of `unk_00`, `unk_20` and `unk_38`? Settles it:
  trace the named record or file through its loader and every relevant consumer, using the cited
  findings as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-VIEW-007. FMT-VIEW-005: What is the purpose of `unk_04`, and what other `kind` values would
  do,? Settles it: trace the named record or file through its loader and every relevant
  consumer, using the cited findings as entry points; record field widths and unresolved
  aliases. Blocks: none.

- Q-VIEW-008. FMT-VIEW-007: Is it true that the game loads the stored maps or builds them with
  RULE-VIEW-006 when a scene starts is not recorded? Settles it: trace the named record or file
  through its loader and every relevant consumer, using the cited findings as entry points;
  record field widths and unresolved aliases. Blocks: none.

- Q-VIEW-009. FMT-VIEW-008: Are pixels stored in row or column order? Settles it: trace the
  named record or file through its loader and every relevant consumer, using the cited findings
  as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-VIEW-010. FMT-VIEW-009: Which channel-value range reaches the display? Settles it: trace the
  named record or file through its loader and every relevant consumer, using the cited findings
  as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-VIEW-011. RULE-VIEW-001: Is it true that the multiplication by `0x20` can overflow for the
  largest differences the game passes is not recorded? Settles it: read the relevant branch and
  its callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-VIEW-012. RULE-VIEW-002: Which products are formed in 32 bits and which in 64 bits? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-VIEW-013. RULE-VIEW-003: What exact contact, projection and opacity procedures replace the
  outlined helpers? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-VIEW-014. RULE-VIEW-003: What the original does when `depth` is 0 or negative, where the two
  projections divide by it,? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-VIEW-015. RULE-VIEW-003: Where the game keeps `scene_map`, `scene_blocks`, `view_width`,
  `horizon`, `view_elevation`, `hit_block`, `hit_depth`, `hit_x`, `hit_y`, `hit_cell_x` and
  `hit_cell_y`? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-VIEW-016. RULE-VIEW-003: When the two ray components have the same magnitude the procedure
  takes y as the major axis? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-VIEW-017. RULE-VIEW-003: Does a state-target chain retain the probe cell's offsets or use
  each target block's offsets? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-VIEW-018. RULE-VIEW-004: How `0x00044A28` computes the contact point for each shape,
  including rounding, which face a diagonal reports, and whether the contact includes the
  block's offsets? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-VIEW-019. RULE-VIEW-004: How the depth of a wall contact is measured (along the ray or along
  the view direction)? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-VIEW-020. RULE-VIEW-004: How `0x000447C4` rotates a kind-4 block's centre by the negative
  view heading, which gives `view_forward` and `view_across`? Settles it: read the relevant
  branch and its callers from the entry's cited findings, following data provenance, call
  effects and every exit relevant to this question. Blocks: none.

- Q-VIEW-021. RULE-VIEW-004: Which texture row `0x000444E8` reads for a view row between `top`
  and `bottom`? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-VIEW-022. RULE-VIEW-005: Where the game keeps `backdrop`, `back_image`, `view_height`,
  `view_pixels` and `view_width`? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-VIEW-023. RULE-VIEW-006: Where the game keeps `combat_palette` and `scene_scenario`? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-VIEW-024. RULE-VIEW-006: Can floating-point and integer palette blending disagree on inputs
  beyond SKIRMISH.PAL? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-VIEW-025. RULE-VIEW-007: How the step combines with the block's `color_family` to name one
  of the 128 maps? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-VIEW-026. RULE-VIEW-007: Where the game keeps `scene_scenario`? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
