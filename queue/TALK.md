# TALK

Next ID: Q-TALK-019

## Static

- Q-TALK-001. BUG-TALK-001: Is it true that any shipped script names variable 190? Settles it:
  trace the cited path and its callers through the boundary case; distinguish executable
  behavior from runtime or driver assumptions. Blocks: none.

- Q-TALK-002. FMT-TALK-001: Is it true that two records with the same node number ever occur?
  Settles it: trace the named record or file through its loader and every relevant consumer,
  using the cited findings as entry points; record field widths and unresolved aliases. Blocks:
  none.

- Q-TALK-003. FMT-TALK-002: What is the purpose of `unk_009`, `unk_049`, `unk_330` and
  `unk_344`? Settles it: trace the named record or file through its loader and every relevant
  consumer, using the cited findings as entry points; record field widths and unresolved
  aliases. Blocks: none.

- Q-TALK-004. FMT-TALK-003: What `unk_00` holds? Settles it: trace the named record or file
  through its loader and every relevant consumer, using the cited findings as entry points;
  record field widths and unresolved aliases. Blocks: none.

- Q-TALK-005. FMT-TALK-005: What branch count kind 3 needs? Settles it: trace the named record
  or file through its loader and every relevant consumer, using the cited findings as entry
  points; record field widths and unresolved aliases. Blocks: none.

- Q-TALK-006. FMT-TALK-007: Does the action interpreter reject an argument list exceeding its
  buffer capacity? Settles it: trace the named record or file through its loader and every
  relevant consumer, using the cited findings as entry points; record field widths and
  unresolved aliases. Blocks: none.

- Q-TALK-007. FMT-TALK-008: What separates the kinds of each pair? Settles it: trace the named
  record or file through its loader and every relevant consumer, using the cited findings as
  entry points; record field widths and unresolved aliases. Blocks: none.

- Q-TALK-008. RULE-TALK-001: What `fn_0001A91C`, `fn_0001ACEC` and `fn_0001A3B8` do beyond
  showing the prompt, waiting and reading the choice, and what `flag` changes? Settles it: read
  the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-TALK-009. RULE-TALK-001: What `fn_0006B3B4` returns? Settles it: read the relevant branch
  and its callers from the entry's cited findings, following data provenance, call effects and
  every exit relevant to this question. Blocks: none.

- Q-TALK-010. RULE-TALK-001: What the dwords at `0x330` and `0x344` of a node are for
  (FMT-TALK-002)? Settles it: read the relevant branch and its callers from the entry's cited
  findings, following data provenance, call effects and every exit relevant to this question.
  Blocks: none.

- Q-TALK-012. RULE-TALK-003: Which item each entry of `script_item_slots` names? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-TALK-013. RULE-TALK-003: What happens when a redirect names a node missing from the index? Settles it:
  read the relevant branch and its callers from the entry's cited findings, following data
  provenance, call effects and every exit relevant to this question. Blocks: none.

- Q-TALK-014. RULE-TALK-003: What `args[0]` of functions 7 to 9 holds? Settles it: read the
  relevant branch and its callers from the entry's cited findings, following data provenance,
  call effects and every exit relevant to this question. Blocks: none.

- Q-TALK-015. RULE-TALK-004: What `fn_00059D50`, `fn_000596C0`, `fn_0002C250`, `g_0009DEA8` and
  `g_0009A934` do or hold beyond what is described here? Settles it: read the relevant branch
  and its callers from the entry's cited findings, following data provenance, call effects and
  every exit relevant to this question. Blocks: none.

- Q-TALK-016. RULE-TALK-004: What the two other writers of `conversation_partner`, at
  `0x00060D85` and `0x00060DE5`, are? Settles it: read the relevant branch and its callers from
  the entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-TALK-017. RULE-TALK-004: Where rumours come from: no code of their own was found, so they
  are presumably conversation content? Settles it: read the relevant branch and its callers from
  the entry's cited findings, following data provenance, call effects and every exit relevant to
  this question. Blocks: none.

- Q-TALK-018. RULE-TALK-005: What `fn_0003FCA8` does before the settlement, and what
  `fn_0002C20C` and `fn_0002C218` hold? Settles it: read the relevant branch and its callers
  from the entry's cited findings, following data provenance, call effects and every exit
  relevant to this question. Blocks: none.

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
