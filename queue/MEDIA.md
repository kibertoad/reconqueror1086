# MEDIA

Next ID: Q-MEDIA-012

## Static

- Q-MEDIA-001. FMT-MEDIA-003: Is it true that the palette values are sent to the display as they
  are or scaled? Settles it: trace the named record or file through its loader and every
  relevant consumer, using the cited findings as entry points; record field widths and
  unresolved aliases. Blocks: none.

- Q-MEDIA-002. FMT-MEDIA-005: What is the layout of the 9,216-byte FNT6.PCX resource? Settles
  it: trace the named record or file through its loader and every relevant consumer, using the
  cited findings as entry points; record field widths and unresolved aliases. Blocks: none.

- Q-MEDIA-003. RULE-MEDIA-002: How does the clipped drawing path apply the rectangle at
  0x000B0810? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-MEDIA-004. RULE-MEDIA-002: What the variant `0x00064524` adds with `0x0006E0B0` and
  `0x000633D0`? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-MEDIA-005. RULE-MEDIA-002, RULE-MEDIA-003: What `fn_0006DF90` does with the rectangle after
  it is recorded? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-MEDIA-006. RULE-MEDIA-003: How `palette_buffer` reaches the display, and whether its 8-bit
  values are scaled there? Settles it: read the relevant branch and its callers from the entry's
  cited findings, following data provenance, call effects and every exit relevant to this
  question. Blocks: none.

- Q-MEDIA-007. RULE-MEDIA-004: What `a` and `b` mean to `fn_00040CE4`? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-MEDIA-008. RULE-MEDIA-004: What loads `MELEE2.PCX`, which no caller of this routine names?
  Settles it: read the relevant branch and its callers from the entry's cited findings,
  following data provenance, call effects and every exit relevant to this question. Blocks:
  none.

- Q-MEDIA-009. RULE-MEDIA-005: What do input event codes 3 and 7 mean? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-MEDIA-010. RULE-MEDIA-005: What `fn_00072119` does with 1 and 3? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-MEDIA-011. RULE-MEDIA-005: How does the library interpret open flags 0x200 and 0x80? Settles
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
