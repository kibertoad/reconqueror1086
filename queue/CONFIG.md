# CONFIG

Next ID: Q-CONFIG-009

## Static

- Q-CONFIG-001. BUG-CONFIG-001: Is it true that a DOS extender that does not map address 0 would
  stop the game here? Settles it: trace the cited path and its callers through the boundary
  case; distinguish executable behavior from runtime or driver assumptions. Blocks: none.

- Q-CONFIG-002. FMT-CONFIG-001: Which setup program writes `GOB`, `AUD_DRV`, `CD_PATH` and
  `WAR_MODE`? Settles it: trace the named record or file through its loader and every relevant
  consumer, using the cited findings as entry points; record field widths and unresolved
  aliases. Blocks: none.

- Q-CONFIG-003. RULE-CONFIG-001: How the search walks `PATH` after its first directory, and the
  flags of the existence test? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-CONFIG-004. RULE-CONFIG-001: Is it true that an empty file makes the list start at an
  unfilled node? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-CONFIG-005. RULE-CONFIG-003: What the four values at `0x0009CDBC` to `0x0009CDC8` are?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-CONFIG-006. RULE-CONFIG-003: What the block `slow_machine` skips draws? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-CONFIG-007. RULE-CONFIG-003: What `cyberman_present` asks the mouse driver? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-CONFIG-008. RULE-CONFIG-004: What table `fn_0006FF50` searches and under which name? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

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
