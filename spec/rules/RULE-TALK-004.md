---
id: RULE-TALK-004
title: Conversation entry and its aftermath
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TALK-007, FND-TALK-008, FND-STRATEGY-036, FND-TOURNEY-001, FND-PERSON-001, FND-PERSON-002, FND-PERSON-004, FND-PERSON-006, FND-UI-004]
conflicting: []
split_with: []
related: [RULE-TALK-001, RULE-TALK-003, RULE-STRATEGY-019, RULE-PERSON-001, RULE-PERSON-005]
---

## Summary

Clicking a lady in the tournament stands, a patron in the inn or the blacksmith sets the
conversation partner and opens the dialogue screen. The screen's hook starts the conversation at the
partner's root node. Inn patrons only talk while the inn is open to talk, which the monthly reset
closes, apart from one patron who always talks. When the conversation ends, variable 6 becomes the
money the player has borrowed, and a set marriage variable makes the player married to the lady it
belongs to; the hook then goes on to the map events of RULE-STRATEGY-019.

## When it runs

On a click in a region of the stands, the inn (`VINN.HAT`, screen 12) or the blacksmith, and when the dialogue screen (`FFDIALOG.HAT`, screen `0x18`) runs its hook.

## Parameters

None.

## Inputs

`fn_00059D50`, the region clicked; `stands_partners`, `inn_partners`, `conversation_roots`, `tournament_here` and the conversation variables.

## Procedure

```text
define talk_at_stands():
    conversation_partner = stands_partners[fn_00059D50()]
    fn_000596C0(0x18, 0, 1)

define talk_at_inn():
    conversation_partner = inn_partners[fn_00059D50()]
    if tournament_here != 0 or conversation_partner == 21:
        g_0009DEA8 = 1
        fn_000596C0(0x18, 0, 1)

define talk_to_blacksmith():
    conversation_partner = 7
    fn_000596C0(0x18, 0, 1)

define dialogue_hook():
    # the screen's palette and sound are set up before the conversation and after it
    converse(conversation_roots[conversation_partner], "all", 0)
    set_attr(0, 26, get_variable(6))
    g_0009A934 = 0
    let marriages = [22, 49, 88, 126, 167]
    let codes = [2, 3, 6, 4, 5]
    for k in 0..5:
        if get_variable(marriages[k]) != 0:
            if attr(0, 28) == 0:
                fn_0002C250(0, codes[k])
            set_attr(0, 28, codes[k])
    # the hook continues with strategic_actions of RULE-STRATEGY-019
```

## Outputs

MONEY_BORROWED (field 26) and MARRIED (field 28) of row 0.

## Edge cases

The partner is set even when the inn does not open the dialogue. Several marriage variables set at
once leave MARRIED at the code of the last in the list, and `fn_0002C250` runs only for the first.
The debt is copied after every conversation, whoever the partner is.

## What the sources say

SRC-MANUAL (p. 26) says a knight who marries can no longer court the other ladies; in the executable that is the scripts testing MARRIED through RULE-TALK-003.

## Differences between builds

None known.

## Open questions

- What `fn_00059D50`, `fn_000596C0`, `fn_0002C250`, `g_0009DEA8` and `g_0009A934` do
  or hold beyond what is described here.
- What the two other writers of `conversation_partner`, at `0x00060D85` and `0x00060DE5`, are.
- Where rumours come from: no code of their own was found, so they are presumably conversation
  content.
