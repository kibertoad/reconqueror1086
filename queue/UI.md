# UI

Next ID: Q-UI-044

## Static

- Q-UI-001. BUG-UI-001: Is it true that the runtime library of the original stops at the space
  as the standard C library does? Settles it: trace the cited path and its callers through the
  boundary case; distinguish executable behavior from runtime or driver assumptions. Blocks:
  none.

- Q-UI-002. FMT-UI-001: What is the purpose of `unk_25`, which the game never reads? Settles it:
  trace the named record or file through its loader and every relevant consumer, using the cited
  findings as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-UI-003. FMT-UI-001: Is it true that anything reads `origin_x`, `origin_y`, `width` and
  `height` after the loader stores them? Settles it: trace the named record or file through its
  loader and every relevant consumer, using the cited findings as entry points; record field
  widths and unresolved aliases. Blocks: none.

- Q-UI-004. FMT-UI-002: What reads `id` and `enabled` when the pointer moves? Settles it: trace
  the named record or file through its loader and every relevant consumer, using the cited
  findings as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-UI-005. FMT-UI-003: What the four digits after `_` in a background name mean? Settles it:
  trace the named record or file through its loader and every relevant consumer, using the cited
  findings as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-UI-006. FMT-UI-004: Is it true that the game reads the descriptions of the owned items for
  anything besides display? Settles it: trace the named record or file through its loader and
  every relevant consumer, using the cited findings as entry points; record field widths and
  unresolved aliases. Blocks: none.

- Q-UI-007. RULE-UI-001, RULE-UI-002: What calendar field `fn_00038678` returns? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-UI-008. RULE-UI-001: What `fn_0005B2C0`, `g_0009AAC4`, `g_0009DED8`, `g_0009DEF8` and
  `g_0009DF04` are? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-UI-009. RULE-UI-001: What the four digits after `_` in a background name mean, and whether
  the game checks them? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-UI-010. RULE-UI-002: What `fn_0005B2C0`, `g_0009AAC4`, `g_0009DED8`, `g_0009DEF8`,
  `g_0009DEDC` and `g_0009DF04` do on the screens that follow? Settles it: read the relevant
  branch and its callers from the entry's cited findings, following data provenance, call
  effects and every exit relevant to this question. Blocks: none.

- Q-UI-011. RULE-UI-003: How the store screen uses the owned entries after `store_offer_count`?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-UI-012. SCR-UI-002: When the byte at `0x0009ADFC` is 1, and what shows region 11's frame?
  Settles it: follow the screen entry, input handlers and drawing paths named by the cited
  findings, retaining branch conditions and resource references. Blocks: none.

- Q-UI-013. SCR-UI-004: Which of regions 3 and 4 scrolls up? Settles it: follow the screen
  entry, input handlers and drawing paths named by the cited findings, retaining branch
  conditions and resource references. Blocks: none.

- Q-UI-014. SCR-UI-006: Is it true that the background drawn is the catalog record's picture or
  `TOWN_Y.PCX`? Settles it: follow the screen entry, input handlers and drawing paths named by
  the cited findings, retaining branch conditions and resource references. Blocks: none.

- Q-UI-015. SCR-UI-007: Where the description window is placed? Settles it: follow the screen
  entry, input handlers and drawing paths named by the cited findings, retaining branch
  conditions and resource references. Blocks: none.

- Q-UI-016. SCR-UI-008: How the army sizes of the practice battle are drawn? Settles it: follow
  the screen entry, input handlers and drawing paths named by the cited findings, retaining
  branch conditions and resource references. Blocks: none.

- Q-UI-017. SCR-UI-010: Which frame shows which army state? Settles it: follow the screen entry,
  input handlers and drawing paths named by the cited findings, retaining branch conditions and
  resource references. Blocks: none.

- Q-UI-018. SCR-UI-011: What the row and terrain routines change, and what `0x00034F20` computes
  from the tax? Settles it: follow the screen entry, input handlers and drawing paths named by
  the cited findings, retaining branch conditions and resource references. Blocks: none.

- Q-UI-019. SCR-UI-012: Which tile set the game picks, and when? Settles it: follow the screen
  entry, input handlers and drawing paths named by the cited findings, retaining branch
  conditions and resource references. Blocks: none.

- Q-UI-020. SCR-UI-012: What sets `0x0009AEF0` and `0x0009AEF4`? Settles it: follow the screen
  entry, input handlers and drawing paths named by the cited findings, retaining branch
  conditions and resource references. Blocks: none.

- Q-UI-021. SCR-UI-012: What the right-button report `0x0001265C` offers for the selected army?
  Settles it: follow the screen entry, input handlers and drawing paths named by the cited
  findings, retaining branch conditions and resource references. Blocks: none.

- Q-UI-022. SCR-UI-013: Which keys pick a response? Settles it: follow the screen entry, input
  handlers and drawing paths named by the cited findings, retaining branch conditions and
  resource references. Blocks: none.

- Q-UI-023. SCR-UI-013: What the name window shows? Settles it: follow the screen entry, input
  handlers and drawing paths named by the cited findings, retaining branch conditions and
  resource references. Blocks: none.

- Q-UI-024. SCR-UI-014: What region 1 covers and why it has no routine? Settles it: follow the
  screen entry, input handlers and drawing paths named by the cited findings, retaining branch
  conditions and resource references. Blocks: none.

- Q-UI-025. SCR-UI-018: What the three report routines show, and how the player leaves them?
  Settles it: follow the screen entry, input handlers and drawing paths named by the cited
  findings, retaining branch conditions and resource references. Blocks: none.

- Q-UI-026. SCR-UI-019: What the entry routine `0x00019A8C` plays or shows? Settles it: follow
  the screen entry, input handlers and drawing paths named by the cited findings, retaining
  branch conditions and resource references. Blocks: none.

- Q-UI-027. SCR-UI-020: Which routine switches to screen 5, and what it shows? Settles it:
  follow the screen entry, input handlers and drawing paths named by the cited findings,
  retaining branch conditions and resource references. Blocks: none.

- Q-UI-028. SCR-UI-021: Which code draws the practice jousting screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-029. SCR-UI-022: Which code draws the moneylender screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-030. SCR-UI-023: Which code draws the store item screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-031. SCR-UI-024: Which code draws the church screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-032. SCR-UI-025: Which code draws the tournament joust screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-033. SCR-UI-026: Which code draws the melee screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-034. SCR-UI-027: Which code draws the political status screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-035. SCR-UI-028: Which code draws the economics screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-036. SCR-UI-029: Which code draws the personal status screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-037. SCR-UI-030: Which code draws the orders screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-038. SCR-UI-031: Which code draws the overview map screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-039. SCR-UI-032: Which code draws the tactical map screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-040. SCR-UI-033: Which code draws the help screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-041. SCR-UI-034: Which code draws the choose formation screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-042. SCR-UI-035: Which code draws the field battle screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

- Q-UI-043. SCR-UI-036: Which code draws the castle skirmish screen, from which resources, with which regions and keys?
  Settles it: locate the screen's drawing and input code from the resources and handlers of the screens it is reached from. Blocks: Survey screen reconciliation.

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
