# SAVE

Next ID: Q-SAVE-009

## Static

- Q-SAVE-001. BUG-SAVE-001: Is it true that normal play can reach the save screen before
  `temp.jap` is written? Settles it: trace the cited path and its callers through the boundary
  case; distinguish executable behavior from runtime or driver assumptions. Blocks: none.

- Q-SAVE-002. BUG-SAVE-002: How the font draws the control characters? Settles it: trace the
  cited path and its callers through the boundary case; distinguish executable behavior from
  runtime or driver assumptions. Blocks: none.

- Q-SAVE-003. FMT-SAVE-001: What the `ictemp*.jp` files and `temp.jap` hold? Settles it: trace
  the named record or file through its loader and every relevant consumer, using the cited
  findings as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-SAVE-004. FMT-SAVE-002: What `unk_AB0C8`, `unk_AB0C0`, `unk_A7540` and `unk_AB170` hold?
  Settles it: trace the named record or file through its loader and every relevant consumer,
  using the cited findings as entry points; record field widths and unresolved aliases. Blocks:
  none.

- Q-SAVE-005. FMT-SAVE-004: What the rest of the block holds, the year included? Settles it:
  trace the named record or file through its loader and every relevant consumer, using the cited
  findings as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-SAVE-006. FMT-SAVE-005: What the nodes of each chain are? Settles it: trace the named record
  or file through its loader and every relevant consumer, using the cited findings as entry
  points; record field widths and unresolved aliases. Blocks: none.

- Q-SAVE-007. RULE-SAVE-001: How the slot picked by the pointer is read? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-SAVE-008. RULE-SAVE-002: Is it true that the calendar, estate and tournament blocks always
  exist once a campaign runs? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

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
