# RES

Next ID: Q-RES-007

## Static

- Q-RES-001. FMT-RES-001: What, if anything, occupies the bytes before FNT6.PCX in SKIRMISH.RES?
  Settles it: trace the named record or file through its loader and every relevant consumer,
  using the cited findings as entry points; record field widths and unresolved aliases. Blocks:
  none.

- Q-RES-003. FMT-RES-002: What is the layout of compression kind 3? Settles it: trace the named
  record or file through its loader and every relevant consumer, using the cited findings as
  entry points; record field widths and unresolved aliases. Blocks: none.

- Q-RES-004. RULE-RES-001: What does `fn_000491D4` decode, and what do `fn_00048090`, `fn_0004830C` and
  `fn_00049124` produce beyond the formats FMT-RES-003 and FMT-RES-004 give? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-RES-005. RULE-RES-004: Is it true that any call loads a picture, layout, song or sound with
  mode 0? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-RES-006. RULE-RES-004: Is it true that the shipped game ever reads a `.LOW` file? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

- Q-RES-002. FMT-RES-002: What was `unk_24` intended to hold? Settles it: a contemporary design
  note, erratum or author statement addressing the intent; executable behavior alone does not
  prove intent. Blocks: none.

## Blocked

None.
