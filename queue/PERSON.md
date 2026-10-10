# PERSON

Next ID: Q-PERSON-023

## Static

- Q-PERSON-022. RULE-PERSON-007: Which other callers write the calendar and
  March flags, and how do reroll and the calendar update control complete their
  lifecycle? Tried: FND-PERSON-012 records the age pass, startup constructor,
  loader flag initialization and the observed pre-input main-loop ordering;
  EXP-PERSON-001 checks the isolated branches and EXP-PERSON-002 observes the
  initial year. Settles it: bounded caller and store searches with independent
  positive controls, followed through each argument and state transition.
  Blocks: complete calendar/aging and reroll timing claims, not the verified
  initial-year discrepancy.

- Q-PERSON-001. FMT-PERSON-001: Is it true that anything removes the line end that `strncpy`
  copies into a name? Settles it: trace the named record or file through its loader and every
  relevant consumer, using the cited findings as entry points; record field widths and
  unresolved aliases. Blocks: none.

- Q-PERSON-002. FMT-PERSON-002: Does the parser read each section kind across all choices before
  moving to the next kind? Settles it: trace the named record or file through its loader and
  every relevant consumer, using the cited findings as entry points; record field widths and
  unresolved aliases. Blocks: none.

- Q-PERSON-003. FMT-PERSON-002: Which field the loader gives `NONE`, and how it turns names into
  fields? Settles it: trace the named record or file through its loader and every relevant
  consumer, using the cited findings as entry points; record field widths and unresolved
  aliases. Blocks: none.

- Q-PERSON-004. FMT-PERSON-002: What the arguments 3, 3, 2 and 5 of `0x00018B60` limit? Settles
  it: trace the named record or file through its loader and every relevant consumer, using the
  cited findings as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-PERSON-005. RULE-PERSON-001: Which code writes attributes without `set_attr`? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-PERSON-006. RULE-PERSON-001: How the executable turns a value into the rank words it shows,
  such as those on the pre-generated screen? Settles it: read the relevant branch and its
  callers from the entry's cited findings, following data provenance, call effects and every
  exit relevant to this question. Blocks: none.

- Q-PERSON-007. RULE-PERSON-002: Which file does `save_characters` write, and what state do its
  save and load paths preserve? Settles it: read the relevant branch and its callers from the
  entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-PERSON-008. RULE-PERSON-003: What `fn_000596C0(6, ...)` opens, and whether screen 6 runs
  `dub_knight`, which is registered as a callback at `0x00019A51`? Settles it: read the relevant
  branch and its callers from the entry's cited findings, following data provenance, call
  effects and every exit relevant to this question. Blocks: none.

- Q-PERSON-009. RULE-PERSON-003: What `fn_0005B2C0` and `g_0009AAC4` do here? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-PERSON-010. RULE-PERSON-003: Which items 9, 23, 32, 28 and 16 are? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-PERSON-011. RULE-PERSON-003: What the Choose Character Name control writes, and the limit of
  20 letters? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-PERSON-012. RULE-PERSON-003: Which region of the pre-generated screen each knight's shield
  is, beyond region 0? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-PERSON-013. RULE-PERSON-004: Which field does `unlisted_field` return for NONE? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-PERSON-014. RULE-PERSON-004: Which dilemma numbers, choices and outcomes
  `dilemma_item_grants` covers in each of the three handlers? Settles it: read the relevant
  branch and its callers from the entry's cited findings, following data provenance, call
  effects and every exit relevant to this question. Blocks: none.

- Q-PERSON-015. RULE-PERSON-004: What `fn_000596C0(6, ...)` opens, and what `fn_0005B2C0` and
  `g_0009AAC4` do here? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-PERSON-016. RULE-PERSON-005: How the lady conversations decide on colours, prizes and
  marriage? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-PERSON-017. RULE-PERSON-005: Which conditions and rewards does each lady's data specify?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-PERSON-018. RULE-PERSON-006: What writes AGE after dubbing? Settles it: read the relevant
  branch and its callers from the entry's cited findings, following data provenance, call
  effects and every exit relevant to this question. Blocks: none.

- Q-PERSON-019. RULE-PERSON-006: What `g_0009ADC0`, `g_0009AABC`, `fn_00024CA0` and `g_0009ADB8`
  hold or do here? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-PERSON-020. RULE-PERSON-006: What the routine does after the movie, what it returns, and
  what calls it? Settles it: read the relevant branch and its callers from the entry's cited
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
