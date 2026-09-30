# ASSAULT

Next ID: Q-ASSAULT-089

## Static

- Q-ASSAULT-001. FMT-ASSAULT-001: Where are kind, template, combat row, live state, owning block
  and heading stored in the combatant record? Settles it: trace the named record or file through
  its loader and every relevant consumer, using the cited findings as entry points; record field
  widths and unresolved aliases. Blocks: none.

- Q-ASSAULT-002. FMT-ASSAULT-001: What does the player's dword at combatant offset 4 hold?
  Settles it: trace the named record or file through its loader and every relevant consumer,
  using the cited findings as entry points; record field widths and unresolved aliases. Blocks:
  none.

- Q-ASSAULT-003. FMT-ASSAULT-001: What is the purpose of `unk_00`, `unk_14` and `unk_38`?
  Settles it: trace the named record or file through its loader and every relevant consumer,
  using the cited findings as entry points; record field widths and unresolved aliases. Blocks:
  none.

- Q-ASSAULT-004. FMT-ASSAULT-002: What is the purpose of `unk_14`? Settles it: trace the named
  record or file through its loader and every relevant consumer, using the cited findings as
  entry points; record field widths and unresolved aliases. Blocks: none.

- Q-ASSAULT-006. FMT-ASSAULT-003: What is the purpose of `unk_00`, `unk_10`, `unk_24` and the
  flag bits other than `0x10` and `0x40`? Settles it: trace the named record or file through its
  loader and every relevant consumer, using the cited findings as entry points; record field
  widths and unresolved aliases. Blocks: none.

- Q-ASSAULT-007. FMT-ASSAULT-004: Which offsets hold the constructor's copied descriptor fields,
  tick counter and owner? Settles it: trace the named record or file through its loader and
  every relevant consumer, using the cited findings as entry points; record field widths and
  unresolved aliases. Blocks: none.

- Q-ASSAULT-008. FMT-ASSAULT-004: Do the scheduler's offsets +0x24 and +0x28 belong to the
  effect record or the combatant record? Settles it: trace the named record or file through its
  loader and every relevant consumer, using the cited findings as entry points; record field
  widths and unresolved aliases. Blocks: none.

- Q-ASSAULT-009. RULE-ASSAULT-001: What does `fn_00029E58` read, and what is its argument order?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-010. RULE-ASSAULT-001, RULE-ASSAULT-003: Where `retainer_shares` is kept? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-011. RULE-ASSAULT-001: Which assault does the caller passing a count divided by 50
  initialize? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-012. RULE-ASSAULT-002: How the loader derives `side` from the block's colour group,
  and how it gives each combatant its own live block,? Settles it: read the relevant branch and
  its callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-013. RULE-ASSAULT-002: Which health, skill and armor values does the template
  initializer assign? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-ASSAULT-014. RULE-ASSAULT-002: Which retainer `prune_one_retainer` removes? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-015. RULE-ASSAULT-002: Where `combatants`, `scene_blocks` and `scene_map` are kept?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-016. RULE-ASSAULT-003: What arguments and effects do `fn_00029E58`, `fn_00029E80`
  and `fn_00029D24` have on assault settlement? Settles it: read the relevant branch and its
  callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-017. RULE-ASSAULT-003: Is it true that an army with other unit types left is also
  removed when the first three reach 0 is not recorded? Settles it: read the relevant branch and
  its callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-018. RULE-ASSAULT-004: Which mode value do the order handlers install, and how do
  Defend and Follow consume it? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ASSAULT-019. RULE-ASSAULT-004: Which result the Attack and Retreat handlers pass to the
  helper? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-020. RULE-ASSAULT-004, RULE-ASSAULT-025: Where `combatants` and `scene_blocks` are
  kept? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-021. RULE-ASSAULT-005: How are screen-pointer coordinates converted to the view's
  row and column? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-022. RULE-ASSAULT-005: How the ordered combatants' requested modes 12 and 8 are
  installed, since the pointer path does not call the transition helper,? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-023. RULE-ASSAULT-005: Is it true that the weapon path compares the depth with the
  reach plus `0x40` strictly, and in what order it starts the swing, applies the damage and
  starts the blood effect, is not recorded? Settles it: read the relevant branch and its callers
  from the entry's cited findings, following data provenance, call effects and every exit
  relevant to this question. Blocks: none.

- Q-ASSAULT-024. RULE-ASSAULT-005: When does the crossbow consume or reject ammunition on the
  pointer-attack path? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-ASSAULT-025. RULE-ASSAULT-005: Is it true that a block marked `weapon_contact` also starts
  the swing, and whether the weapon path reaches it for a block marked `actionable` as well, is
  not recorded? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-026. RULE-ASSAULT-005: Where `scene_blocks`, `scene_map`, `view_heading`, `view_x`
  and `view_y` are kept? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-ASSAULT-027. RULE-ASSAULT-006: Which index does the retainer cursor wrap to? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-028. RULE-ASSAULT-006: Is it true that the count `visits` has a lower limit of 1 is
  not recorded? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-029. RULE-ASSAULT-006, RULE-ASSAULT-012, RULE-ASSAULT-013, RULE-ASSAULT-023,
  RULE-ASSAULT-031: Where `combatants` is kept? Settles it: read the relevant branch and its
  callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-030. RULE-ASSAULT-007: Which condition removes a combatant after its death effect
  finishes? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-031. RULE-ASSAULT-007: What counts as a change of state for `take_transition`, and
  so when the previous effect is cancelled,? Settles it: read the relevant branch and its
  callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-032. RULE-ASSAULT-007: How the routine treats the mode value the retainer orders
  reset to (RULE-ASSAULT-004)? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ASSAULT-033. RULE-ASSAULT-008: What transitions do the mode-table entries marked unread in
  RULE-ASSAULT-008 perform? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ASSAULT-034. RULE-ASSAULT-008: What does `fn_0004F89B` test when choosing mode 11 or mode
  13? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-035. RULE-ASSAULT-009: What the scan reads for a cell outside the map? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-036. RULE-ASSAULT-009, RULE-ASSAULT-021: Where `combatants` and `scene_map` are
  kept? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-037. RULE-ASSAULT-010, RULE-ASSAULT-012, RULE-ASSAULT-013, RULE-ASSAULT-020: Which view row does actor acquisition use? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-038. RULE-ASSAULT-010: Where `combatants` and `hit_depth` are kept? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-040. RULE-ASSAULT-013: How does `fn_0004F89B` pass its test result to the hit
  handlers? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-041. RULE-ASSAULT-013: Which cell `at_destination` compares, the one under the live
  position or the one the actor occupies,? Settles it: read the relevant branch and its callers
  from the entry's cited findings, following data provenance, call effects and every exit
  relevant to this question. Blocks: none.

- Q-ASSAULT-042. RULE-ASSAULT-014: Which sentinel does the wandering handler write when it
  clears the target fields? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ASSAULT-043. RULE-ASSAULT-014: Where `effect_defs` and `scene_blocks` are kept? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-044. RULE-ASSAULT-015: Which sentinel does the go-to handler write when it clears
  `target`? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-045. RULE-ASSAULT-015: Which cell the go-to handler subtracts, the one under the
  live position or the one the actor occupies,? Settles it: read the relevant branch and its
  callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-046. RULE-ASSAULT-015, RULE-ASSAULT-016: Where `combatants`, `effect_defs` and
  `scene_blocks` are kept? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-ASSAULT-047. RULE-ASSAULT-016: Which x87 rounding mode is active during movement-step
  conversion? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-048. RULE-ASSAULT-016: Does the handler write the whole flags value 0x112 or replace
  only its low byte? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-049. RULE-ASSAULT-017: In which order does the scheduler test axes, and does a
  refused axis prevent the other from moving? Settles it: read the relevant branch and its
  callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-050. RULE-ASSAULT-017: Is it true that the scheduler writes `x` and `y` at each tick
  is not recorded? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-051. RULE-ASSAULT-017: What the neighbouring-cell lookup reads outside the map?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-052. RULE-ASSAULT-017, RULE-ASSAULT-019: Where `scene_blocks` and `scene_map` are
  kept? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-053. RULE-ASSAULT-018: Which clock supplies `now`, and at what resolution? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-054. RULE-ASSAULT-018: Is it true that the comparison with the interval is signed or
  unsigned is not recorded? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ASSAULT-055. RULE-ASSAULT-018: Which offsets and widths hold the remaining effect-record
  fields? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-056. RULE-ASSAULT-018: What the constructor does when no slot is free? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-057. RULE-ASSAULT-018: Where `combatants` and `effects` are kept? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-058. RULE-ASSAULT-019: Which record owns the scheduler's +0x24 and +0x28 fields?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-059. RULE-ASSAULT-020: What the handler does when the ray hits no combatant? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-060. RULE-ASSAULT-020: Where `combatants`, `effect_defs`, `hit_depth` and
  `scene_blocks` are kept? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-ASSAULT-061. RULE-ASSAULT-021: Does one click dispatch its action before or after map
  replacement? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-062. RULE-ASSAULT-021: What do the remaining interaction dispatcher cases do?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-063. RULE-ASSAULT-021: Which feedback callback runs after an accepted action, and
  what does it do? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-064. RULE-ASSAULT-021: Is it true that food adds to the player's combatant health or
  to another copy of it is not recorded? Settles it: read the relevant branch and its callers
  from the entry's cited findings, following data provenance, call effects and every exit
  relevant to this question. Blocks: none.

- Q-ASSAULT-065. RULE-ASSAULT-022: How a combatant that survives a hit enters mode 14? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-066. RULE-ASSAULT-022: How the comparison in the hit handlers reaches the next
  decision, and what else the mode-15 handler does,? Settles it: read the relevant branch and
  its callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-067. RULE-ASSAULT-022: Is it true that the hostile count skips the ten templates is
  not recorded? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-068. RULE-ASSAULT-022: Where `hostiles_left` is kept? Settles it: read the relevant
  branch and its callers from the entry's cited findings, following data provenance, call
  effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-069. RULE-ASSAULT-022: Where `combatants`, `effect_defs`, `scene_blocks` and
  `scene_map` are kept? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-ASSAULT-070. RULE-ASSAULT-023: Is it true that the reach comparison is strict is not
  recorded? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-071. RULE-ASSAULT-023: How "within one cell of the player" is measured? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-072. RULE-ASSAULT-023: Is it true that a hit with 0 damage still reaches `apply_hit`
  is not recorded? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-073. RULE-ASSAULT-024: Is `max_health` initialized from the same sum as current
  health? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-074. RULE-ASSAULT-024: Where `character_honour`, `character_sword_experience` and
  `combatants` are kept? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-ASSAULT-075. RULE-ASSAULT-025: Which colour each value of `player_color` is? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-076. RULE-ASSAULT-025: Is it true that the loader writes the actors' own blocks or
  their base blocks is not recorded? Settles it: read the relevant branch and its callers from
  the entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ASSAULT-077. RULE-ASSAULT-026: How does `swing_side_x` select and mirror the swing's
  starting side? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-078. RULE-ASSAULT-026: Which side of the `H / 3` line counts as below it? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-079. RULE-ASSAULT-026: In what order are the two random values drawn? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-080. RULE-ASSAULT-026: Where `combatants`, `swing_frame_height`,
  `swing_frame_width`, `swing_start_x`, `swing_start_y`, `swing_target_x`, `swing_target_y`,
  `swing_vx`, `swing_vy`, `swing_x` and `swing_y` are kept? Settles it: read the relevant branch
  and its callers from the entry's cited findings, following data provenance, call effects and
  every exit relevant to this question. Blocks: none.

- Q-ASSAULT-081. RULE-ASSAULT-027: Which combatant offsets hold kind, side, template and combat
  row? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ASSAULT-082. RULE-ASSAULT-027: Which effect-record offsets hold interval, flags, steps,
  ticks and owner? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ASSAULT-083. RULE-ASSAULT-027: How `0x0004CEA0` resolves a block to a combatant? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-084. RULE-ASSAULT-028: Where `blood_frame` and `blood_step` are kept? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-085. RULE-ASSAULT-029: Which fields the copier takes from the state block apart from
  the texture at `surface0`, and which it keeps apart from `color_family`,? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-086. RULE-ASSAULT-029: Where `scene_blocks` is kept? Settles it: read the relevant
  branch and its callers from the entry's cited findings, following data provenance, call
  effects and every exit relevant to this question. Blocks: none.

- Q-ASSAULT-087. RULE-ASSAULT-030: How the draw picks the retainer, how many draws a removal
  makes, and how the removed combatant leaves the map? Settles it: read the relevant branch and
  its callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-ASSAULT-088. RULE-ASSAULT-031: Which outcomes count as a miss, and what the game does with a
  broken weapon? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

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
