# ESTATE

Next ID: Q-ESTATE-023

## Static

- Q-ESTATE-002. RULE-ESTATE-001: What `fn_00063050`, `fn_000386CC` and `fn_000386A0` do when the
  screen opens, and what `fn_00059760` does when it closes? Settles it: read the relevant branch
  and its callers from the entry's cited findings, following data provenance, call effects and
  every exit relevant to this question. Blocks: none.

- Q-ESTATE-003. RULE-ESTATE-001: Is it true that `planning_joined` is set anywhere else, and
  what reads it? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ESTATE-004. RULE-ESTATE-001: Is it true that any army record ever holds a count in unit rows
  3 to 5, and whether anything reads `brigand_orders[0].unk_00` after the opening copy writes
  it? Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-ESTATE-005. RULE-ESTATE-001: What are the initial values of the estate fields the entry
  leaves unspecified? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-ESTATE-006. RULE-ESTATE-002: What `fn_0002DE60`, `fn_0002D9E8`, `fn_0002DA48`,
  `fn_0002E588`, `fn_0002E628` and `fn_0002E150` do each month, and `fn_0002E3B8`,
  `fn_0002DEAC`, `fn_0002DDFC`, `fn_0002DD3C` and `fn_0002DD94` in July? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-007. RULE-ESTATE-002: What is the layout of the estate definition rows? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-008. RULE-ESTATE-002: What `fn_0002CCD4` does beyond the message, and what
  `fn_0005C550` and `fn_0005C5CC` do? Settles it: read the relevant branch and its callers from
  the entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ESTATE-009. RULE-ESTATE-002: What `fiefs[0]` field `+0x14`, which `0x0002D5D4` reads capped
  at 100, holds? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ESTATE-010. RULE-ESTATE-003: Where the loan is taken, and whether the executable enforces
  the 20 to 200 range the manual gives? Settles it: read the relevant branch and its callers
  from the entry's cited findings, following data provenance, call effects and every exit
  relevant to this question. Blocks: none.

- Q-ESTATE-011. RULE-ESTATE-003: What conversation variables 1 and 6 of `g_0009A928` hold, and
  what `fn_000628AC` does with them? Settles it: read the relevant branch and its callers from
  the entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ESTATE-012. RULE-ESTATE-003: What `fn_00059CDC`, `fn_0003CED8` and `fn_0001070C` do around
  the fight, and what `fn_000106C0` and `fn_0001BF54` do when the player dies? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-013. RULE-ESTATE-004: How the executable computes productivity? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-014. RULE-ESTATE-004: Is it true that the order effect the guide reports is real, and
  what causes it? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ESTATE-015. RULE-ESTATE-004: Is it true that productivity is limited to 0 to 100? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-016. RULE-ESTATE-004: Where the executable keeps `fief_improvements`,
  `improvement_bonus`, `fief_has_staff`, `fief_has_servant_room`, `fief_bean_tiles`,
  `fief_food_tiles` and `fief_houses`? Settles it: read the relevant branch and its callers from
  the entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-ESTATE-017. RULE-ESTATE-005: How the executable computes revenue from productivity? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-018. RULE-ESTATE-005: Which of the fief record's lists hold the crops and the forest
  industries? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-ESTATE-019. RULE-ESTATE-005: How `scale_by_productivity` grows a value above 50%? Settles
  it: read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-020. RULE-ESTATE-005: Where the executable keeps `fief_crops`, `fief_forest`,
  `crop_monthly_revenue`, `crop_harvest_revenue` and `forest_revenue`? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-021. RULE-ESTATE-006: How the executable computes growth and taxes? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-ESTATE-022. RULE-ESTATE-006: Where the executable keeps `fief_houses`, `fief_food_tiles`,
  `fief_tax_rate` and the bands of `growth_band_rate`, and how `scale_by_productivity` scales
  the rate? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

- Q-ESTATE-001. BUG-ESTATE-001: Is it true that the refund was meant only for companies raised
  on this screen? Settles it: a contemporary design note, erratum or author statement addressing
  the intent; executable behavior alone does not prove intent. Blocks: none.

## Blocked

None.
