---
id: RULE-TALK-005
title: Joust result for the conversations
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TALK-009, FND-TOURNEY-004, FND-TOURNEY-005, FND-PERSON-001, FND-PERSON-002, FND-TALK-011]
conflicting: []
split_with: []
related: [RULE-TALK-003, RULE-PERSON-001]
---

## Summary

The joust settles its stake and records the result in the wins and losses of both knights, and in
conversation variable 3: 2 after a win, 1 after a loss, for the scripts to read. No code outside
the scripts reads or writes variable 42; the lady scripts use it for whose colours the player
wears (FND-TALK-011). The joust gives no item; the ladies' gifts come from their scripts.

## When it runs

When `fn_0003FCA8` ends a joust with a result of 0 or 1.

## Parameters

`opponent`, the opponent's row; `stake`; `result`, 1 for a win and 0 for a loss.

## Inputs

None.

## Procedure

```text
define settle_joust(opponent, stake, result):
    if result == 1:
        fn_0002C218(fn_0002C20C() + stake)
        set_attr(0, 17, attr(0, 17) + stake)
        set_attr(0, 20, attr(0, 20) + 1)
        set_attr(opponent, 21, attr(opponent, 21) + 1)
        set_variable(3, 2)
    else if result == 0:
        fn_0002C218(fn_0002C20C() - stake)
        set_attr(0, 17, attr(0, 17) - stake)
        set_attr(0, 21, attr(0, 21) + 1)
        set_attr(opponent, 20, attr(opponent, 20) + 1)
        set_variable(3, 1)
```

## Outputs

WEALTH, JOUST_WON and JOUST_LOST of both knights, and variable 3.

## Edge cases

A result other than 0 or 1 changes nothing here.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_0003FCA8` does before the settlement, and what `fn_0002C20C` and `fn_0002C218` hold.
