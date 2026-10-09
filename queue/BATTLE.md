# BATTLE

Next ID: Q-BATTLE-025

## Static

- Q-BATTLE-002. FMT-BATTLE-001: What reads `value`, if anything? Settles it: trace the named
  record or file through its loader and every relevant consumer, using the cited findings as
  entry points; record field widths and unresolved aliases. Blocks: none.

- Q-BATTLE-003. FMT-BATTLE-002: What is the purpose of `unk_10`? Settles it: trace the named
  record or file through its loader and every relevant consumer, using the cited findings as
  entry points; record field widths and unresolved aliases. Blocks: none.

- Q-BATTLE-004. RULE-BATTLE-001: What the gate `0x000256FC` returns when the player leaves it
  without picking a region, and how it reads the pointer? Settles it: read the relevant branch
  and its callers from the entry's cited findings, following data provenance, call effects and
  every exit relevant to this question. Blocks: none.

- Q-BATTLE-005. RULE-BATTLE-001: What the callers at `0x0002A80B` and `0x00042363` are? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-BATTLE-006. RULE-BATTLE-001: How the resolver draws the backdrop in each display mode?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-BATTLE-007. RULE-BATTLE-001: What an automatic battle with a foe total of 0 does on the
  original machine? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-BATTLE-008. RULE-BATTLE-002: What `x` and `y` hold for the player's units before a formation
  writes them, for a code above 3? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-BATTLE-009. RULE-BATTLE-002: What the four formations look like on the choice screen?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-BATTLE-010. RULE-BATTLE-003: What `battle_last_pass` holds when the first battle of a
  session starts? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-BATTLE-011. RULE-BATTLE-005: What the three bytes that allow `battle_contact_filter` to
  change hold (RULE-BATTLE-009)? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-BATTLE-012. RULE-BATTLE-005, RULE-BATTLE-008: What `fn_0005B3B0` does beyond what the
  findings record? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-BATTLE-013. RULE-BATTLE-005, RULE-BATTLE-008: What `g_000A9C80` does beyond what the
  findings record? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-BATTLE-014. RULE-BATTLE-007: Is it true that `0x00063EC0` makes the distance truncate, as
  the procedure assumes? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-BATTLE-015. RULE-BATTLE-009: What `g_000B0C36`, `g_000B0C2A` and `g_000B0C38` hold? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-BATTLE-016. RULE-BATTLE-009: What the caller of the resolver does with `map_session_over`
  after Alt+X? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-BATTLE-017. RULE-BATTLE-010: When the game registers the 250 Hz callback, and whether it
  ever removes it? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-BATTLE-019. RULE-BATTLE-010: Does the timer condition read the same baseline that the
  eligible pass resets? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-BATTLE-020. RULE-BATTLE-010: Where the game keeps `bios_chain_accumulator`? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-BATTLE-021. RULE-BATTLE-011: What orders two units whose depth keys compare equal? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-BATTLE-022. RULE-BATTLE-011: How the backdrop and the control strip are drawn around the
  units? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-BATTLE-023. RULE-BATTLE-012: Is it true that the callback discards an event or overwrites
  one when the queue is full? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-BATTLE-024. RULE-BATTLE-012: When `pointer_clock` starts counting? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

- Q-BATTLE-001. BUG-BATTLE-001: Is it true that the designers meant a third? Settles it: a
  contemporary design note, erratum or author statement addressing the intent; executable
  behavior alone does not prove intent. Blocks: none.

## Blocked

None.
